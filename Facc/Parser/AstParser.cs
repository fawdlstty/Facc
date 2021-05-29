using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Parser {
	public class AstParser {
		public static T Parse<T> (string code) where T : IAST, new () {
			var _parser = new AstParser (code);
			var _t = new T { Parser = _parser };
			var _enum = _t.TryParse (0);
			while (true) {
				if (_enum.MoveNext ()) {
					var _ast = _enum.Current;
					if (_ast == code.Length)
						return _t;
				}
			}
			throw new Exception ("解析失败");
		}

		private string Code { init; get; } = "";
		private AstParser (string code) => Code = code;



		public char? TryGetChar (int _pos, Func<char, bool> _check) {
			if (_pos >= Code.Length)
				return null;
			if (!_check (Code[_pos]))
				return null;
			return Code[_pos];
		}

		public bool TryMatchString (int _pos, string _s) {
			if (Code.Length < _pos + _s.Length)
				return false;
			return Code[_pos..(_pos + _s.Length)] == _s;
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
