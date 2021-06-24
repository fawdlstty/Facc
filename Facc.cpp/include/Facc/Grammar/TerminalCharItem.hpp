#ifndef __TERMINAL_CHAR_ITEM_HPP__
#define __TERMINAL_CHAR_ITEM_HPP__



#include <sstream>
#include <string>
#include <vector>

#include <fmt/core.h>

#include "../Common.hpp"
#include "IGrammarExprItem.hpp"



class TerminalCharItem: public IGrammarExprItem {
public:
	TerminalCharItem (std::string _content): Content (_content) {}

	std::string Content;
	bool Reverse = false;

	std::string GetClearStrings () override {
		return fmt::format ("Value{} = \"\";", Suffix);
	}

	std::string GenerateTryParse2 (std::string _parent_class_name) override {
		std::stringstream _sb;
		auto _append0 = [&_sb] (std::string _s) { _sb << Common::trim_end (_s) << "\r\n"; };
		auto _append1 = [&_append0] (std::string _s, std::string _a0) { _append0 (fmt::format (_s, _a0)); };
		auto _append2 = [&_append0] (std::string _s, std::string _a0, std::string _a1) { _append0 (fmt::format (_s, _a0, _a1)); };
		if (RType::max_1 (RepeatType) && (!Reverse)) {
			_append2 ("inline IEnumerator<int> {}::_try_parse{} (int _pos) {{				", _parent_class_name, Suffix);
			_append0 ("	Parser->SetErrorPos (_pos);											");
			_append1 ("	Value{} = \"\";														", Suffix);
			_append0 ("	std::optional<char> _ch;											");
			_append2 ("	auto _check{} = [] (char _c) {{ return {}; }};						", Suffix, _get_compare_char ());
			_append1 ("	if ((_ch = Parser->TryGetChar (_pos, _check{})).has_value ()) {{	", Suffix);
			_append1 ("		Value{} += _ch.value ();										", Suffix);
			_append0 ("		co_yield _pos + 1;												");
			_append1 ("		Value{} = \"\";													", Suffix);
			_append0 ("	}																	");
			if (RType::min_0 (RepeatType)) {
				_append0 ("	co_yield _pos;													");
			}
			_append0 ("}																	");
		} else {
			_append2 ("IEnumerator<int> {}::_try_parse{} (int _pos) {{						", _parent_class_name, Suffix);
			_append0 ("	Parser->SetErrorPos (_pos);											");
			_append1 ("	Value{} = \"\";														", Suffix);
			_append0 ("	std::optional<char> _ch;											");
			_append0 ("	int _pos0 = _pos;													");
			_append2 ("	auto _check{} = [] (char _c) {{ return {}; }};						", Suffix, _get_compare_char ());
			_append1 ("	while ((_ch = Parser->TryGetChar (_pos0++, _check{})).has_value ())	", Suffix);
			_append1 ("		Value{} += _ch.value ();										", Suffix);
			_append1 ("	if (Value{}.size () > 0) {{											", Suffix);
			_append1 ("		co_yield _pos + Value{}.size ();								", Suffix);
			_append1 ("		Value{} = \"\";													", Suffix);
			_append0 ("	}																	");
			if (RType::min_0 (RepeatType)) {
				_append0 ("	co_yield _pos;													");
			}
			_append0 ("}																	");
		}
		return _sb.str ();
	}

	std::string IsValidExpr () override {
		return RType::min_0 (RepeatType) ? "true" : fmt::format ("Value{}.size () > 0", Suffix);
	}

	std::string PrintTree () override {
		return fmt::format ("std::cout << std::string ((_indent + 1) * 4, ' ') << '[' << Value{} << ']' << std::endl;", Suffix);
	}

	std::string MValue () override {
		return fmt::format ("std::string Value{} = \"\";", Suffix);
	}

private:
	class ITItem {
	public:
		virtual std::string Gen () = 0;
	};

	class TCharItem: ITItem {
	public:
		TCharItem (std::string _value): Value (_value) {}
		std::string Value;
		std::string Gen () override { return fmt::format ("_c == '{}'", Value); }
	};

	class TRangeItem: ITItem {
	public:
		TRangeItem (std::string _value1, std::string _value2): Value1 (_value1), Value2 (_value2) {}
		std::string Value1;
		std::string Value2;
		std::string Gen () override { return fmt::format ("(_c >= '{}' && _c <= '{}')", Value1, Value2); }
	};

	std::string _get_compare_char () {
		std::vector<std::string> _chars;
		for (size_t i = 0; i < Content.size (); ++i) {
			char _ch = Content [i];
			if (_ch == '\\') {
				_ch = Content [i + 1];
				int _len = _ch == 'x' ? 4 : (_ch == 'u' ? 6 : 2);
				_chars.push_back (Content.substr (i, _len));
				i += _len - 1;
			} else {
				_chars.push_back (fmt::format ("{}", Content [i]));
			}
		}
		std::vector<ITItem*> _items;
		while (!_chars.empty ()) {
			if (_chars.size () >= 3 && _chars [1] == "-") {
				_items.push_back ((ITItem*) new TRangeItem (_chars [0], _chars [2]));
				_chars.erase (_chars.cbegin (), _chars.cbegin () + 2);
			} else {
				_items.push_back ((ITItem *) new TCharItem (_chars [0]));
				_chars.erase (_chars.cbegin ());
			}
		}
		std::stringstream _sb;
		for (size_t i = 0; i < _items.size (); ++i) {
			if (i > 0)
				_sb << " || ";
			_sb << _items [i]->Gen ();
		}
		std::string _ret = _sb.str ();
		_ret = (_items.size () == 1 && _ret [0] == '(') ? _ret.substr (0, _ret.size () - 2) : _ret;
		for (ITItem *_item : _items)
			delete _item;
		_items.clear ();
		if (Reverse)
			_ret = fmt::format ("!({})", _ret);
		return _ret;
	}
};



#endif // __TERMINAL_CHAR_ITEM_HPP__
