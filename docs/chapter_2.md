# 第二章：编程语言语法

## 由字符组成的代码

代码的组成，实际就是各种各样的字符组成。比如如下代码：

```csharp
static void Main (string [] args) {
    Console.WriteLine ("Hello World!");
}
```

首先是 `static` 这六个字符组成的关键字代表静态，然后是 `void` 这四个字符组成的关键字代表函数返回类型，后面还有可忽略的换行符、tab缩进符等。我们首先需要明确的概念就是，语言代码就是字符串由各种各样的字符组成。

## 各种语句的组合

上面的代码我们扩展一下：

```csharp
static void Main (string [] args) {
    Console.WriteLine ("Hello World!");
    Console.WriteLine ("Hello World!");
    Console.WriteLine ("Hello World!");
}
```

很简单的代码，是吧？我们再来分析一下，这段代码与上面那段代码的区别，可以明显发现，hello world打印了三遍。我们由此可以大胆给出一个猜测，一个函数里面可以有0行代码，也可以有N行代码（废话）。

我们现在来定义一下函数的结构，首先是函数头，我们定义名称为func_begin，然后是语句，我们定义名称为stmt，再然后是函数尾，我们定义名称为func_end

通过一种特殊的语法来描述这种现象：

```ebnf
func_stmt ::= func_begin stmt* func_end
```

简单解释一下，这段描述符的含义是，函数结构（func_stmt）由三个部分组成：函数头、函数体（由语句stmt组成，星号代表重复0次到无限次）、函数尾。

这个描述符就能完成匹配上面两段代码了，不管Main函数里面有几串表达式语句。

## 终结符和非终结符

简而言之，终结符就是代码中的字符或字符串；非终结符就是我们对一个结构的定义。
比如我们想要解析这一段语句：

```csharp
1+2*3-4/5
```

这段语句由9个部分组成，刚好一个字符就是一个语句，其中又包含含义相同的元素，我们可以将其理解为

```ebnf
expr ::= num op num op num op num op num
```

简化一下:

```ebnf
expr ::= num (op num)+
```

op与num的组合在后面重复了4次，我们就折叠一下，并要求重复次数至少1次以上。  
然后我们定义一下num与op：

```ebnf
num ::= [0-9]+
op ::= '+' | '-' | '*' | '/'
```

发现没，我们定义语言文法时， 由 `::=` 符号左右的两个部分组成，左边是对非终结符的定义，右边可以是终结符或者非终结符的组合。

我们可以将一串代码理解为多叉树的根节点，非终结符为树的茎节点，终结符为树的叶子节点。我们定义语法就是规定，一个茎节点允许有哪些子节点类型，比如是否允许有叶子节点等等，当我么定义好之后，拿一串代码，从根节点开始匹配，一直匹配到所有叶子节点，此时这棵多叉树就是我们的AST。
