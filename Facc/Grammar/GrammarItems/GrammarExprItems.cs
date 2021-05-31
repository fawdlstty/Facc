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

		// 当前对象或子对象为无限匹配列表类型，代表匹配值为List<>类型
		public bool IsBListRevursive {
			get {
				if (IsBList)
					return true;
				foreach (var _item in ChildItems) {
					if (_item is GrammarExprItems _items) {
						if (_items.IsBListRevursive)
							return true;
					}
				}
				return false;
			}
		}

		// 当前对象为无限匹配列表类型，代表匹配值为List<>类型
		public bool IsBList {
			get {
				return (ListType == ExprItemListType.All && (RepeatType == EbnfExprItemRepeatType._0_to_N || RepeatType == EbnfExprItemRepeatType._1_to_N));
			}
		}

		public bool IsContainsCharTerm {
			get {
				foreach (var _item in ChildItems) {
					if (_item is TerminalCharItem) {
						return true;
					}
				}
				return false;
			}
		}

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
						throw new NotImplementedException ();

					if (_expr.Length > 0) {
						_items.RepeatType = _expr[0] switch {
							'*' => EbnfExprItemRepeatType._0_to_N,
							'+' => EbnfExprItemRepeatType._1_to_N,
							'?' => EbnfExprItemRepeatType._0_to_1,
							_ => EbnfExprItemRepeatType._1,
						};
						if (_items.RepeatType != EbnfExprItemRepeatType._1)
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
					if (_terminal.RepeatType != EbnfExprItemRepeatType._1)
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
					if (_terminal.RepeatType != EbnfExprItemRepeatType._1)
						_expr = _expr[1..].Trim ();
					_terminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_terminal);
				} else if (_expr[0] == '(') {
					// range
					var _nonterminal_items = ParseItems ($"[part of] {_ebnf_id}", $"{_items.ClassName}", ref _expr);
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
						if (_nonterminal.RepeatType != EbnfExprItemRepeatType._1)
							_expr = _expr [1..];
					}
					_expr = _expr.Trim ();
					_nonterminal.Suffix = $"{_items.Suffix}_{_items.ChildItems.Count}";
					_items.ChildItems.Add (_nonterminal);
				}

				// 处理连接符
				if (string.IsNullOrEmpty (_expr))
					break;
				if (_expr.Length > 0 && _expr [0] == '|') {
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

		public string ClassCode () {
			StringBuilder _sb = new StringBuilder ();
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
			Console.WriteLine ($""{{new string (' ', _indent * 4)}}{ClassName}{Suffix}"");
{PrintTree ()}
		}}

		public int Length {{ get => {LengthExpr ()}; }}

{MValue ()}
	}}");
			for (int i = 0; i < ChildItems.Count; ++i) {
				if (ChildItems [i] is GrammarExprItems _items) {
					_sb.Append ("\r\n\r\n").Append (_items.ClassCode ());
				}
			}
			return $"{_sb}";
		}

		public string GetClearStrings () {
			if (IsBList) {
				return $"Value{Suffix}.Clear ();";
			} else {
				return $"Value{Suffix} = null;";
			}
		}

		public string GenerateTryParse () {
			if (ClassName == "IntValAST")
				ClassName = ClassName;
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

			if (InitRecognize && _sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string GenerateTryParse2Wrap () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");
			for (int i = 0; i < ChildItems.Count; ++i) {
				if (ChildItems[i] is GrammarExprItems _items) {
					if (_items.IsBList) {
						_append ($"IEnumerator<int> _try_parse{Suffix}_{i} (int _pos) {{		");
						_append ($"	Parser.ErrorPos = _pos;										");
						_append ($"	var _o = new {ClassName}{Suffix}_{i} {{ Parser = Parser }};	");
						_append ($"	var _enum = _o.TryParse (_pos);								");
						_append ($"	int _list_pos = Value_{i}.Count;							");
						_append ("	while (_enum.MoveNext ()) {									");
						_append ($"		Value_{i}.Add (_o);										");
						_append ("		yield return _enum.Current;								");
						_append ($"		var _enum1 = _try_parse{Suffix}_{i} (_enum.Current);	");
						_append ("		while (_enum1.MoveNext ())								");
						_append ("			yield return _enum1.Current;						");
						_append ($"		Value_{i}.RemoveAt (_list_pos);							");
						_append ("	}															");
						if (_items.RepeatType.min_0 ()) {
							_append ("	yield return _pos;										");
						}
						_append ("}																");
					} else {
						_append ($"IEnumerator<int> _try_parse{Suffix}_{i} (int _pos) {{		");
						_append ($"	Parser.ErrorPos = _pos;										");
						_append ($"	var _o = new {ClassName}{Suffix}_{i} {{ Parser = Parser }};	");
						_append ($"	var _enum = _o.TryParse (_pos);								");
						_append ("	while (_enum.MoveNext ()) {									");
						_append ($"		Value{Suffix}_{i} = _o;									");
						_append ($"		yield return _enum.Current;								");
						_append ($"		Value{Suffix}_{i} = null;								");
						_append ("	}															");
						if (_items.RepeatType.min_0 ()) {
							_append ("	yield return _pos;										");
						}
						_append ("}																");
					}
				} else {
					_sb.Append ("\r\n").Append (ChildItems[i].GenerateTryParse2 ()).Append ("\r\n");
				}
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string GenerateTryParse2 () {
			throw new NotImplementedException ();
		}

		public string IsValidExpr () {
			if (ChildItems.Count == 0) {
				throw new NotImplementedException ();
			} else if (ListType == ExprItemListType.Any) {
				return $"ValidIndex{Suffix} >= 0";
			} if (RepeatType == EbnfExprItemRepeatType._0_to_1 || RepeatType == EbnfExprItemRepeatType._0_to_N) {
				return "true";
			} else if (ChildItems.Count == 1) {
				if (ChildItems [0] is TerminalCharItem || ChildItems [0] is TerminalStringItem) {
					return $"!string.IsNullOrEmpty (Value{Suffix}_{0})";
				} else {
					return $"Value{Suffix}_{0}.IsValid ()";
				}
			} else {
				StringBuilder _sb = new StringBuilder ();
				_sb.Append ("(");
				for (int i = 0; i < ChildItems.Count; ++i) {
					_sb.Append (i > 0 ? " && " : "");
					if (ChildItems [i] is GrammarExprItems _items) {
						if (_items.IsBList) {
							if (_items.RepeatType == EbnfExprItemRepeatType._1_to_N) {
								_sb.Append ($"Value{Suffix}_{i}.Count > 0");
							} else {
								_sb.Append ("true");
							}
						} else {
							if (ChildItems[i].RepeatType == EbnfExprItemRepeatType._0_to_1 || ChildItems[i].RepeatType == EbnfExprItemRepeatType._0_to_N) {
								_sb.Append ("true");
							} else {
								_sb.Append ($"Value{Suffix}_{i}.IsValid ()");
							}
						}
					} else {
						if (ChildItems[i].RepeatType == EbnfExprItemRepeatType._0_to_1 || ChildItems[i].RepeatType == EbnfExprItemRepeatType._0_to_N) {
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
			if (ListType == ExprItemListType.All) {
				for (int i = 0; i < ChildItems.Count; ++i) {
					if (ChildItems[i] is NonTerminalItem _item) {
						_append ($"if (Value{Suffix}_{i} != null{(_item.RepeatType.min_1 () ? $" && Value{Suffix}_{i}.IsValid ()" : "")}) {{");
						_append ($"	{ChildItems[i].PrintTree ()}");
						_append ("}																	");
					} else if (ChildItems[i] is GrammarExprItems _items) {
						if (_items.IsBList) {
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
						if (_items.IsBList) {
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
				throw new NotImplementedException ();
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string MValue () {
			StringBuilder _sb = new StringBuilder ();
			Action<string> _append = (s) => _sb.Append (new string ('\t', 2)).Append (s.TrimEnd ()).Append ("\r\n");

			for (int i = 0; i < ChildItems.Count; ++i) {
				if (ChildItems[i] is GrammarExprItems _items) {
					if (_items.IsBList) {
						_append ($"public List<{ClassName}{Suffix}_{i}> Value{Suffix}_{i} {{ get; set; }} = new List<{ClassName}{Suffix}_{i}> ();	");
					} else {
						_append ($"public {ClassName}{Suffix}_{i} Value{Suffix}_{i} {{ get; set; }} = null;											");
					}
				} else {
					_append (ChildItems[i].MValue ());
				}
			}
			if (ListType == ExprItemListType.Any) {
				_append ($"public int ValidIndex{Suffix} {{ get; set; }} = -1;																		");
			}
			while (_sb[^2] == '\r' && _sb[^1] == '\n')
				_sb.Remove (_sb.Length - 2, 2);
			return _sb.ToString ();
		}

		public string LengthExpr () {
			StringBuilder _sb = new StringBuilder ();
			for (int i = 0; i < ChildItems.Count; ++i) {
				_sb.Append (i > 0 ? " + " : "");
				if (ChildItems[i] is GrammarExprItems _items) {
					if (_items.IsBList) {
						_sb.Append ($"(from p in Value{Suffix}_{i} select p.Length).Sum ()");
					} else {
						_sb.Append ($"Value{Suffix}_{i}.Length");
					}
				} else {
					_sb.Append (ChildItems[i].LengthExpr ());
				}
				
			}
			return _sb.ToString ();
		}
	}
}
