using System;
using System.Text;

namespace Facc.Grammar.GrammarItems {
	class NonTerminalItem: IGrammarExprItem {
		public EbnfExprItemRepeatType RepeatType { get; set; } = EbnfExprItemRepeatType.Unknown;
		public string Suffix { get; set; } = "";
		public string NonTerminalName { init; get; } = "";

		public string GetClearStrings () {
			if (RepeatType.max_N ()) {
				return $"Value{Suffix}.Clear ();";
			} else {
				return $"Value{Suffix} = null;";
			}
		}

		public string GenerateTryParse2 () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{		");
			_append ("	Parser.ErrorPos = _pos;									");
			_append ($"	var _o = new {NonTerminalName} {{ Parser = Parser }};	");
			_append ("	var _enum = _o.TryParse (_pos);							");
			_append ("	while (_enum.MoveNext ()) {								");
			if (RepeatType.max_N ()) {
				_append ($"		int _list_pos = Value{Suffix}.Count;			");
				_append ($"		Value{Suffix}.Add (_o);							");
				_append ("		yield return _enum.Current;						");
				_append ($"		var _enum1 = _try_parse{Suffix} (_enum.Current);");
				_append ("		while (_enum1.MoveNext ())						");
				_append ("			yield return _enum1.Current;				");
				_append ($"		Value{Suffix}.RemoveAt (_list_pos);				");
			} else {
				_append ($"		Value{Suffix} = _o;								");
				_append ("		yield return _enum.Current;						");
				_append ($"		Value{Suffix} = null;							");
			}
			_append ("	}														");
			if (RepeatType.min_0 ()) {
				_append ("	yield return _pos;									");
			}
			_append ("}															");
			return _sb.ToString ();
		}

		public string IsValidExpr () {
			if (RepeatType.max_N ()) {
				if (RepeatType.min_0 ()) {
					return "true";
				} else {
					return $"Value{Suffix}.Count > 0";
				}
			} else {
				return $"(Value{Suffix} != null && Value{Suffix}.IsValid ())";
			}
		}

		public string PrintTree () {
			if (RepeatType.max_N ()) {
				return $"foreach (var _val in Value{Suffix}) _val.PrintTree (_indent + 1);";
			} else {
				return $"Value{Suffix}.PrintTree (_indent + 1);";
			}
		}

		public string MValue () {
			if (RepeatType.max_N ()) {
				return $"public List<{NonTerminalName}> Value{Suffix} {{ get; set; }} = new List<{NonTerminalName}> ();";
			} else {
				return $"public {NonTerminalName} Value{Suffix} {{ get; set; }} = null;";
			}
		}

		public string LengthExpr () {
			if (RepeatType.max_N ()) {
				return $"(from p in Value{Suffix} select p.Length).Sum ()";
			} else {
				return $"(Value{Suffix} != null ? Value{Suffix}.Length : 0)";
			}
		}
	}
}
