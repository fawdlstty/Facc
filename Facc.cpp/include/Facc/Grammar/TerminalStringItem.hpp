#ifndef __TERMINAL_STRING_ITEM_HPP__
#define __TERMINAL_STRING_ITEM_HPP__



#include <sstream>
#include <string>

#include <fmt/core.h>

#include "../Common.hpp"
#include "IGrammarExprItem.hpp"



class TerminalStringItem: public IGrammarExprItem {
public:
	TerminalStringItem (std::string _content): Content (_content) {}

	std::string Content;

	std::string GetClearStrings () override {
		return fmt::format ("Value{} = \"\";", Suffix);
	}

	std::string GenerateTryParse2 (std::string _parent_class_name) override {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		_append2 ("inline IEnumerator<int> {}::_try_parse{} (int _pos) {{	", _parent_class_name, Suffix);
		_append0 ("	Parser->SetErrorPos (_pos);								");
		_append1 ("	if (Parser->TryMatchString (_pos, \"{}\")) {{			", Content);
		_append2 ("		Value{} = \"{}\";									", Suffix, Content);
		_append1 ("		co_yield _pos + Value{}.size ();					", Suffix);
		_append1 ("		Value{} = \"\";										", Suffix);
		_append0 ("	}														");
		if (RType::min_0 (RepeatType)) {
			_append0 ("	co_yield _pos;										");
		}
		_append0 ("}														");
		return _sb.str ();
	}

	std::string IsValidExpr () override {
		return (RType::min_0 (RepeatType)) ? "true" : fmt::format ("Value{}.size () > 0", Suffix);
	}

	std::string PrintTree () override {
		return fmt::format ("std::cout << std::string ((_indent + 1) * 4, ' ') << '[' << Value{} << ']' << std::endl;", Suffix);
	}

	std::string MValue () override {
		return fmt::format ("std::string Value{} = \"\";", Suffix);
	}
};



#endif // __TERMINAL_STRING_ITEM_HPP__
