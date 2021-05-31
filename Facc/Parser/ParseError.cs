using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Parser {
	public class ParseError {
		public ParseError (string _code, int _error_pos, int _line, int _start_pos) {
			ErrorPos = _error_pos;
			Line = _line;
			LinePos = _error_pos - _start_pos;
			int _end_pos = _code.IndexOf ('\n', _start_pos);
			if (_end_pos == -1) {
				LineCode = _code[_start_pos..];
			} else {
				LineCode = _code[_start_pos.._end_pos];
				if (LineCode.Length > 0 && LineCode[^1] == '\r')
					LineCode = LineCode[..^1];
			}
			ErrorInfo = $"Invalid Character '{_code[_error_pos]}'";
		}

		public int ErrorPos { init; get; }
		public int Line { init; get; }
		public int LinePos { init; get; }
		public string LineCode { init; get; }
		public string ErrorInfo { init; get; }
	}
}
