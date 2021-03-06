using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Grammar.GrammarItems {
	class TerminalStringItem: IGrammarExprItem {
		public EbnfExprItemRepeatType RepeatType { get; set; } = EbnfExprItemRepeatType.Unknown;
		public string Suffix { get; set; } = "";
		public string Content { init; get; } = "";
		public bool Reverse { get; set; } = false;

		public string GetClearStrings () => $"Value{Suffix} = \"\";";

		public string GenerateTryParse2 () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{				");
			_append ($"	Parser.ErrorPos = _pos;											");
			if (Reverse) {
				_append ($"	int _len = Parser.MatchReverseString (_pos, \"{Content}\");	");
				_append ($"	Value{Suffix} = Parser.GetPartCode (_pos, _len);			");
				_append ($"	yield return _pos + Value{Suffix}.Length;					");
				_append ($"	Value{Suffix} = \"\";										");
			} else {
				_append ($"	if (Parser.TryMatchString (_pos, \"{Content}\")) {{			");
				_append ($"		Value{Suffix} = \"{Content}\";							");
				_append ($"		yield return _pos + Value{Suffix}.Length;				");
				_append ($"		Value{Suffix} = \"\";									");
				_append ("	}															");
			}
			if ((!Reverse) && RepeatType.min_0 ()) {
				_append ("	yield return _pos;											");
			}
			_append ("}																	");
			return _sb.ToString ();
		}

		public string IsValidExpr () {
			return (RepeatType.min_0 ()) ? "true" : $"!string.IsNullOrEmpty (Value{Suffix})";
		}

		public string PrintTree () => $"Console.WriteLine ($\"{{new string (' ', (_indent + 1) * 4)}}[{{Value{Suffix}}}]\");";

		public string MValue () => $"public string Value{Suffix} {{ get; set; }} = \"\";";

		public string LengthExpr () => $"Value{Suffix}.Length";
	}
}
