﻿
namespace Maple.MonoGameAssistant.MetadataExtensions.MetadataGenerator
{
#if MetadataSourceGenerator
    internal class MonoJsonFieldDTO  (ulong code, byte[]? utf8Name, byte[]? utf8Type, bool isStatic)
    {
 
    }
#else
    public class MonoJsonFieldDTO : Maple.MonoGameAssistant.Model.MonoDescriptionFieldDTO
    {
        public MonoJsonFieldDTO(ulong code, byte[]? utf8Name, byte[]? utf8Type, bool isStatic)
        {
            this.Code = code;
            this.Utf8Name = utf8Name;
            this.Utf8FieldType = utf8Type;
        }
    }
#endif 


}
