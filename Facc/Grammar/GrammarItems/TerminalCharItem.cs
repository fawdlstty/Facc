using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Grammar.GrammarItems {
	class TerminalCharItem: IGrammarExprItem {
		public EbnfExprItemRepeatType RepeatType { get; set; } = EbnfExprItemRepeatType.Unknown;
		public string Suffix { get; set; } = "";
		public string Content { init; get; } = "";

		public string GetClearStrings () => $"Value{Suffix} = \"\";";

		public string GenerateTryParse2 () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			if (RepeatType.max_1 ()) {
				_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{						");
				_append ($"	char? _ch;																");
				_append ($"	Func<char, bool> _check{Suffix} = (_c)=> {_get_compare_char ()};		");
				_append ($"	if ((_ch = Parser.TryGetChar (_pos, _check{Suffix})).HasValue) {{	");
				_append ($"		Value{Suffix} = $\"{{_ch.Value}}\";									");
				_append ($"		yield return _pos + 1;												");
				_append ($"		Value{Suffix} = \"\";												");
				_append ("	}																		");
				if (RepeatType.min_0 ()) {
					_append ("	yield return _pos;													");
				}
				_append ("}																			");
			} else {
				_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{						");
				_append ($"	Value{Suffix} = \"\";													");
				_append ("	char? _ch;																");
				_append ("	int _pos0 = _pos;														");
				_append ($"	Func<char, bool> _check{Suffix} = (_c)=> {_get_compare_char ()};		");
				_append ($"	while ((_ch = Parser.TryGetChar (_pos0++, _check{Suffix})).HasValue)	");
				_append ($"		Value{Suffix} += _ch.Value;											");
				_append ($"	if (Value{Suffix}.Length > 0) {{										");
				_append ($"		yield return _pos + Value{Suffix}.Length;							");
				_append ($"		Value{Suffix} = \"\";												");
				_append ("	}																		");
				if (RepeatType.min_0 ()) {
					_append ("	yield return _pos;													");
				}
				_append ("}																			");
			}
			return _sb.ToString ();
		}

		public string IsValidExpr () => (RepeatType == EbnfExprItemRepeatType._0_to_1 || RepeatType == EbnfExprItemRepeatType._0_to_N) ? "true" : $"!string.IsNullOrEmpty (Value{Suffix})";

		public string PrintTree () => $"Console.WriteLine ($\"{{new string (' ', (_indent + 1) * 4)}}[{{Value{Suffix}}}]\");";

		public string MValue () => $"public string Value{Suffix} {{ get; set; }} = \"\";";

		public string LengthExpr () => $"Value{Suffix}.Length";

		private string _get_compare_char () {
			var _chars = new List<string> ();
			for (int i = 0; i < Content.Length; ++i) {
				if (Content [i] == '\\') {
					int _len = Content[i + 1] switch {
						'x' => 4,
						'u' => 6,
						_ => 2,
					};
					_chars.Add ($"{Content[i..(i + _len)]}");
					i += _len - 1;
				} else {
					_chars.Add ($"{Content[i]}");
				}
			}
			var _items = new List<ITItem> ();
			while (_chars.Count > 0) {
				if (_chars.Count >= 3 && _chars [1] == "-") {
					_items.Add (new TRangeItem { Value1 = _chars [0], Value2 = _chars [2] });
					_chars.RemoveRange (0, 3);
				} else {
					_items.Add (new TCharItem { Value = _chars[0] });
					_chars.RemoveAt (0);
				}
			}
			string _ret = string.Join (" || ", from p in _items select p.Gen ());
			return (_items.Count == 1 && _ret[0] == '(') ? _ret[1..^1] : _ret;
		}
	}



	interface ITItem {
		string Gen ();
	}

	class TCharItem: ITItem {
		public string Value { init; get; }
		public string Gen () => $"_c == '{Value}'";
	}

	class TRangeItem: ITItem {
		public string Value1 { init; get; }
		public string Value2 { init; get; }
		public string Gen () => $"(_c >= '{Value1}' && _c <= '{Value2}')";
	}
}
