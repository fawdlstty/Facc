//
// This file is automatically generated by Facc
// https://github.com/fawdlstty/Facc
//

using Facc.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facc.Example.ASTs {
	public class NumAST: IAST {
		// num ::= [0-9]+

		public AstParser Parser { init; get; }

		public IEnumerator<int> TryParse (int _pos) {
			if (!Parser.TryReg ("NumAST", _pos))
				yield break;
			var _0_enum = _try_parse_0 (_pos);
			while (_0_enum.MoveNext ()) {
				yield return _0_enum.Current;
			}
			Parser.UnReg ("NumAST", _pos);
		}


		IEnumerator<int> _try_parse_0 (int _pos) {
			Value_0 = "";
			char? _ch;
			int _pos0 = _pos;
			Func<char, bool> _check_0 = (_c)=> _c >= '0' && _c <= '9';
			while ((_ch = Parser.TryGetChar (_pos0++, _check_0)).HasValue)
				Value_0 += _ch.Value;
			if (Value_0.Length > 0) {
				yield return _pos + Value_0.Length;
				Value_0 = "";
			}
		}

		public bool IsValid () => !string.IsNullOrEmpty (Value_0);

		public void PrintTree (int _indent) {
			Console.WriteLine ($"{new string (' ', _indent * 4)}NumAST");
			Console.WriteLine ($"{new string (' ', (_indent + 1) * 4)}[{Value_0}]");
		}

		public int Length { get => Value_0.Length; }

		public string Value_0 { get; set; } = "";
	}
}
