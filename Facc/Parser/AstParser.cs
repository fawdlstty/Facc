using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Parser {
	public class AstParser {
		public T Parse<T> (string code) where T : IAST, new () {
			m_code = code;
			var _t = new T { Parser = this };
			var _enum = _t.TryParse (0);
			while (_enum.MoveNext ()) {
				if (_enum.Current == code.Length) {
					m_error_pos = -1;
					return _t;
				}
			}
			return default;
		}

		private string m_code = "";

		private int m_error_pos = -1;
		public int ErrorPos { set => m_error_pos = Math.Max (value, m_error_pos); }
		public ParseError Error {
			get {
				if (m_error_pos == -1)
					throw new Exception ("No any error");
				int _line = 1;
				int _start_pos = 0, _end_pos = m_code.IndexOf ('\n') + 1;
				while (_end_pos < m_error_pos) {
					if (_end_pos == 0)
						return new ParseError (m_code, m_error_pos, _line, _start_pos);
					_start_pos = _end_pos;
					_end_pos = m_code.IndexOf ('\n', _start_pos) + 1;
					++_line;
				}
				return new ParseError (m_code, m_error_pos, _line, _start_pos);
			}
		}



		public char? TryGetChar (int _pos, Func<char, bool> _check) {
			if (_pos >= m_code.Length)
				return null;
			if (!_check (m_code[_pos]))
				return null;
			return m_code[_pos];
		}

		public bool TryMatchString (int _pos, string _s) {
			if (m_code.Length < _pos + _s.Length)
				return false;
			return m_code[_pos..(_pos + _s.Length)] == _s;
		}



		public bool TryReg (string _class_name, int _pos) {
			_class_name = $"{_class_name}@{_pos}";
			if (!m_set.ContainsKey (_class_name)) {
				m_set.Add (_class_name, 1);
			} else if (m_set [_class_name] >= 2) {
				return false;
			} else {
				m_set [_class_name]++;
			}
			return true;
		}

		public void UnReg (string _class_name, int _pos) {
			_class_name = $"{_class_name}@{_pos}";
			if (!m_set.ContainsKey (_class_name)) {
				throw new Exception ("解析错误");
			} else if (m_set [_class_name] > 1) {
				m_set [_class_name]--;
			} else {
				m_set.Remove (_class_name);
			}
		}

		private Dictionary<string, int> m_set = new Dictionary<string, int> ();
	}
}
