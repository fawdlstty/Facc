#include <iostream>
#include <memory>
#include <string>
#include <string_view>
#include <vector>

#include <Facc/IEnumerator.hpp>
#include <Facc/Grammar/ASTGenerator.hpp>



static void generate () {
	std::string _grammar = R"( id ::= [a-zA-Z\x80-\xff_][0-9a-zA-Z\x80-\xff_]* )";
	std::string _path = "E:\\GitHub\\_\\fa_experimental\\Facc\\Facc.cpp\\ASTs";
	AstGenerator _generator { _grammar, _path };
	_generator.ClearPath ();
	_generator.Generate ();
}

#if 1
#include "ASTs/IdAST.hpp"
static void parse () {
	auto _ast_parser = std::make_shared<AstParser> ();
	auto _root = _ast_parser->Parse<IdAST> ("eaw53张三");
	std::cout << std::endl;
	if (_root) {
		_root->PrintTree (0);
	} else {
		auto _err = _ast_parser->GetError ();
		std::cout << "Error in Line " << _err.Line << ": " << _err.ErrorInfo << std::endl;
		std::cout << _err.LineCode << std::endl;
		std::cout << std::string (_err.LinePos, ' ') << "^" << std::endl;
	}
}
#else
static void parse () {
	std::cout << "已跳过AST步骤。" << std::endl;
}
#endif

int main () {
	std::cout << "输入序号指定执行逻辑：" << std::endl;
	std::cout << "    1. 生成AST代码" << std::endl;
	std::cout << "    2. 执行AST树" << std::endl;
	std::cout << "    其他. 退出" << std::endl;
	std::string _s;
	std::cin >> _s;
	if (_s == "1") {
		generate ();
		std::cout << "代码已生成。";
	} else if (_s == "2") {
		parse ();
	}
	std::cout << "按任意键退出。。。" << std::endl;
	std::cin.get ();
	std::cin.get ();
	return 0;
}
