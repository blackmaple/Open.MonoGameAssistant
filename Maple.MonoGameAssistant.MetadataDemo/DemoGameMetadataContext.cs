﻿using Maple.MonoGameAssistant.MetadataExtensions.MetadataCollector;
using Maple.MonoGameAssistant.MetadataExtensions.MetadataGenerator;

namespace Maple.MonoGameAssistant.MetadataDemo
{
    [ContextParentMetadata<ContextMetadataCollector>()]
    [ContextPropertyMetadata<DemoGameSystem>]
    [ContextPropertyMetadata<ItemSystem>]

    
    public partial class DemoGameMetadataContext
    {
        //public DemoGameMetadataContext()
        //{
        //    //this.DemoGameSystem = new DemoGameSystem();
        //}
    }
}

