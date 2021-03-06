# 第一章：语言基础

编程语言，是一种人与计算机交互的方式。人想让计算机做什么，就通过编程语言的方式，与计算机达成共识。通常计算机无法直接识别一个语言，需要使用一种工具或程序，将编程语言转为二进制代码，计算机就能识别编程语言，从而开始执行。现存编程语言千千万，只要你能让你的语言转为计算机可识别的代码，那么等于你创造了一个编程语言。

## 第一节：代码和数据

代码或数据通常指代不同的东西，代码指的是让计算机执行的内容，数据指的是不可执行的内容。比如如下C#代码：

```csharp
Console.WriteLine ("Hello World!");
```

此代码分为两部分，一部分是 `"Hello World!"` 这个字符串，这是一串数据，计算机无法执行；一部分是 `Console.WriteLine ()` 这是一句可执行语句，代表执行这个函数，同时将前面的字符串内容传递给此函数，让此函数去执行。

虽然绝大部分语言的代码和数据区分的明明白白，但依旧有部分语言模糊了两者的关系，比如：

- Lisp没有明显的区分代码和数据
- JavaScript的eval函数能够执行字符串js代码
- 等等……

为了简单，本文档只以主流编程语言来解读，也就是明显区分代码与数据。当所有内容全部悉知后，你也能创建出模糊代码与数据关系的编程语言。

## 第二节：编译代码

编译，顾名思义，就是将人类可读的代码，翻译为计算机可读的指令。

假如我们设计了一个语言，首先将语言解析为语法树，然后将语法树转为masm汇编代码，最后调用汇编器编译汇编代码，翻译为二进制程序。  
这个过程有很多可以省略或者增加步骤，比如C++之父开发的CFront这个编译器能够编译C++代码，最开始的实现是将其编译为C代码，然后调用C编译器完成编译。  
当然，以上方式都不太主流了，主流方式有两种，一种是自己写一个虚拟机，然后解释执行虚拟指令（或者jit、AOT编译），比如C#、Java；再或者生成LLVM IR，调用LLVM生成对应环境可执行代码（比如clang、Rust）。

语法树其实也能直接生成CPU可执行的汇编代码，但没必要，因为现在已经有了汇编器工具了，假如全部靠手工从0实现各种轮子，那么只能说工作不饱和。

比如前段时间在公众号上非常火的V语言，编译方式是翻译为C++，然后调用C++编译器编译，然后很多人喷这种编译方式。其实真没必要喷，C++之父都这么搞的呢，何必。

此处就按LLVM IR作为中间语言。编译代码最关键的步骤就是代码转语法树，以及语法树转LLVM IR。实现这两个功能，就能实现出一个完整的编译器了。

## 第三节：语言前端和语言后端

听起来很像一个写UI一个写实现。其实不是，语言前端指的是，语言代码翻译为AST树，然后将AST树转为中间语言（比如LLVM IR）；语言后端是将中间语言翻译为对应平台的可执行指令，比如流行的指令集有：Intel x86、arm、mips、risc-v……

我们一般做的自制编程语言指的是完成语言前端。后端这个就是另一回事了，比如自制了一块CPU，自己设计了指令集，然后想让代码跑在这个CPU上，就需要自己完成编程语言后端了，将LLVM IR翻译为自制的指令集。在此之后，使用LLVM的编译器就能编译自制指令集可执行的代码了。
