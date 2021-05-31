using System;
using System.Text;

namespace Facc.Grammar.GrammarItems {
	class NonTerminalItem: IGrammarExprItem {
		public EbnfExprItemRepeatType RepeatType { get; set; } = EbnfExprItemRepeatType.Unknown;
		public string Suffix { get; set; } = "";
		public string NonTerminalName { init; get; } = "";

		public string GetClearStrings () => $"Value{Suffix} = null;";

		public string GenerateTryParse2 () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{		");
			_append ($"	Parser.ErrorPos = _pos;									");
			_append ($"	var _o = new {NonTerminalName} {{ Parser = Parser }};	");
			_append ($"	var _enum = _o.TryParse (_pos);							");
			_append ("	while (_enum.MoveNext ()) {								");
			_append ($"		Value{Suffix} = _o;									");
			_append ($"		yield return _enum.Current;							");
			_append ($"		Value{Suffix} = null;								");
			_append ("	}														");
			if (RepeatType.min_0 ()) {
				_append ("	yield return _pos;									");
			}
			_append ("}															");
			return _sb.ToString ();
		}

		public string IsValidExpr () => $"(Value{Suffix} != null && Value{Suffix}.IsValid ())";

		public string PrintTree () => $"Value{Suffix}.PrintTree (_indent + 1);";

		public string MValue ()=> $"public {NonTerminalName} Value{Suffix} {{ get; set; }} = null;";

		public string LengthExpr () => $"(Value{Suffix} != null ? Value{Suffix}.Length : 0)";
	}
}
