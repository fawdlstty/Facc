﻿using Facc.Grammar.GrammarItems;
using System;
using System.IO;
using System.Linq;

namespace Facc.Grammar {
	public class AstGenerator {
		private string m_grammar { init; get; }
		private string m_path { init; get; }
		private string m_namespace { init; get; }
		public AstGenerator (string _grammar, string _path, string _namespace) => (m_grammar, m_path, m_namespace) = (_grammar, _path, _namespace);



		public void ClearPath () {
			if (Directory.Exists (m_path))
				Directory.Delete (m_path, true);
			Directory.CreateDirectory (m_path);
		}



		public void Generate () {
			string [] _ebnf_lines = m_grammar.Split (new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			bool _in_comment = false;
			foreach (var _ebnf_line0 in _ebnf_lines) {
				string _ebnf_line = _ebnf_line0.Trim ();
				int _p, _p1;
				if (_in_comment) {
					_p = _ebnf_line.IndexOf ("*/");
					if (_p == -1)
						continue;
					_ebnf_line = _ebnf_line[(_p + 2)..].Trim ();
					_in_comment = false;
				}
				_p = _ebnf_line.IndexOf ("//");
				_p1 = _ebnf_line.IndexOf ("/*");
				while (_p != -1 || _p1 != -1) {
					if (_p != -1 && _p1 != -1) {
						if (_p < _p1) {
							_p1 = -1;
						} else {
							_p = -1;
						}
					}
					if (_p != -1) {
						_ebnf_line = _ebnf_line[.._p];
					} else if (_p1 != -1) {
						int _p2 = _ebnf_line.IndexOf ("*/", _p + 2);
						if (_p2 == -1) {
							_ebnf_line = _ebnf_line[.._p1];
							_in_comment = true;
						} else {
							_ebnf_line = $"{_ebnf_line[.._p1]}{_ebnf_line[(_p2 + 2)..]}";
						}
					}
					_ebnf_line = _ebnf_line.Trim ();
					if (_ebnf_line == "")
						break;
					_p = _ebnf_line.IndexOf ("//");
					_p1 = _ebnf_line.IndexOf ("/*");
				}
				if (_ebnf_line == "")
					continue;
				_p = _ebnf_line.IndexOf ("::=");
				if (_p == -1) {
					Console.WriteLine ("解析错误：从ebnf表达式中无法找到元素[::=]。");
					Console.WriteLine ($"错误行：{_ebnf_line}");
					break;
				}
				string _id = _ebnf_line[0.._p].Trim ();
				string _expr = _ebnf_line[(_p+3)..].Trim ();
				//
				if (string.IsNullOrEmpty (_id))
					throw new Exception ("非终结符名称不可为空");
				if (_id [0] >= '0' && _id[0] <= '9')
					throw new Exception ("非终结符名称不可以数字开头");
				foreach (char _ch in _id) {
					if (!((_ch >= '0' && _ch <= '9') || (_ch >= 'a' && _ch <= 'z') || (_ch >= 'A' && _ch <= 'Z') || _ch == '_'))
						throw new Exception ($"非终结符【{_id}】名称中不允许出现符号【{_ch}】");
				}
				if (string.IsNullOrEmpty (_expr))
					throw new Exception ($"【{_id}】所指代的表达式不可为空");
				//
				string _class_name = $"{string.Join ("", from p in _id.Split ('_', StringSplitOptions.RemoveEmptyEntries) let q = p[0] select $"{((q >= 'a' && q <= 'z') ? (char) (q - 'a' + 'A') : q)}{p[1..]}")}AST";
				var _items = GrammarExprItems.ParseItems (_id, _class_name, ref _expr);
				File.WriteAllText (Path.Combine (m_path, $"{_class_name}.cs"), $@"//
// This file is automatically generated by Facc
// https://github.com/fawdlstty/Facc
//

using Facc.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace {m_namespace} {{
{_items.ClassCode ()}
}}
");
			}
		}
	}
}
