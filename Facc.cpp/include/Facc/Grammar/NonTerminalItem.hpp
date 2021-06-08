#ifndef __NON_TERMINAL_ITEM_HPP__
#define __NON_TERMINAL_ITEM_HPP__



#include <sstream>
#include <string>

#include <fmt/core.h>

#include "../Common.hpp"
#include "IGrammarExprItem.hpp"



class NonTerminalItem: public IGrammarExprItem {
public:
	NonTerminalItem (std::string _non_terminal_name): NonTerminalName (_non_terminal_name) {}

	std::string NonTerminalName;

	std::string GetClearStrings () override {
		if (RType::max_N (RepeatType)) {
			return fmt::format ("Value{}.clear ();", Suffix);
		} else {
			return fmt::format ("Value{} = nullptr;", Suffix);
		}
	}

	std::string GenerateTryParse2 (std::string _parent_class_name) override {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		_append2 ("inline IEnumerator<int> {}::_try_parse{} (int _pos) {{	", _parent_class_name, Suffix);
		_append0 ("	Parser->SetErrorPos (_pos);								");
		_append1 ("	auto _o = std::make_shared<{}> ();						", NonTerminalName);
		_append0 ("	_o->Parser = Parser;									");
		_append0 ("	auto _enum = _o->TryParse (_pos);						");
		_append0 ("	while (_enum.MoveNext ()) {								");
		if (RType::max_N (RepeatType)) {
			_append1 ("		Value{}.push_back (_o);							", Suffix);
			_append0 ("		co_yield _enum.Current;							");
			_append1 ("		auto _enum1 = _try_parse{} (_enum.Current);		", Suffix);
			_append0 ("		while (_enum1.MoveNext ())						");
			_append0 ("			co_yield _enum1.Current;					");
			_append1 ("		Value{}.pop_back ();							", Suffix);
		} else {
			_append1 ("		Value{} = _o;									", Suffix);
			_append0 ("		co_yield _enum.Current;							");
			_append1 ("		Value{} = nullptr;								", Suffix);
		}
		_append0 ("	}														");
		if (RType::min_0 (RepeatType)) {
			_append0 ("	co_yield _pos;										");
		}
		_append0 ("}														");
		return _sb.str ();
	}

	std::string IsValidExpr () override {
		if (RType::max_N (RepeatType)) {
			if (RType::min_0 (RepeatType)) {
				return "true";
			} else {
				return fmt::format ("Value{}.size () > 0", Suffix);
			}
		} else {
			return fmt::format ("(Value{} != nullptr && Value{}->IsValid ())", Suffix, Suffix);
		}
	}

	std::string PrintTree () override {
		if (RType::max_N (RepeatType)) {
			return fmt::format ("for (auto &_val : Value{}) _val->PrintTree (_indent + 1);", Suffix);
		} else {
			return fmt::format ("Value{}->PrintTree (_indent + 1);", Suffix);
		}
	}

	std::string MValue () override {
		if (RType::max_N (RepeatType)) {
			return fmt::format ("std::vector<std::shared_ptr<{}>> Value{};", NonTerminalName, Suffix);
		} else {
			return fmt::format ("std::shared_ptr<{}> Value{} = nullptr;", NonTerminalName, Suffix);
		}
	}
};



#endif // __NON_TERMINAL_ITEM_HPP__
