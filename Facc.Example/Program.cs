using Facc.Grammar;
using Facc.Parser;
using System;

namespace Facc.Example {
	class Program {
		static void generate () {
			var _grammar = @"
						// 方括号代表匹配其中任一字符
num					::= [0-9]+
						// 单引号或双引号代表匹配整个字符串，“|”代表“或”关系，匹配任一串字符串
op2_sign			::= '+' | '-' | '*' | '/'
						// 空格连接代表“与”关系，所有元素必须同时存在
op0_expr			::= '(' expr ')'
						// 匹配 1+2*3-4 这样的字符串
op2_expr			::= expr (op2_sign expr)+
						// 表达式允许纯数字、括号或四则运算字符串
expr				::= num | op0_expr | op2_expr
";
			string _path = System.Diagnostics.Process.GetCurrentProcess ().MainModule.FileName;
			_path = $"{_path [..(_path.IndexOf ("Facc.Example\\") + 13)]}ASTs";
			var _generator = new AstGenerator (_grammar, _path, "Facc.Example.ASTs");
			_generator.ClearPath ();
			_generator.Generate ();
		}

		static void parse () {
			var _tree = AstParser.Parse<ASTs.ExprAST> ("3+2*5-4");
			_tree.PrintTree (0);
		}

		static void Main (string [] args) {
			Console.WriteLine ("输入序号指定执行逻辑：");
			Console.WriteLine ("    1. 生成AST代码");
			Console.WriteLine ("    2. 执行AST树");
			Console.WriteLine ("    其他. 退出");
			string _s = Console.ReadLine ();
			if (_s == "1") {
				generate ();
				Console.WriteLine ("代码已生成。");
			} else if (_s == "2") {
				parse ();
			}
			Console.WriteLine ("按任意键退出。。。");
			Console.ReadKey ();
		}
	}
}
