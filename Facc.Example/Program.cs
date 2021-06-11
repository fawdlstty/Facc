using Facc.Grammar;
using Facc.Parser;
using System;
using System.Collections.Generic;

namespace Facc.Example {
	class Program {
		static Dictionary<string, string> s_ext_code = new Dictionary<string, string> {
			// 获取数字
			["NumAST"] = "public int Num { get => int.Parse (Value_0); }",

			// 获取符号
			["Op2SignAST"] = "public string Sign { get => ValidIndex switch { 0 => Value_0, 1 => Value_1, 2 => Value_2, 3 => Value_3, _ => throw new Exception (\"Invalid ValidIndex\") }; }",

			// 协助调整优先级
			["Op0ExprAST"] = "public void Process () => Value_1.Process ();",

			// 调整优先级
			["Op2ExprAST"] = @"private static Dictionary<string, int> s_level = new Dictionary<string, int> {
	[""+""] = 10,
	[""-""] = 10,
	[""*""] = 20,
	[""/""] = 20,
};

public void Process () {
	while (Value_1.Count > 1) {
		int _pos = 0;
		string _sign = Value_1 [0].Value_1_0.Sign;
		for (int i = 1; i < Value_1.Count; ++i) {
			if (s_level[Value_1[i].Value_1_0.Sign] > s_level[_sign]) {
				_sign = Value_1[i].Value_1_0.Sign;
				_pos = i;
			}
		}
		if (_pos == 0) {
			Value_0 = new ExprAST {
				ValidIndex = 2, Value_2 = new Op2ExprAST {
					Value_0 = Value_0,
					Value_1 = new List<Op2ExprAST_1> { Value_1[0], },
				}
			};
			Value_1.RemoveAt (0);
		} else {
			Value_1[_pos - 1] = new Op2ExprAST_1 {
				Value_1_0 = Value_1[_pos - 1].Value_1_0,
				Value_1_1 = new ExprAST {
					ValidIndex = 2, Value_2 = new Op2ExprAST {
						Value_0 = Value_1[_pos - 1].Value_1_1,
						Value_1 = new List<Op2ExprAST_1> { Value_1[_pos], },
					}
				},
			};
			Value_1.RemoveAt (_pos);
		}
	}
	Value_0.Process ();
	Value_1[0].Value_1_1.Process ();
}",

			// 协助调整优先级
			["ExprAST"] = @"public void Process () {
	while (ValidIndex == 1) {
		(Value_0, Value_1, Value_2, ValidIndex) = (Value_1.Value_1.Value_0, Value_1.Value_1.Value_1, Value_1.Value_1.Value_2, Value_1.Value_1.ValidIndex);
	}
	if (ValidIndex == 2) {
		Value_2.Process ();
	}
}",
		};

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
			_generator.Generate (s_ext_code);
		}

		static void parse () {
			var _ast_parser = new AstParser ();
			var _root = _ast_parser.Parse<ASTs.ExprAST> ("(((3+2)))*(5-4)");
			if (_root != null) {
				Console.WriteLine ();
				_root.Process ();
				_root.PrintTree (0);
			} else {
				var _err =_ast_parser.Error;
				Console.WriteLine ();
				Console.WriteLine ($"Error in Line {_err.Line}: {_err.ErrorInfo}");
				Console.WriteLine (_err.LineCode);
				Console.WriteLine ($"{new string (' ', _err.LinePos)}^");
			}
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
