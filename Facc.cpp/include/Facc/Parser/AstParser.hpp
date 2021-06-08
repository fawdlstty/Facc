#ifndef __AST_PARSER_HPP__
#define __AST_PARSER_HPP__



#include <functional>
#include <map>
#include <memory>
#include <optional>
#include <string>

#include <fmt/core.h>

#include "../Common.hpp"
#include "ParseError.hpp"



class AstParser: public std::enable_shared_from_this<AstParser> {
public:
	template<typename T>
	std::shared_ptr<T> Parse (std::string code) {
		m_code = code;
		auto _t = std::make_shared<T> ();
		_t->Parser = shared_from_this ();
		auto _enum = _t->TryParse (0);
		while (_enum.MoveNext ()) {
			if (_enum.Current == code.size ()) {
				ErrorPos = -1;
				return _t;
			}
		}
		return nullptr;
	}

	void SetErrorPos (int _pos) { ErrorPos = ErrorPos > _pos ? ErrorPos : _pos; }

	ParseError GetError () {
		if (ErrorPos == -1)
			throw Exception ("No any error");
		int _line = 1;
		int _start_pos = 0, _end_pos = m_code.find ('\n') + 1;
		while (_end_pos < ErrorPos) {
			if (_end_pos == 0)
				return ParseError { m_code, ErrorPos, _line, _start_pos };
			_start_pos = _end_pos;
			_end_pos = m_code.find ('\n', _start_pos) + 1;
			++_line;
		}
		return ParseError { m_code, ErrorPos, _line, _start_pos };
	}

	std::optional<char> TryGetChar (size_t _pos, std::function<bool (char)> _check) {
		if (_pos >= m_code.size ())
			return std::nullopt;
		if (!_check (m_code[_pos]))
			return std::nullopt;
		return m_code[_pos];
	}

	bool TryMatchString (int _pos, std::string _s) {
		if (m_code.size () < _pos + _s.size ())
			return false;
		return m_code.substr (_pos, _s.size ()) == _s;
	}



	bool TryReg (std::string _class_name, int _pos) {
		_class_name = fmt::format ("{}@{}", _class_name, _pos);
		if (!m_set.contains (_class_name)) {
			m_set [_class_name] = 1;
		} else if (m_set [_class_name] >= 2) {
			return false;
		} else {
			m_set [_class_name]++;
		}
		return true;
	}

	void UnReg (std::string _class_name, int _pos) {
		_class_name = fmt::format ("{}@{}", _class_name, _pos);
		if (!m_set.contains (_class_name)) {
			throw new Exception ("½âÎö´íÎó");
		} else if (m_set [_class_name] > 1) {
			m_set [_class_name]--;
		} else {
			m_set.erase (_class_name);
		}
	}

private:
	int ErrorPos = -1;
	std::string m_code = "";
	std::map<std::string, int> m_set;
};



#endif //__AST_PARSER_HPP__
