#ifndef __ASTGENERATOR_HPP__
#define __ASTGENERATOR_HPP__



#include <algorithm>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <string_view>
#include <vector>

//#define NOMINMAX
//#include <Windows.h>

#include <fmt/core.h>

#include "../Common.hpp"
#include "GrammarExprItems.hpp"



class AstGenerator {
private:
	std::string m_grammar;
	std::string m_path;

public:
	AstGenerator (std::string _grammar, std::string _path): m_grammar (_grammar), m_path (_path) {}

	void ClearPath () {
		if (std::filesystem::exists (m_path)) {
			std::filesystem::directory_iterator _iter { m_path };
			for (auto _item : _iter)
				std::filesystem::remove (_item);
		} else {
			std::filesystem::create_directory (m_path);
		}
		//std::string _path_find = fmt::format ("{}\\*AST.hpp", m_path);
		//WIN32_FIND_DATAA _wfd;
		//HANDLE _find = ::FindFirstFileA (_path_find.data (), &_wfd);
		//if (_find != INVALID_HANDLE_VALUE) {
		//	do {
		//		_path_find = fmt::format ("{}\\{}", m_path, _wfd.cFileName);
		//		::DeleteFileA (_path_find.data ());
		//	} while (::FindNextFileA (_find, &_wfd));
		//}
	}

	void Generate () {
		static auto f_split_line = [] (std::string_view _str) -> std::vector<std::string> {
			size_t _start = 0, _start_find = 0;
			std::vector<std::string> _v;
			while (_start_find < _str.size ()) {
				size_t _end = _str.find ('\n', _start_find);
				if (_end == std::string::npos) _end = _str.size ();
				if (_end > _start) {
					std::string _tmp = std::string (_str.substr (_start, _end - _start));
					_tmp = Common::trim (_tmp);
					if (!_tmp.empty ())
						_v.push_back (_tmp);
				}
				_start = _start_find = _end + 1;
			}
			return _v;
		};

		static auto f_get_macro = [] (std::string_view _str) -> std::string {
			std::string _tmp = fmt::format ("__{}_AST_HPP__", _str);
			std::transform (_tmp.begin (), _tmp.end (), _tmp.begin (), [] (char _ch) { return _ch >= 'a' && _ch <= 'z' ? (_ch - 'a' + 'A') : _ch; });
			return _tmp;
		};

		std::vector<std::string> _grammar_items = f_split_line (m_grammar);
		bool _in_comment = false;
		for (std::string _grammar_line : _grammar_items) {
			int _p, _p1;
			if (_in_comment) {
				_p = _grammar_line.find ("*/");
				if (_p == std::string::npos)
					continue;
				_grammar_line = Common::trim (_grammar_line.substr (_p + 2));
				_in_comment = false;
			}
			_p = _grammar_line.find ("//");
			_p1 = _grammar_line.find ("/*");
			while (_p != std::string::npos || _p1 != std::string::npos) {
				if (_p != std::string::npos && _p1 != std::string::npos) {
					if (_p < _p1) {
						_p1 = std::string::npos;
					} else {
						_p = std::string::npos;
					}
				}
				if (_p != std::string::npos) {
					_grammar_line = _grammar_line.substr (0, _p);
				} else if (_p1 != std::string::npos) {
					int _p2 = _grammar_line.find ("*/", _p + 2);
					if (_p2 == std::string::npos) {
						_grammar_line = _grammar_line.substr (0, _p1);
						_in_comment = true;
					} else {
						_grammar_line = fmt::format ("{}{}", _grammar_line.substr (0, _p1), _grammar_line.substr (_p2 + 2));
					}
				}
				_grammar_line = Common::trim (_grammar_line);
				if (_grammar_line == "")
					break;
				_p = _grammar_line.find ("//");
				_p1 = _grammar_line.find ("/*");
			}
			if (_grammar_line == "")
				continue;
			_p = _grammar_line.find ("::=");
			if (_p == std::string::npos) {
				throw Exception (fmt::format ("从ebnf表达式中无法找到元素[::=]。错误行：{}", _grammar_line));
			}
			std::string _id = Common::trim (_grammar_line.substr (0, _p));
			std::string _expr = Common::trim (_grammar_line.substr (_p + 3));
			//
			if (_id.empty ()) {
				throw Exception ("非终结符名称不可为空");
			}
			if (_id [0] >= '0' && _id [0] <= '9') {
				throw Exception ("非终结符名称不可以数字开头");
			}
			for (char _ch : _id) {
				if (!((_ch >= '0' && _ch <= '9') || (_ch >= 'a' && _ch <= 'z') || _ch == '_')) {
					throw Exception (fmt::format ("非终结符【{}】名称中不允许出现符号【{}】", _id, _ch));
				}
			}
			if (_expr.empty ()) {
				throw Exception (fmt::format ("【{}】所指代的表达式不可为空", _id));
			}
			//
			std::string _macro = f_get_macro (_id);
			std::string _class_name = Common::get_classname (_id);
			std::string _expr_tmp = std::string (_expr);
			auto _items = GrammarExprItems::ParseItems (std::string (_id), _class_name, _expr_tmp);
			_items->ProcessConstruct ();
			std::vector<std::string> _non_terminals;
			_items->GetNonterminals (_non_terminals);
			std::string _path = m_path;
			std::transform (_path.begin (), _path.end (), _path.begin (), [] (char _ch) { return _ch == '/' ? '\\' : _ch; });
			if (_path [_path.size () - 1] != '\\')
				_path += '\\';
			_path += _class_name;
			_path += ".hpp";
			std::ofstream _ofs { _path, std::ios::binary };
			_ofs << "\xef\xbb\xbf//\r\n";
			_ofs << "// This file is automatically generated by Facc\r\n";
			_ofs << "// https://github.com/fawdlstty/Fapp\r\n";
			_ofs << "//\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "#ifndef " << _macro << "\r\n";
			_ofs << "#define " << _macro << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "#include <iostream>\r\n";
			_ofs << "#include <memory>\r\n";
			_ofs << "#include <optional>\r\n";
			_ofs << "#include <string>\r\n";
			_ofs << "#include <string_view>\r\n";
			_ofs << "\r\n";
			_ofs << "#include <fmt/core.h>\r\n";
			_ofs << "\r\n";
			_ofs << "#include <Facc/IEnumerator.hpp>\r\n";
			_ofs << "#include <Facc/Parser/IAST.h>\r\n";
			_ofs << "#include <Facc/Parser/AstParser.hpp>\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			for (std::string _non_terminal : _non_terminals)
				_ofs << "class " << _non_terminal << ";" << "\r\n";
			if (_non_terminals.size () > 0)
				_ofs << "\r\n";
			_ofs << _items->ClassCode () << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			for (std::string _non_terminal : _non_terminals)
				_ofs << "#include \"" << _non_terminal << ".hpp\"" << "\r\n";
			if (_non_terminals.size () > 0)
				_ofs << "\r\n\r\n\r\n";
			_ofs << _items->AfterClassCode () << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "\r\n";
			_ofs << "#endif // " << _macro << "\r\n";
			_ofs.flush ();
			_ofs.close ();
		}
	}
};



#endif //__ASTGENERATOR_HPP__
