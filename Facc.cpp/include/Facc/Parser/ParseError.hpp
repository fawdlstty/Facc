#ifndef __PARSE_ERROR_HPP__
#define __PARSE_ERROR_HPP__



#include <string>

#include <fmt/core.h>



class ParseError {
public:
	ParseError (std::string _code, int _error_pos, int _line, int _start_pos) {
		ErrorPos = _error_pos;
		Line = _line;
		LinePos = _error_pos - _start_pos;
		int _end_pos = _code.find ('\n', _start_pos);
		if (_end_pos == -1) {
			LineCode = _code.substr (_start_pos);
		} else {
			LineCode = _code.substr (_start_pos, _end_pos - _start_pos);
			if (LineCode.size () > 0 && LineCode [LineCode.size () - 1] == '\r')
				LineCode = LineCode.substr (0, LineCode.size () - 1);
		}
		ErrorInfo = fmt::format ("Invalid Character '{}'", _code [_error_pos - (_error_pos == _code.size () ? 1 : 0)]);
	}

	int ErrorPos = -1;
	int Line = -1;
	int LinePos = -1;
	std::string LineCode;
	std::string ErrorInfo;
};



#endif // __PARSE_ERROR_HPP__
