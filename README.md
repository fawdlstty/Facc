# Facc

Facc 是一种自制语言的工具，通过极简语法描述文法，自动生成AST代码。

## 引言

编译原理自始至终都是非常难学的知识，虽然网上能找到各种各样的教程及文档，但也极少有开发者深入研究。本仓库作为另一种方案，以更简单的视角来解读编译原理，提供完善教程协助用户自主完成一个编译器。

## 开始

首先，NuGet上安装Facc。

生成AST：

```csharp
var _grammar = @"   // 语法描述字符串
                    // 方括号代表匹配其中任一字符
num                 ::= [0-9]+
                    // 单引号或双引号代表匹配整个字符串，“|”代表“或”关系，匹配任一串字符串
op2_sign            ::= '+' | '-' | '*' | '/'
                    // 空格连接代表“与”关系，所有元素必须同时存在
op0_expr            ::= '(' expr ')'
                    // 匹配 1+2*3-4 这样的字符串
op2_expr            ::= expr (op2_sign expr)+
                    // 表达式允许纯数字、括号或四则运算字符串
expr                ::= num | op0_expr | op2_expr
";
string _path = "D:\\ASTs"; // AST解析文件生成路径
string _namespace = "Facc.Example.ASTs"; // 生成的AST解析文件的命名空间
var _generator = new AstGenerator (_grammar, _path, _namespace);
_generator.ClearPath (); // 清空指定路径下的所有文件
_generator.Generate (); // 生成AST解析文件
```

执行生成的AST代码，解析文法：

```csharp
var _ast_parser = new AstParser ();
if (_ast_parser.Parse<ASTs.ExprAST> ("3+2*5-4")) {
    var _root = _ast_parser.GetAST<ASTs.ExprAST> ();
    Console.WriteLine ();
    _root.PrintTree (0);
} else {
    var _err =_ast_parser.Error;
    Console.WriteLine ();
    Console.WriteLine ($"Error in Line {_err.Line}: {_err.ErrorInfo}");
    Console.WriteLine (_err.LineCode);
    Console.WriteLine ($"{new string (' ', _err.LinePos)}^");
}
```

## 文档

- [第一章：语言基础](docs/chapter_1.md)
- [第二章：编程语言语法](docs/chapter_2.md)
- [第三章：Facc语法规范](docs/chapter_3.md)

<!--
- 第一部分：语法描述

	+ 第四章：编译错误的处理

- 第二部分：语法树
	+ 第五章：语法树的使用
	+ 第六章：处理四则运算中运算符优先级
	+ 第七章：Python的缩进为何与众不同
	+ 第八章：CLI功能扩展方式

- 第三部分：LLVM
	+ 第九章：生成LLVM IR
	+ 第十章：Hello World!
	+ 第十一章：优化编译
-->

## License

代码开源方式：MIT  
文档开源方式：CC-BY-SA 4.0
