﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
#pragma warning disable CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。

namespace Maple.MonoGameAssistant.MetadataSourceGenerator
{
    public class ClassMethodMetadataData
    {
        public ISymbol MethodSymbol { set; get; } = default!;
        public byte[]? Utf8MethodName { set; get; }
        public byte[]? Utf8MethodReturnType { set; get; }
        public byte[]?[]? Utf8MethodParameterTypes { set; get; }

        public bool RuntimeMethod { set; get; }
    }

}
#pragma warning restore CS8632 // 只能在 "#nullable" 注释上下文内的代码中使用可为 null 的引用类型的注释。
