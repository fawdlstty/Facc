#ifndef __GRAMMAR_EXPR_ITEMS_HPP__
#define __GRAMMAR_EXPR_ITEMS_HPP__



#include <iostream>
#include <memory>
#include <sstream>
#include <string>
#include <vector>

#include <fmt/core.h>

#include "../Common.hpp"
#include "IGrammarExprItem.hpp"
#include "TerminalCharItem.hpp"
#include "TerminalStringItem.hpp"
#include "NonTerminalItem.hpp"



class GrammarExprItems: public IGrammarExprItem {
public:
	GrammarExprItems (std::string _ebnf_id, std::string _class_name, std::string _expr): EbnfId (_ebnf_id), ClassName (_class_name), Expr (_expr) {}

	std::string EbnfId;
	std::string ClassName;
	std::string Expr;
	void SetSuffix (std::string _suffix) override {
		Suffix = _suffix;
		for (size_t i = 0; i < ChildItems.size (); ++i)
			ChildItems [i]->SetSuffix (fmt::format ("{}_{}", Suffix, i));
	}
	std::vector<std::shared_ptr<IGrammarExprItem>> ChildItems;
	ExprItemListType ListType = ExprItemListType::Unknown;
	bool InitRecognize = true;

	// 当前对象为无限匹配列表类型，代表匹配值为List<>类型
	bool IsBList () { return RType::max_N (RepeatType); }

	static std::shared_ptr<GrammarExprItems> ParseItems (std::string _ebnf_id, std::string _class_name, std::string &_expr) {
		auto _items = std::make_shared<GrammarExprItems> (_ebnf_id, _class_name, _expr);
		bool _is_in_quot = false;

		// 处理终止符
		_expr = Common::trim (_expr);
		if (_expr [0] != '(') {
			_items->RepeatType = EbnfExprItemRepeatType::_1;
		} else {
			_is_in_quot = true;
			_expr = Common::trim (_expr.substr (1));
		}

		while (!_expr.empty ()) {
			if (_expr [0] == ')') {
				_expr = Common::trim (_expr.substr (1));
				if (!_is_in_quot) {
					throw Exception ("'('、')' 字符必须一一对应");
				}

				if (!_expr.empty ()) {
					char _ch = _expr [0];
					_items->RepeatType = _ch == '*' ? EbnfExprItemRepeatType::_0_to_N
						: (_ch == '+' ? EbnfExprItemRepeatType::_1_to_N
							: (_ch == '?' ? EbnfExprItemRepeatType::_0_to_1 : EbnfExprItemRepeatType::_1));
					if (!RType::is_1 (_items->RepeatType))
						_expr = Common::trim (_expr.substr (1));
				} else {
					_items->RepeatType = EbnfExprItemRepeatType::_1;
				}
				_items->Expr = Common::trim (_items->Expr.substr (0, _items->Expr.size () - _expr.size ()));
				return _items;
			}

			// 处理表达式
			if (_expr [0] == '[') {
				size_t _p = _expr.find (']');
				auto _terminal = std::make_shared<TerminalCharItem> (_expr.substr (1, _p - 1));
				if (_expr.size () > _p + 1) {
					char _ch = _expr [_p + 1];
					_terminal->RepeatType = _ch == '*' ? EbnfExprItemRepeatType::_0_to_N
						: (_ch == '+' ? EbnfExprItemRepeatType::_1_to_N
							: (_ch == '?' ? EbnfExprItemRepeatType::_0_to_1 : EbnfExprItemRepeatType::_1));
				}
				_expr = Common::trim (_expr.substr (_p + 1));
				if (!RType::is_1 (_terminal->RepeatType))
					_expr = Common::trim (_expr.substr (1));
				_terminal->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
				_items->ChildItems.push_back (_terminal);
			} else if (_expr [0] == '\'' || _expr [0] == '"') {
				size_t _p = _expr.find (_expr [0], 1);
				auto _terminal = std::make_shared<TerminalStringItem> (_expr.substr (1, _p - 1));
				if (_expr.size () > _p + 1) {
					char _ch = _expr [_p + 1];
					_terminal->RepeatType = _ch == '*' ? EbnfExprItemRepeatType::_0_to_N
						: (_ch == '+' ? EbnfExprItemRepeatType::_1_to_N
							: (_ch == '?' ? EbnfExprItemRepeatType::_0_to_1 : EbnfExprItemRepeatType::_1));
				}
				_expr = Common::trim (_expr.substr (_p + 1));
				if (!RType::is_1 (_terminal->RepeatType))
					_expr = Common::trim (_expr.substr (1));
				_terminal->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
				_items->ChildItems.push_back (_terminal);
			} else if (_expr [0] == '^') {
				if (_expr.size () == 1)
					throw Exception ("表达式不规范");
				_expr = _expr.substr (1);
				if (_expr [0] == '[') {
					size_t _p = _expr.find (']');
					auto _terminal = std::make_shared<TerminalCharItem> (_expr.substr (1, _p - 1));
					_expr = Common::trim (_expr.substr (_p + 1));
					_terminal->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
					_terminal->Reverse = true;
					_items->ChildItems.push_back (_terminal);
				} else if (_expr [0] == '\'' || _expr [0] == '"') {
					size_t _p = _expr.find (_expr [0], 1);
					auto _terminal = std::make_shared<TerminalStringItem> (_expr.substr (1, _p - 1));
					_expr = Common::trim (_expr.substr (_p + 1));
					_terminal->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
					_terminal->Reverse = true;
					_items->ChildItems.push_back (_terminal);
				} else {
					throw Exception ("表达式不规范");
				}
			} else if (_expr [0] == '(') {
				// range
				auto _nonterminal_items = ParseItems (fmt::format ("[part of] {}", _ebnf_id), _items->ClassName, _expr);
				_nonterminal_items->InitRecognize = false;
				_expr = Common::trim (_expr);
				_nonterminal_items->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
				_items->ChildItems.push_back (_nonterminal_items);
			} else {
				// ID
				std::string _name = "";
				while (_expr.size () > 0) {
					char _ch = _expr [0];
					if (!((_ch >= '0' && _ch <= '9') || (_ch >= 'a' && _ch <= 'z') || (_ch >= 'A' && _ch <= 'Z') || _ch == '_'))
						break;
					_name += _ch;
					_expr = _expr.substr (1);
				}
				_name = Common::get_classname (_name);
				auto _nonterminal = std::make_shared<NonTerminalItem> (_name);
				if (_expr.size () > 0) {
					char _ch = _expr [0];
					_nonterminal->RepeatType = _ch == '*' ? EbnfExprItemRepeatType::_0_to_N
						: (_ch == '+' ? EbnfExprItemRepeatType::_1_to_N
							: (_ch == '?' ? EbnfExprItemRepeatType::_0_to_1 : EbnfExprItemRepeatType::_1));
					if (!RType::is_1 (_nonterminal->RepeatType))
						_expr = _expr.substr (1);
				}
				_expr = Common::trim (_expr);
				_nonterminal->SetSuffix (fmt::format ("{}_{}", _items->Suffix, _items->ChildItems.size ()));
				_items->ChildItems.push_back (_nonterminal);
			}

			// 处理连接符
			if (_expr.empty ())
				break;
			if (_expr [0] == '|') {
				if (_items->ListType == ExprItemListType::Unknown || _items->ListType == ExprItemListType::Any) {
					_items->ListType = ExprItemListType::Any;
				} else if (_expr [0] != ')') {
					std::cout << "列表类型不可变化" << std::endl;
					return nullptr;
				}
				_expr = Common::trim (_expr.substr (1));
			} else {
				if (_items->ListType == ExprItemListType::Unknown || _items->ListType == ExprItemListType::All) {
					_items->ListType = ExprItemListType::All;
				} else if (_expr [0] != ')') {
					std::cout << "列表类型不可变化" << std::endl;
					return nullptr;
				}
			}
		}
		if (_items->ListType == ExprItemListType::Unknown)
			_items->ListType = ExprItemListType::All;
		return _items;
	}

	std::string ClassCode () {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			auto _items = dynamic_cast<GrammarExprItems*> (&*ChildItems [i]);
			if (_items)
				_sb << _items->ClassCode () << "\r\n\r\n\r\n\r\n";
		}
		_append2 ("class {}{}: IAST {{															", ClassName, Suffix);
		_append0 ("public:																		");
		_append2 ("	// {} ::= {}																", EbnfId, Expr);
		_append0 ("																				");
		_append0 ("	std::shared_ptr<AstParser> Parser;											");
		_append0 ("																				");
		_append0 ("	IEnumerator<int> TryParse (int _pos) override {								");
		_append2 ("		if (!Parser->TryReg (\"{}{}\", _pos))									", ClassName, Suffix);
		_append0 ("			co_return;															");
		_append0 (GenerateTryParse ());
		_append2 ("		Parser->UnReg (\"{}{}\", _pos);											", ClassName, Suffix);
		_append0 ("	}																			");
		_append0 ("																				");
		_append0 (GenerateTryParse2Declare ());
		_append0 ("	bool IsValid () override;													");
		_append0 ("	void PrintTree (int _indent);												");
		_append0 ("	int size ();																");
		_append0 ("																				");
		_append0 (MValue ());
		_append0 ("};																			");
		return Common::remove_rn (_sb);
	}

	void GetNonterminals (std::vector<std::string> &_non_terminals) {
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
			if (_items) {
				_items->GetNonterminals (_non_terminals);
			} else {
				NonTerminalItem *_non_term_item = dynamic_cast<NonTerminalItem *> (ChildItems [i].get ());
				if (_non_term_item) {
					std::string _name = _non_term_item->NonTerminalName;
					bool _contains = false;
					for (size_t j = 0; j < _non_terminals.size (); ++j) {
						if (_non_terminals [j] == _name) {
							_contains = true;
							break;
						}
					}
					if (!_contains)
						_non_terminals.push_back (_name);
				}
			}
		}
	}

	std::string AfterClassCode () {
		std::stringstream _sb;
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
			if (_items)
				_sb << _items->AfterClassCode () << "\r\n\r\n";
		}
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append2_ = [&_append0] (std::string _s, std::string _a0, size_t _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		_append2 ("inline bool {}{}::IsValid () {{												", ClassName, Suffix);
		_append1 ("	return {};																	", IsValidExpr ());
		_append0 ("}																			");
		_append0 ("																				");
		_append2 ("inline void {}{}::PrintTree (int _indent) {{									", ClassName, Suffix);
		_append2 ("	std::cout << std::string (_indent * 4, ' ') << \"{}{}\" << std::endl;		", ClassName, Suffix);
		_append0 (PrintTree ());
		_append0 ("}																			");
		_append0 ("																				");
		_append2 ("inline int {}{}::size () {{													", ClassName, Suffix);
		_append0 ("	int _len = 0;																");
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
			NonTerminalItem *_non_term_item = dynamic_cast<NonTerminalItem *> (ChildItems [i].get ());
			if (_items || _non_term_item) {
				if (RType::max_N (ChildItems [i]->RepeatType)) {
					_append2_ ("	for (size_t i = 0; i < Value{}_{}.size (); ++i)				", Suffix, i);
					_append2_ ("		_len += Value{}_{} [i]->size ();						", Suffix, i);
				} else {
					_append2_ ("	_len += Value{}_{}->size ();								", Suffix, i);
				}
			} else {
				TerminalCharItem *_term_ch_item = dynamic_cast<TerminalCharItem *> (ChildItems [i].get ());
				if (_term_ch_item) {
					_append2_ ("	_len += Value{}_{}.size ();									", Suffix, i);
				} else {
					if (RType::max_N (ChildItems [i]->RepeatType)) {
						_append2_ ("	for (size_t i = 0; i < Value{}_{}.size (); ++i)			", Suffix, i);
						_append2_ ("		_len += Value{}_{} [i].size ();						", Suffix, i);
					} else {
						_append2_ ("	_len += Value{}_{}.size ();								", Suffix, i);
					}
				}
			}
		}
		_append0 ("	return _len;																");
		_append0 ("}																			");
		for (size_t i = 0; i < ChildItems.size (); ++i)
			_sb << ChildItems [i]->GenerateTryParse2 (fmt::format ("{}{}", ClassName, Suffix)) << "\r\n";
		return Common::remove_rn (_sb);
	}

	std::string GetClearStrings () override {
		if (IsBList ()) {
			return fmt::format ("Value{}.clear ();", Suffix);
		} else {
			return fmt::format ("Value{} = nullptr;", Suffix);
		}
	}

	std::string GenerateTryParse () {
		if (ChildItems.size () == 0)
			throw Exception ("表达式对象列表不存在");

		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << std::string (2, '\t') << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append2_ = [&_append0] (std::string _s, std::string _a0, size_t _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append3 = [&_append0] (std::string _s, std::string _a0, std::string _a1, size_t _a2) { _append0 (fmt::format (_s, _a0, _a1, _a2)); };
		auto _append4 = [&_append0] (std::string _s, std::string _a0, size_t _a1, std::string _a2, size_t _a3) { _append0 (fmt::format (_s, _a0, _a1, _a2, _a3)); };
		auto _append6 = [&_append0] (std::string _s, std::string _a0, std::string _a1, size_t _a2, std::string _a3, size_t _a4, std::string _a5) { _append0 (fmt::format (_s, _a0, _a1, _a2, _a3, _a4, _a5)); };
		if (ListType == ExprItemListType::All) {
			std::string _pos_name = "_pos";
			for (size_t i = 0; i < ChildItems.size (); ++i) {
				_append6 ("{}auto {}_{}_enum = _try_parse{}_{} ({});	", std::string (i, '\t'), Suffix, i, Suffix, i, _pos_name);
				_append3 ("{}while ({}_{}_enum.MoveNext ()) {{			", std::string (i, '\t'), Suffix, i);
				_pos_name = fmt::format ("{}_{}_enum.Current", Suffix, i);
			}
			_append2 ("{}co_yield {};									", std::string (ChildItems.size (), '\t'), _pos_name);
			for (size_t i = ChildItems.size () - 1; i != std::string::npos; --i) {
				_append1 ("{}}}											", std::string (i, '\t'));
			}
		} else if (ListType == ExprItemListType::Any) {
			for (size_t i = 0; i < ChildItems.size (); ++i) {
				_append4 ("auto {}_{}_enum = _try_parse{}_{} (_pos);	", Suffix, i, Suffix, i);
				_append2_ ("while ({}_{}_enum.MoveNext ()) {{			", Suffix, i);
				_append2_ ("	ValidIndex{} = {};						", Suffix, i);
				_append2_ ("	co_yield {}_{}_enum.Current;			", Suffix, i);
				_append0 ("}											");
			}
		}
		return Common::remove_rn (_sb);
	}

	std::string GenerateTryParse2Declare () {
		std::stringstream _sb;
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			_sb << fmt::format ("\tIEnumerator<int> _try_parse{}_{} (int _pos);\r\n", Suffix, i);
		}
		return Common::remove_rn (_sb);
	}

	std::string GenerateTryParse2 (std::string _parent_class_name) override {
		//if (_parent_class_name == "")
		//	return fmt::format ("IEnumerator<int> _try_parse{} (int _pos);", Suffix);
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append2_ = [&_append0] (std::string _s, std::string _a0, size_t _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		if (IsBList ()) {
			_append2 ("inline IEnumerator<int> {}::_try_parse{} (int _pos) {{	", _parent_class_name, Suffix);
			_append0 ("	Parser->SetErrorPos (_pos);								");
			_append2 ("	auto _o = std::make_shared<{}{}> ();					", ClassName, Suffix);
			_append0 ("	_o->Parser = Parser;									");
			_append0 ("	auto _enum = _o->TryParse (_pos);						");
			_append0 ("	while (_enum.MoveNext ()) {								");
			_append1 ("		int _list_pos = Value{}.size ();					", Suffix);
			_append1 ("		Value{}.push_back (_o);								", Suffix);
			_append0 ("		co_yield _enum.Current;								");
			_append1 ("		auto _enum1 = _try_parse{} (_enum.Current);			", Suffix);
			_append0 ("		while (_enum1.MoveNext ())							");
			_append0 ("			co_yield _enum1.Current;						");
			_append2 ("		Value{}.erase (Value{}.begin () + _list_pos);		", Suffix, Suffix);
			_append0 ("	}														");
			if (RType::min_0 (RepeatType)) {
				_append0 ("	co_yield _pos;										");
			}
			_append0 ("}														");
		} else {
			_append2 ("inline IEnumerator<int> {}::_try_parse{} (int _pos) {{	", _parent_class_name, Suffix);
			_append0 ("	Parser->SetErrorPos (_pos);								");
			_append2 ("	auto _o = std::make_shared<{}{}> ();					", ClassName, Suffix);
			_append0 ("	_o->Parser = Parser;									");
			_append0 ("	auto _enum = _o->TryParse (_pos);						");
			_append0 ("	while (_enum.MoveNext ()) {								");
			_append1 ("		Value{} = _o;										", Suffix);
			_append0 ("		co_yield _enum.Current;								");
			_append1 ("		Value{} = nullptr;									", Suffix);
			_append0 ("	}														");
			if (RType::min_0 (RepeatType)) {
				_append0 ("	co_yield _pos;										");
			}
			_append0 ("}														");
		}
		return Common::remove_rn (_sb);
	}

	std::string IsValidExpr () override {
		if (ChildItems.size () == 0) {
			throw Exception ("子元素个数必须 >0");
		} else if (ListType == ExprItemListType::Any) {
			return fmt::format ("ValidIndex{} >= 0", Suffix);
		} if (RType::min_0 (RepeatType)) {
			return "true";
		} else if (ChildItems.size () == 1) {
			if (dynamic_cast<TerminalCharItem *> (ChildItems [0].get ()) || dynamic_cast<TerminalStringItem *> (ChildItems [0].get ()) || dynamic_cast<NonTerminalItem *> (ChildItems [0].get ())) {
				return fmt::format ("Value{}_0->IsValid ()", Suffix);
			} else {
				return fmt::format ("Value{}_0.size () > 0", Suffix);
			}
		} else {
			std::stringstream _sb;
			_sb << "(";
			for (size_t i = 0; i < ChildItems.size (); ++i) {
				_sb << (i > 0 ? " && " : "");
				GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
				if (_items) {
					if (_items->IsBList ()) {
						if (RType::is_1N (_items->RepeatType)) {
							_sb << fmt::format ("Value{}_{}.size () > 0", Suffix, i);
						} else {
							_sb << "true";
						}
					} else {
						if (RType::min_0 (ChildItems [i]->RepeatType)) {
							_sb << "true";
						} else {
							_sb << fmt::format ("Value{}_{}->IsValid ()", Suffix, i);
						}
					}
				} else {
					if (RType::min_0 (ChildItems [i]->RepeatType)) {
						_sb << "true";
					} else {
						_sb << ChildItems [i]->IsValidExpr ();
					}
				}
			}
			_sb << ")";
			return Common::remove_rn (_sb);
		}
	}

	std::string PrintTree () override {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << std::string (1, '\t') << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append1_ = [&_append0] (std::string _s, size_t _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append2_ = [&_append0] (std::string _s, std::string _a0, size_t _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		auto _append3_ = [&_append0] (std::string _s, std::string _a0, std::string _a1, size_t _a2) { _append0 (fmt::format (_s, _a0, _a1, _a2)); };
		auto _append4_ = [&_append0] (std::string _s, std::string _a0, size_t _a1, std::string _a2, size_t _a3) { _append0 (fmt::format (_s, _a0, _a1, _a2, _a3)); };
		if (ListType == ExprItemListType::All) {
			for (size_t i = 0; i < ChildItems.size (); ++i) {
				NonTerminalItem *_item = dynamic_cast<NonTerminalItem *> (ChildItems [i].get ());
				if (_item) {
					if (RType::max_N (_item->RepeatType)) {
						_append0 (ChildItems [i]->PrintTree ());
					} else {
						if (RType::min_1 (_item->RepeatType)) {
							_append4_ ("if (Value{}_{} && Value{}_{}->IsValid ()) {{		", Suffix, i, Suffix, i);
						} else {
							_append2_ ("if (Value{}_{}) {{									", Suffix, i);
						}
						_append1 ("	{}														", ChildItems [i]->PrintTree ());
						_append0 ("}														");
					}
				} else {
					GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
					if (_items) {
						if (_items->IsBList ()) {
							_append2_ ("for (size_t i = 0; i < Value{}_{}.size (); ++i)		", Suffix, i);
							_append2_ ("	Value{}_{} [i]->PrintTree (_indent + 1);		", Suffix, i);
						} else {
							_append2_ ("Value{}_{}->PrintTree (_indent + 1);				", Suffix, i);
						}
					} else {
						_append0 (ChildItems [i]->PrintTree ());
					}
				}
			}
		} else if (ListType == ExprItemListType::Any) {
			for (size_t i = 0; i < ChildItems.size (); ++i) {
				_append3_ ("{}if (ValidIndex{} == {}) {{									", (i > 0 ? "} else " : ""), Suffix, i);
				NonTerminalItem *_item = dynamic_cast<NonTerminalItem *> (ChildItems [i].get ());
				if (_item) {
					if (RType::min_1 (_item->RepeatType)) {
						_append4_ ("	if (Value{}_{} && Value{}_{}->IsValid ()) {{	", Suffix, i, Suffix, i);
					}
					
					_append1 ("		{}														", ChildItems [i]->PrintTree ());
					_append0 ("	}															");
				} else {
					GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
					if (_items) {
						if (_items->IsBList ()) {
							_append2_ ("	for (size_t i = 0; i < Value{}_{}.size (); ++i)	", Suffix, i);
							_append2_ ("		Value{}_{} [i]->PrintTree (_indent + 1);	", Suffix, i);
						} else {
							_append2_ ("	Value{}_{}->PrintTree (_indent + 1);			", Suffix, i);
						}
					} else {
						_append1 ("	{}														", ChildItems [i]->PrintTree ());
					}
				}
			}
			_append0 ("}																	");
		} else {
			throw Exception ("列表类型错误");
		}
		return Common::remove_rn (_sb);
	}

	std::string MValue () override {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << std::string (1, '\t') << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append1_ = [&_append0] (std::string _s, size_t _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append5_ = [&_append0] (std::string _s, std::string _a0, std::string _a1, size_t _a2, std::string _a3, size_t _a4) { _append0 (fmt::format (_s, _a0, _a1, _a2, _a3, _a4)); };
		for (size_t i = 0; i < ChildItems.size (); ++i) {
			GrammarExprItems *_items = dynamic_cast<GrammarExprItems *> (ChildItems [i].get ());
			if (_items) {
				if (_items->IsBList ()) {
					_append5_ ("std::vector<std::shared_ptr<{}{}_{}>> Value{}_{};			", ClassName, Suffix, i, Suffix, i);
				} else {
					_append5_ ("std::shared_ptr<{}{}_{}> Value{}_{};						", ClassName, Suffix, i, Suffix, i);
				}
			} else {
				_append0 (ChildItems [i]->MValue ());
			}
		}
		if (ListType == ExprItemListType::Any) {
			_append1 ("int ValidIndex{} = -1;												", Suffix);
		}
		return Common::remove_rn (_sb);
	}
};



#endif // __GRAMMAR_EXPR_ITEMS_HPP__
