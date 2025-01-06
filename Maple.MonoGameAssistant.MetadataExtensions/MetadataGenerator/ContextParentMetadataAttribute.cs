﻿using System;

namespace Maple.MonoGameAssistant.MetadataExtensions.MetadataGenerator
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
#if MetadataSourceGenerator
    internal
#else
    public
#endif 
         class ContextParentMetadataAttribute<T>(bool export = false) : Attribute
    {
        public bool ExportJson { get; } = export;
    }

}