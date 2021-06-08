#ifndef __COMMON_HPP__
#define __COMMON_HPP__



#include <exception>
#include <sstream>
#include <string>
#include <string_view>



class Exception: public std::exception {
public:
	Exception (std::string _str): m_str (_str) {}
	char const *what () const override { return m_str.data (); }

private:
	std::string m_str;
};

class Common {
public:
	static std::string trim (std::string _str) {
		while (!_str.empty ()) {
			if (_str [0] == '\r' || _str [0] == '\t' || _str [0] == ' ') {
				_str = _str.substr (1);
				continue;
			}
			size_t _p = _str.size () - 1;
			if (_str [_p] == '\r' || _str [_p] == '\t' || _str [_p] == ' ') {
				_str = _str.substr (0, _p);
				continue;
			}
			return _str;
		}
		return "";
	}
	static std::string_view trim (std::string_view _str) {
		while (!_str.empty ()) {
			if (_str [0] == '\r' || _str [0] == '\t' || _str [0] == ' ') {
				_str = _str.substr (1);
				continue;
			}
			size_t _p = _str.size () - 1;
			if (_str [_p] == '\r' || _str [_p] == '\t' || _str [_p] == ' ') {
				_str = _str.substr (0, _p);
				continue;
			}
			return _str;
		}
		return "";
	}
	static std::string trim_end (std::string _str) {
		while (!_str.empty ()) {
			size_t _p = _str.size () - 1;
			if (_str [_p] == '\r' || _str [_p] == '\t' || _str [_p] == ' ') {
				_str = _str.substr (0, _p);
				continue;
			}
			return _str;
		}
		return "";
	}
	static std::string_view trim_end (std::string_view _str) {
		while (!_str.empty ()) {
			size_t _p = _str.size () - 1;
			if (_str [_p] == '\r' || _str [_p] == '\t' || _str [_p] == ' ') {
				_str = _str.substr (0, _p);
				continue;
			}
			return _str;
		}
		return "";
	}
	static std::string get_classname (std::string_view _str) {
		std::string _class_name = "";
		bool _upper = true;
		for (char _ch : _str) {
			if (_ch == '_') {
				_upper = true;
			} else {
				_class_name += (_upper && _ch >= 'a' && _ch <= 'z') ? (_ch - 'a' + 'A') : _ch;
				_upper = false;
			}
		}
		return _class_name + "AST";
	};
	static std::string get_classname (std::string _str) {
		std::string _class_name = "";
		bool _upper = true;
		for (char _ch : _str) {
			if (_ch == '_') {
				_upper = true;
			} else {
				_class_name += (_upper && _ch >= 'a' && _ch <= 'z') ? (_ch - 'a' + 'A') : _ch;
				_upper = false;
			}
		}
		return _class_name + "AST";
	};
	static std::string remove_rn (std::stringstream &_ss) {
		std::string _ret = _ss.str ();
		while (_ret.size () >= 2 && _ret [_ret.size () - 2] == '\r' && _ret [_ret.size () - 1] == '\n')
			_ret.erase (_ret.size () - 2, 2);
		return _ret;
	}
};



#endif //__COMMON_HPP__
