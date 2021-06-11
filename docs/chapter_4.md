# 第四章：Facc 最佳实践

## 左递归

Facc 允许带左递归的表达式，想怎么用怎么用。

## 优先级

优先级需自己处理。比如如下四则运算：

```ebnf
op2_expr ::= expr (op2_sign expr)+
```

上述 op2_expr 可匹配：

- expr op2_sign expr
- expr op2_sign expr op2_sign expr
- expr op2_sign expr op2_sign expr op2_sign expr
- expr op2_sign expr op2_sign expr op2_sign expr op2_sign expr
- ...

通过这种方式，将同级计算在AST树中并行列举出来，然后手工根据符号优先级确定计算方式。

## 扩展方法

比如识别一个ID，要求是字母数字汉字，以及不能数字开头，默认将分为两部分：

```ebnf
id ::= [a-zA-Z\u0100-\uffff_] [0-9a-zA-Z\u0100-\uffff_]*
```

通过 `PrintTree` 方法可以看出，第一个字与后续被断开了，分成了两部分，对ID的使用将造成不便。此处可以加上扩展方法：

```csharp
static class AstExtensionMethods {
    public static string Value (this IdAST _a) => $"{_a.Value_0}{_a.Value_1}";
}
```

后续手工分析AST时，就能通过 `id.Value ()` 获取id字符串。其他AST结构也能做类似操作，简化AST的分析处理。

## 自定义生成代码

Facc 提供一种扩展方法的替代方式，使用自定义生成代码。此功能可以将开发者提供的字符串代码内置进AST方法中。

使用方式（首先关注示例代码）：

```csharp
_generator.Generate (null);
```

这儿有一个参数，我们简单的传了null。现在改为字典对象，key含义为类名，value含义为添加的字符串代码。只需将类名以及该类下需添加的代码一一对应，那么此功能就会将对应代码嵌入AST解析器里。示例：

```csharp
var _ext_code = new Dictionary<string, string> {
    ["ExprAST"] = "public int UserData = 0;",
};
_generator.Generate (_ext_code);
```

相比扩展方法的优缺点如下：

- 优点
    + 能嵌入自定义变量、getter/setter、自定义静态方法
    + 修改解析器相比扩展方法更方便，假如因代码错误导致生成解析器失败，需注释掉所有扩展方法才能重新编译
- 缺点
    + 只能以字符串方式使用，无法在AST编译期通过编译器检查
        * 弥补：生成AST后，在AST里写代码，写完之后将字符串拷贝至自定义生成代码里

有个地方可能开发者觉得奇怪，也就是嵌入代码需传类名，而不是grammar id。存在这样的设计的原因是，假如存在嵌套结构，比如：

```ebnf
op2_expr ::= expr (op2_sign expr)+
```

那么将会生成两个类，Op2ExprAST与Op2ExprAST_1，后者用于描述 `op2_sign expr` 语法结构。通过指定类名，就能正确区分出grammar嵌套的层级。

## 优先级调整

`Facc.Example` 项目里，通过自定义生成代码方式，手工识别并调整优先级，具体使用参考示例即可
