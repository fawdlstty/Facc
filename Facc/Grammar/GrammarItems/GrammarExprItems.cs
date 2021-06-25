using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Grammar.GrammarItems {
	class GrammarExprItems: IGrammarExprItem {
		public string EbnfId { get; set; } = "";
		public string ClassName { get; set; } = "";
		public string Expr { get; set; } = "";
		private string m_suffix = "";
		public string Suffix {
			get => m_suffix;
			set {
				m_suffix = value;
				for (int i = 0; i < ChildItems.Count; ++i)
					ChildItems[i].Suffix = $"{m_suffix}_{i}";
			}
		}
		public EbnfExprItemRepeatType RepeatType { get; set; } = EbnfExprItemRepeatType.Unknown;
		public List<IGrammarExprItem> ChildItems { get; set; } = new List<IGrammarExprItem> ();
		public ExprItemListType ListType { get; set; } = ExprItemListType.Unknown;
		public bool InitRecognize { get; set; } = true;

		public static GrammarExprItems ParseItems (string _ebnf_id, string _class_name, ref string _expr) {
			var _items = new GrammarExprItems { EbnfId = _ebnf_id, ClassName = _class_name, Expr = _expr };
			bool _is_in_quot = false;

			// 处理终止符
			_expr = _expr.Trim ();
			if (_expr[0] != '(') {
				_items.RepeatType = EbnfExprItemRepeatType._1;
			} else {
				_is_in_quot = true;
				_expr = _expr[1..].Trim ();
			}

			while (!string.IsNullOrEmpty (_expr)) {
				if (_expr[0] == ')') {
					_expr = _expr [1..].Trim ();
					if (!_is_in_quot)
						throw new Exception ("'('、')' 字符必须一一对应");

					if (_expr.Length > 0) {
						_items.RepeatType = _expr[0] switch {
							'*' => EbnfExprItemRepeatType._0_to_N,
							'+' => EbnfExprItemRepeatType._1_to_N,
							'?' => EbnfExprItemRepeatType._0_to_1,
							_ => EbnfExprItemRepeatType._1,
						};
						if (!_items.RepeatType.is_1 ())
							_expr = _expr[1..].Trim ();
					} else {
						_items.RepeatType = EbnfExprItemRepeatType._1;
					}
					_items.Expr = _items.Expr[..(_items.Expr.Length - _expr.Length)].Trim ();
					return _items;
				}

				// 处理表达式
				if (_expr[0] == '[') {
					int _p = _expr.IndexOf (']');
					var _terminal = new TerminalCharItem { RepeatType = EbnfExprItemRepeatType._1, Content = _expr[1.._p] };
					if (_expr.Length > _p + 1) {
						_terminal.RepeatType = _expr[_p + 1] switch {
							'*' => EbnfExprItemRepeatType._0_to_N,
							'+' => EbnfExprItemRepeatType._1_to_N,
							'?' => EbnfExprItemRepeatType._0_to_1,
							_ => EbnfExprItemRepeatType._1
						};
					}
					_expr = _expr[(_p + 1)..].Trim ();
					if (!_terminal.RepeatType.is_1 ())
						_expr = _expr[1..].Trim ();
					_terminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_terminal);
				} else if (_expr[0] == '\'' || _expr[0] == '"') {
					int _p = _expr.IndexOf (_expr[0], 1);
					var _terminal = new TerminalStringItem { RepeatType = EbnfExprItemRepeatType._1, Content = _expr[1.._p] };
					if (_expr.Length > _p + 1) {
						_terminal.RepeatType = _expr[_p + 1] switch {
							'*' => EbnfExprItemRepeatType._0_to_N,
							'+' => EbnfExprItemRepeatType._1_to_N,
							'?' => EbnfExprItemRepeatType._0_to_1,
							_ => EbnfExprItemRepeatType._1
						};
					}
					_expr = _expr[(_p + 1)..].Trim ();
					if (!_terminal.RepeatType.is_1 ())
						_expr = _expr[1..].Trim ();
					_terminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_terminal);
				} else if (_expr[0] == '^') {
					if (_expr.Length == 1)
						throw new Exception ("表达式不规范");
					_expr = _expr[1..];
					if (_expr[0] == '[') {
						int _p = _expr.IndexOf (']');
						var _terminal = new TerminalCharItem { RepeatType = EbnfExprItemRepeatType._1, Content = _expr[1.._p] };
						_expr = _expr[(_p + 1)..].Trim ();
						_terminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
						_terminal.Reverse = true;
						_items.ChildItems.Add (_terminal);
					} else if (_expr[0] == '\'' || _expr[0] == '"') {
						int _p = _expr.IndexOf (_expr[0], 1);
						var _terminal = new TerminalStringItem { RepeatType = EbnfExprItemRepeatType._1, Content = _expr[1.._p] };
						_expr = _expr[(_p + 1)..].Trim ();
						_terminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
						_terminal.Reverse = true;
						_items.ChildItems.Add (_terminal);
					} else {
						throw new Exception ("表达式不规范");
					}
				} else if (_expr[0] == '(') {
					// range
					var _nonterminal_items = ParseItems ($"[part of] {_ebnf_id}", _items.ClassName, ref _expr);
					_nonterminal_items.InitRecognize = false;
					_expr = _expr.Trim ();
					_nonterminal_items.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_nonterminal_items);
				} else {
					// ID
					string _name = "";
					while (_expr.Length > 0) {
						char _ch = _expr [0];
						if (!((_ch >= '0' && _ch <= '9') || (_ch >= 'a' && _ch <= 'z') || (_ch >= 'A' && _ch <= 'Z') || _ch == '_'))
							break;
						_name += _ch;
						_expr = _expr [1..];
					}
					_name = $"{string.Join ("", from p in _name.Split ('_', StringSplitOptions.RemoveEmptyEntries) let q = p[0] select $"{((q >= 'a' && q <= 'z') ? (char) (q - 'a' + 'A') : q)}{p[1..]}")}AST";
					var _nonterminal = new NonTerminalItem { RepeatType = EbnfExprItemRepeatType._1, NonTerminalName = _name };
					if (_expr.Length > 0) {
						_nonterminal.RepeatType = _expr[0] switch {
							'*' => EbnfExprItemRepeatType._0_to_N,
							'+' => EbnfExprItemRepeatType._1_to_N,
							'?' => EbnfExprItemRepeatType._0_to_1,
							_ => EbnfExprItemRepeatType._1
						};
						if (!_nonterminal.RepeatType.is_1 ())
							_expr = _expr [1..];
					}
					_expr = _expr.Trim ();
					_nonterminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_nonterminal);
				}

				// 处理连接符
				if (string.IsNullOrEmpty (_expr))
					break;
				if (_expr [0] == '|') {
					if (_items.ListType == ExprItemListType.Unknown || _items.ListType == ExprItemListType.Any) {
						_items.ListType = ExprItemListType.Any;
					} else if (_expr [0] != ')') {
						throw new Exception ("列表类型不可变化");
					}
					_expr = _expr[1..].Trim ();
				} else {
					if (_items.ListType == ExprItemListType.Unknown || _items.ListType == ExprItemListType.All) {
						_items.ListType = ExprItemListType.All;
					} else if (_expr [0] != ')') {
						throw new Exception ("列表类型不可变化");
					}
				}
			}
			if (_items.ListType == ExprItemListType.Unknown)
				_items.ListType = ExprItemListType.All;
			return _items;
		}

		public void ProcessConstruct (bool _init = true) {
			// 递归处理，放前面的原因是，先处理好子节点，再处理当前节点
			foreach (var _child in ChildItems) {
				if (_child is GrammarExprItems _child_item)
					_child_item.ProcessConstruct (false);
			}

			// 调整当前节点结构
			if (RepeatType.is_1 ()) {
				//// 当前元素重复类型为1
				//// 如果子元素只有一个，并且重复类型不为1，那么与当前元素交换重复类型
				//if (ChildItems.Count == 1 && (!ChildItems[0].RepeatType.is_1 ())) {
				//	(RepeatType, ChildItems[0].RepeatType) = (ChildItems[0].RepeatType, RepeatType);
				//}
			} else if (RepeatType.max_N ()) {
				// 当前元素重复类型为0N或1N
				// 如果子元素不止一个，那么加入中间元素
				if (ChildItems.Count > 1) {
					var _items = new GrammarExprItems {
						EbnfId = $"{(EbnfId.StartsWith ("[part of] ") ? "" : "[part of] ")}{EbnfId}",
						ClassName = $"{ClassName}",
						Expr = Expr[..^1],
						ListType = ListType,
						InitRecognize = false,
						RepeatType = EbnfExprItemRepeatType._1
					};
					_items.ChildItems.AddRange (ChildItems);
					ListType = ExprItemListType.All;
					ChildItems.Clear ();
					ChildItems.Add (_items);
				}
			}

			if (_init)
				Suffix = m_suffix; // 重置子元素后缀
		}

		public string ClassCode (Dictionary<string, string> _ext_code) {
			StringBuilder _sb = new StringBuilder ();
			string _ext_key = $"{ClassName}{Suffix}";
			string _ext_code_str = (_ext_code?.ContainsKey (_ext_key) ?? false) ? _ext_code [_ext_key].Trim () : "";
			if (_ext_code_str != "") {
				_ext_code_str = string.Join (@"
", from p in _ext_code_str.Split ('\n', StringSplitOptions.RemoveEmptyEntries) let q = p.TrimEnd () select $"{(q == "" ? "" : "\t\t")}{q}");
			}
			_sb.Append ($@"	public class {ClassName}{Suffix}: IAST {{
		// {EbnfId} ::= {Expr}

		public AstParser Parser {{ init; get; }}

		public IEnumerator<int> TryParse (int _pos) {{
			if (!Parser.TryReg (""{ClassName}{Suffix}"", _pos))
				yield break;
{GenerateTryParse ()}
			Parser.UnReg (""{ClassName}{Suffix}"", _pos);
		}}

{GenerateTryParse2Wrap ()}

		public bool IsValid () => {IsValidExpr ()};

		public void PrintTree (int _indent) {{
			//Console.WriteLine ($""{{new string (' ', _indent * 4)}}{ClassName}{Suffix}"");
{PrintTree ()}
		}}

		public int Length {{ get => {LengthExpr ()}; }}

{MValue ()}{(_ext_code_str != "" ? $"\r\n\r\n{_ext_code_str}" : "")}
	}}");
			for (int i = 0; i < ChildItems.Count; ++i) {
				if (ChildItems [i] is GrammarExprItems _items) {
					_sb.Append ("\r\n\r\n").Append (_items.ClassCode (_ext_code));
				}
			}
			return _sb.ToString ();
		}

		public string GetClearStrings () {
			if (/*RepeatType.max_N () && InitRecognize*/false) {
				return $"Value{Suffix}.Clear ();";
			} else {
				return $"Value{Suffix} = null;";
			}
		}

		public string GenerateTryParse () {
			if (ChildItems.Count == 0)
				throw new Exception ("表达式对象列表不存在");

			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 3)).Append (s.TrimEnd ()).Append ("\r\n");

			if (ListType == ExprItemListType.All) {
				string _pos_name = "_pos";
				for (int i = 0; i < ChildItems.Count; ++i) {
					_append ($"{new string ('\t', i)}var {Suffix}_{i}_enum = _try_parse{Suffix}_{i} ({_pos_name});	");
					_append ($"{new string ('\t', i)}while ({Suffix}_{i}_enum.MoveNext ()) {{						");
					_pos_name = $"{Suffix}_{i}_enum.Current";
				}
				_append ($"{new string ('\t', ChildItems.Count)}yield return {_pos_name};							");
				for (int i = ChildItems.Count - 1; i >= 0; --i) {
					_append ($"{new string ('\t', i)}}}																");
				}
			} else if (ListType == ExprItemListType.Any) {
				for (int i = 0; i < ChildItems.Count; ++i) {
					_append ($"var {Suffix}_{i}_enum = _try_parse{Suffix}_{i} (_pos);								");
					_append ($"while ({Suffix}_{i}_enum.MoveNext ()) {{												");
					_append ($"	ValidIndex{Suffix} = {i};															");
					_append ($"	yield return {Suffix}_{i}_enum.Current;												");
					_append ("}																						");
				}
			}

			while (InitRecognize && _sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string GenerateTryParse2Wrap () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			if (/*RepeatType.max_N () && InitRecognize*/false) {
				if (ChildItems.Count != 1)
					throw new MethodAccessException ("需调用 ProcessConstruct 更新列结构");
				_append ($"IEnumerator<int> _try_parse{Suffix}_0 (int _pos) {{			");
				_append ($"	Parser.ErrorPos = _pos;										");
				_append ($"	var _o = new {ClassName}{Suffix}_0 {{ Parser = Parser }};	");
				_append ($"	var _enum = _o.TryParse (_pos);								");
				_append ("	while (_enum.MoveNext ()) {									");
				_append ($"		Value{Suffix}_0.Add (_o);								");
				_append ($"		yield return _enum.Current;								");
				_append ($"		var _enum1 = _try_parse{Suffix}_0 (_enum.Current);		");
				_append ($"		while (_enum1.MoveNext ())								");
				_append ($"			yield return _enum1.Current;						");
				_append ($"		Value{Suffix}_0.RemoveAt (Value{Suffix}_0.Count - 1);	");
				_append ("	}															");
				if (RepeatType.min_0 ()) {
					_append ("	yield return _pos;										");
				}
				_append ("}																");
			} else {
				for (int i = 0; i < ChildItems.Count; ++i) {
					_sb.Append (ChildItems[i].GenerateTryParse2 ()).Append ("\r\n");
				}
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string GenerateTryParse2 () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			if (RepeatType.max_N ()) {
				_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{		");
				_append ($"	Parser.ErrorPos = _pos;									");
				_append ($"	var _o = new {ClassName}{Suffix} {{ Parser = Parser }};	");
				_append ($"	var _enum = _o.TryParse (_pos);							");
				_append ("	while (_enum.MoveNext ()) {								");
				_append ($"		int _list_pos = Value{Suffix}.Count;				");
				_append ($"		Value{Suffix}.Add (_o);								");
				_append ("		yield return _enum.Current;							");
				_append ($"		var _enum1 = _try_parse{Suffix} (_enum.Current);	");
				_append ("		while (_enum1.MoveNext ())							");
				_append ("			yield return _enum1.Current;					");
				_append ($"		Value{Suffix}.RemoveAt (_list_pos);					");
				_append ("	}														");
				if (RepeatType.min_0 ()) {
					_append ("	yield return _pos;									");
				}
				_append ("}															");
			} else {
				_append ($"IEnumerator<int> _try_parse{Suffix} (int _pos) {{		");
				_append ($"	Parser.ErrorPos = _pos;									");
				_append ($"	var _o = new {ClassName}{Suffix} {{ Parser = Parser }};	");
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
			}
			return _sb.ToString ();
		}

		public string IsValidExpr () {
			if (ChildItems.Count == 0) {
				throw new Exception ("子元素个数必须 >0");
			} else if (ListType == ExprItemListType.Any) {
				return $"ValidIndex{Suffix} >= 0";
			} if (RepeatType.min_0 ()) {
				return "true";
			} else if (ChildItems.Count == 1) {
				if (/*RepeatType.max_N () && InitRecognize*/false) {
					if (ChildItems[0].RepeatType.min_0 ()) {
						return "true";
					} else {
						return $"Value{Suffix}_{0}.Count > 0";
					}
				} else {
					if (ChildItems[0] is TerminalCharItem || ChildItems[0] is TerminalStringItem) {
						return $"!string.IsNullOrEmpty (Value{Suffix}_{0})";
					} else {
						return $"Value{Suffix}_{0}.IsValid ()";
					}
				}
			} else {
				StringBuilder _sb = new StringBuilder ();
				_sb.Append ("(");
				for (int i = 0; i < ChildItems.Count; ++i) {
					_sb.Append (i > 0 ? " && " : "");
					if (ChildItems [i] is GrammarExprItems _items) {
						if (_items.RepeatType.max_N ()) {
							if (_items.RepeatType.is_1N ()) {
								_sb.Append ($"Value{Suffix}_{i}.Count > 0");
							} else {
								_sb.Append ("true");
							}
						} else {
							if (ChildItems[i].RepeatType.min_0 ()) {
								_sb.Append ("true");
							} else {
								_sb.Append ($"Value{Suffix}_{i}.IsValid ()");
							}
						}
					} else {
						if (ChildItems[i].RepeatType.min_0 ()) {
							_sb.Append ("true");
						} else {
							_sb.Append (ChildItems[i].IsValidExpr ());
						}
					}
				}
				_sb.Append (")");
				return _sb.ToString ();
			}
		}

		public string PrintTree () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 3)).Append (s.TrimEnd ()).Append ("\r\n");
			if (/*RepeatType.max_N () && InitRecognize*/false) {
				if (ChildItems.Count != 1)
					throw new MethodAccessException ("需调用 ProcessConstruct 更新列结构");
				_append ($"for (int i = 0; i < Value{Suffix}_{0}.Count; ++i)						");
				_append ($"	Value{Suffix}_{0} [i].PrintTree (_indent + 1);							");
			} else if (ListType == ExprItemListType.All) {
				for (int i = 0; i < ChildItems.Count; ++i) {
					if (ChildItems[i] is NonTerminalItem _item) {
						_append ($"if (Value{Suffix}_{i} != null{(_item.RepeatType.min_1 () ? $" && Value{Suffix}_{i}.IsValid ()" : "")}) {{");
						_append ($"	{ChildItems[i].PrintTree ()}");
						_append ("}																	");
					} else if (ChildItems[i] is GrammarExprItems _items) {
						if (_items.RepeatType.max_N ()) {
							_append ($"for (int i = 0; i < Value{Suffix}_{i}.Count; ++i)			");
							_append ($"	Value{Suffix}_{i} [i].PrintTree (_indent + 1);				");
						} else {
							_append ($"Value{Suffix}_{i}.PrintTree (_indent + 1);					");
						}
					} else {
						_append (ChildItems[i].PrintTree ());
					}
				}
			} else if (ListType == ExprItemListType.Any) {
				for (int i = 0; i < ChildItems.Count; ++i) {
					_append ($"{(i > 0 ? "} else " : "")}if (ValidIndex{Suffix} == {i}) {{			");
					if (ChildItems[i] is NonTerminalItem _item) {
						_append ($"	if (Value{Suffix}_{i} != null{(_item.RepeatType.min_1 () ? $" && Value{Suffix}_{i}.IsValid ()" : "")}) {{");
						_append ($"		{ChildItems[i].PrintTree ()}");
						_append ("	}																");
					} else if (ChildItems[i] is GrammarExprItems _items) {
						if (_items.RepeatType.max_N ()) {
							_append ($"	for (int i = 0; i < Value{Suffix}_{i}.Count; ++i)			");
							_append ($"		Value{Suffix}_{i} [i].PrintTree (_indent + 1);			");
						} else {
							_append ($"	Value{Suffix}_{i}.PrintTree (_indent + 1);					");
						}
					} else {
						_append ($"	{ChildItems[i].PrintTree ()}");
					}
				}
				_append ("}");
			} else {
				throw new Exception ("列表类型错误");
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string MValue () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");

			if (/*RepeatType.max_N () && InitRecognize*/false) {
				if (ChildItems.Count != 1)
					throw new MethodAccessException ("需调用 ProcessConstruct 更新列结构");
				_append ($"public List<{ClassName}{Suffix}_{0}> Value{Suffix}_{0} {{ get; set; }} = new List<{ClassName}{Suffix}_{0}> ();	");
			} else {
				for (int i = 0; i < ChildItems.Count; ++i) {
					if (ChildItems[i] is GrammarExprItems _items) {
						if (_items.RepeatType.max_N ()) {
							_append ($"public List<{ClassName}{Suffix}_{i}> Value{Suffix}_{i} {{ get; set; }} = new List<{ClassName}{Suffix}_{i}> ();	");
						} else {
							_append ($"public {ClassName}{Suffix}_{i} Value{Suffix}_{i} {{ get; set; }} = null;		");
						}
					} else {
						_append (ChildItems[i].MValue ());
					}
				}
				if (ListType == ExprItemListType.Any) {
					_append ($"public int ValidIndex{Suffix} {{ get; set; }} = -1;									");
				}
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string LengthExpr () {
			StringBuilder _sb = new StringBuilder ();
			if (/*RepeatType.max_N () && InitRecognize*/false) {
				if (ChildItems.Count != 1)
					throw new MethodAccessException ("需调用 ProcessConstruct 更新列结构");
				_sb.Append ($"(from p in Value{Suffix}_{0} select p.Length).Sum ()");
			} else {
				for (int i = 0; i < ChildItems.Count; ++i) {
					_sb.Append (i > 0 ? " + " : "");
					if (ChildItems[i] is GrammarExprItems _items) {
						if (_items.RepeatType.max_N ()) {
							_sb.Append ($"(from p in Value{Suffix}_{i} select p.Length).Sum ()");
						} else {
							_sb.Append ($"Value{Suffix}_{i}.Length");
						}
					} else {
						_sb.Append (ChildItems[i].LengthExpr ());
					}

				}
			}
			return _sb.ToString ();
		}
	}
}
