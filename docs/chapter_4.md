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
