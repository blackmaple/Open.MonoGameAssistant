﻿using Maple.MonoGameAssistant.MetadataExtensions.MetadataGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Maple.MonoGameAssistant.MetadataSourceGenerator
{
    public static class MetadataSourceGeneratorExtensions
    {


        static T? GetAttributeValue_NamedArgs<T>(this AttributeData attributeData, string name, T def)
        {
            var nVal = attributeData.NamedArguments.FirstOrDefault(p => p.Key == name).Value;
            if (nVal.Kind == TypedConstantKind.Array)
            {
                if (nVal.Values is T arr)
                {
                    return arr;
                }
            }
            else if (nVal.Kind == TypedConstantKind.Enum)
            {
                return (T?)nVal.Value;
            }
            else if (nVal.Value is T val2)
            {
                return val2;
            }
            return def;
        }
        static bool TryGetAttributeValue_CtorArgs<T>(this AttributeData attributeData, int index, out T? val)
        {
            Unsafe.SkipInit(out val);
            var nVal = attributeData.ConstructorArguments.ElementAtOrDefault(index);
            if (nVal.Kind == TypedConstantKind.Array)
            {
                if (nVal.IsNull)
                {
                    val = default;
                    return true;
                }

                if (nVal.Values is T arr)
                {
                    val = arr;
                    return true;
                }
            }
            else if (nVal.Kind == TypedConstantKind.Enum)
            {
                val = (T?)nVal.Value;
                return true;
            }
            else if (nVal.Value is T val2)
            {
                val = val2;
                return true;
            }

            return false;
        }
        static bool TryReadImmutableArray<T>(this ImmutableArray<TypedConstant> offsetSymbols, out T[]? values)
            where T : struct
        {
            if (offsetSymbols.IsDefault)
            {
                values = default;
                return false;
            }
            if (offsetSymbols.IsEmpty)
            {
                values = [];
            }
            else
            {
                values = [.. EnumImmutableArray()];
            }
            return true;
            IEnumerable<T> EnumImmutableArray()
            {
                foreach (var subMember in offsetSymbols)
                {
                    if (subMember.Value is T val)
                    {
                        yield return val;
                    }
                }
            }

        }
        static string ArrayDisplay<T>(this IEnumerable<T> arr)
            where T : struct
        {
            return $@"[{string.Join(", ", arr)}]";
        }


        #region Common

        public static IEnumerable<IParameterSymbol> GetCtorParameterSymbolExpression(ISymbol symbol)
        {
            if (symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                MetadataSourceGeneratorException.Throw<ParameterSyntax>($"{nameof(ISymbol)}:{symbol.ToDisplayString()} is not {nameof(INamedTypeSymbol)}");
                yield break;
            }
            if (namedTypeSymbol.Constructors.Length > 1)
            {
                MetadataSourceGeneratorException.Throw<ParameterSyntax>($"{symbol.ToDisplayString()}.ctor Number:{namedTypeSymbol.Constructors.Length}");
                yield break;
            }
            foreach (var ctor in namedTypeSymbol.Constructors)
            {
                foreach (var arg in ctor.Parameters)
                {
                    yield return arg;
                }
            }
        }
        public static IEnumerable<ParameterSyntax> BuildCtorParameterSyntaxExpression(IEnumerable<IParameterSymbol> parameterSymbols)
        {
            foreach (var arg in parameterSymbols)
            {
                yield return SyntaxFactory.Parameter(SyntaxFactory.Identifier(arg.Name))
                       .WithType(SyntaxFactory.ParseTypeName(arg.Type.ToDisplayString()));
            }
        }
        public static IEnumerable<ParameterSyntax> BuildCtorParameterSyntaxExpression(ISymbol symbol)
        {
            return BuildCtorParameterSyntaxExpression(GetCtorParameterSymbolExpression(symbol));
        }

        public static ConstructorDeclarationSyntax BuildDerivedCtorMethodExpression(ISymbol symbol,
            ParameterSyntax[] parameterSyntaxes, IEnumerable<StatementSyntax> statementSyntaxes)
        {
            var args = parameterSyntaxes.Select(p => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(p.Identifier))).ToArray();
            return BuildDerivedCtorMethodExpression(symbol, parameterSyntaxes, args, statementSyntaxes);
        }

        public static ConstructorDeclarationSyntax BuildDerivedCtorMethodExpression(ISymbol symbol,
           ParameterSyntax[] parameterSyntaxes, ArgumentSyntax[] args, IEnumerable<StatementSyntax> statementSyntaxes)
        {

            return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(symbol.Name))
                   .WithModifiers([SyntaxFactory.Token(SyntaxKind.PublicKeyword)])
                   .AddParameterListParameters(parameterSyntaxes)
                   .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                       SyntaxFactory.ArgumentList([.. args])
                   ))
                   .WithBody(SyntaxFactory.Block(statementSyntaxes));
        }


        public static IEnumerable<StatementSyntax> BuildAssignmentMemeberExpression(this IEnumerable<PropertyDeclarationSyntax> propertyDeclarations)
        {


            foreach (var prop in propertyDeclarations)
            {

                yield return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                   SyntaxKind.SimpleAssignmentExpression,
                   SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(prop.Identifier)),
                   SyntaxFactory.ObjectCreationExpression(prop.Type)
                   .WithArgumentList(SyntaxFactory.ArgumentList([
                             SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                       ]))
                    ));
            }

        }

        public static ClassDeclarationSyntax SetDerivedClassMemberExpression(
            ISymbol symbol,
            ISymbol baseSymbol,
            SyntaxList<MemberDeclarationSyntax> memberDeclarations)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(symbol.Name)
                .WithModifiers([SyntaxFactory.Token(SyntaxKind.PartialKeyword)])
                .WithBaseList(SyntaxFactory.BaseList([SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(baseSymbol.ToDisplayString()))]))
                .WithMembers(memberDeclarations);
            return classDeclaration;
        }

        public static NamespaceDeclarationSyntax BuildNamespaceExpression(ISymbol symbol,
            params ClassDeclarationSyntax[] classDeclarationSyntaxes)
        {
            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(symbol.ContainingNamespace.ToDisplayString()))
                .WithMembers([.. classDeclarationSyntaxes]);
        }


        #endregion

        #region ContextPropertyMetadataData
        public static IEnumerable<PropertyDeclarationSyntax> BuildContextPropertiesExpression(this ContextMemberMetadataData contextProperties, ISymbol parentSymbol)
        {
            foreach (var symbol in contextProperties.ClassSymbols)
            {
                if (symbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    MetadataSourceGeneratorException.Throw<PropertyDeclarationSyntax>($"{nameof(ISymbol)}:{symbol.ToDisplayString()} is not {nameof(INamedTypeSymbol)}");
                    yield break;
                }
                if (namedTypeSymbol.Constructors.Length > 1)
                {
                    MetadataSourceGeneratorException.Throw<PropertyDeclarationSyntax>($"{symbol.ToDisplayString()}.ctor Number:{namedTypeSymbol.Constructors.Length}");
                    yield break;
                }
                //foreach (var ctor in namedTypeSymbol.Constructors)
                //{
                //    if (false == ctor.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, parentSymbol)))
                //    {
                //        MetadataSourceGeneratorException.Throw<PropertyDeclarationSyntax>($"{symbol.ToDisplayString()}.ctor Parameters not found:{parentSymbol.ToDisplayString()}");
                //        yield break;
                //    }
                //}
                var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(symbol.ToDisplayString()), symbol.Name)
                  .WithModifiers([SyntaxFactory.Token(SyntaxKind.PublicKeyword)])
                  .WithAccessorList(
                    SyntaxFactory.AccessorList([SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))])
                    );

                yield return propertyDeclaration;
            }
        }


        #endregion

        #region ClassPropertyMetadataData

        public static IEnumerable<ParameterSyntax> BuildClassParentCtorParameterExpression(IEnumerable<IParameterSymbol> parameterSymbols, ulong code)
        {
            foreach (var arg in parameterSymbols)
            {
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(arg.Name))
                           .WithType(SyntaxFactory.ParseTypeName(arg.Type.ToDisplayString()));

                if (arg.Type.SpecialType == SpecialType.System_UInt64)
                {
                    parameter = parameter.WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(code))));
                }
                yield return parameter;
            }
        }



        #endregion

        #region ClassMemberMetadataData
        public static ClassMemberMetadataData GetClassMemberMetadataData(this GeneratorAttributeSyntaxContext ctx, AttributeData att)
        {
            var parentMetadata = ctx.TargetSymbol.GetAttributes().Where(p =>
              p.AttributeClass!.ContainingNamespace.ToDisplayString() == typeof(ClassParentMetadataAttribute<,>).Namespace
              && p.AttributeClass.MetadataName == typeof(ClassParentMetadataAttribute<,>).Name
              ).FirstOrDefault();
            if (parentMetadata is null)
            {
                return MetadataSourceGeneratorException.Throw<ClassMemberMetadataData>($"{ctx.TargetSymbol.ToDisplayString()} not found {typeof(ClassParentMetadataAttribute<,>).FullName}");
            }
            var ptrSymbol = parentMetadata.AttributeClass!.TypeArguments[1];
            var metadata = new ClassMemberMetadataData()
            {
                ParentSymbol = parentMetadata.AttributeClass.TypeArguments[0],
                PtrSymbol = ptrSymbol,
                ContextSymbol = ctx.TargetSymbol,

                PropertyMetadataDatas = [.. ptrSymbol.EnumClassPropertyMetadata()],
                MethodMetadataDatas = [.. ptrSymbol.EnumClassMethodMetadata()]
            };

            if (att.TryGetAttributeValue_CtorArgs(0, out ImmutableArray<TypedConstant> arrName) && arrName.TryReadImmutableArray(out byte[]? utf8ImageName))
            {
                metadata.Utf8ImageName = utf8ImageName;
            }
            if (att.TryGetAttributeValue_CtorArgs(1, out arrName) && arrName.TryReadImmutableArray(out byte[]? utf8Namespace))
            {
                metadata.Utf8Namespace = utf8Namespace;
            }
            if (att.TryGetAttributeValue_CtorArgs(2, out arrName) && arrName.TryReadImmutableArray(out byte[]? utf8ClassName))
            {
                metadata.Utf8ClassName = utf8ClassName;
            }
            if (att.TryGetAttributeValue_CtorArgs(3, out arrName) && arrName.TryReadImmutableArray(out byte[]? utf8FullName))
            {
                metadata.Utf8FullName = utf8FullName;
            }
            return metadata;

        }
        public static IEnumerable<ClassPropertyMetadataData> EnumClassPropertyMetadata(this ITypeSymbol ptrSymbol)
        {
            foreach (var member in ptrSymbol.GetMembers())
            {
                if (member.Kind != SymbolKind.Property)
                {
                    continue;
                }
                if (member is not IPropertySymbol propertySymbol)
                {
                    continue;
                }
                if (propertySymbol.IsPartialDefinition == false)
                {
                    continue;
                }
                var att = propertySymbol.GetAttributes().Where(p => p.AttributeClass!.ToDisplayString() == typeof(ClassPropertyMetadataAttribute).FullName).FirstOrDefault();
                if (att is not null)
                {

                    var metadata = new ClassPropertyMetadataData()
                    {
                        PropertySymbol = propertySymbol,
                    };
                    if (att.TryGetAttributeValue_CtorArgs(0, out ImmutableArray<TypedConstant> arrName) && arrName.TryReadImmutableArray(out byte[]? utf8Name))
                    {
                        metadata.Utf8PropertyName = utf8Name;
                    }
                    if (att.TryGetAttributeValue_CtorArgs(1, out ImmutableArray<TypedConstant> arrType) && arrType.TryReadImmutableArray(out byte[]? utf8Type))
                    {
                        metadata.Utf8PropertyType = utf8Type;
                    }

                    yield return metadata;
                }
            }

        }
        public static IEnumerable<ClassMethodMetadataData> EnumClassMethodMetadata(this ITypeSymbol ptrSymbol)
        {
            foreach (var member in ptrSymbol.GetMembers())
            {
                if (member.Kind != SymbolKind.Method)
                {
                    continue;
                }
                if (member is not IMethodSymbol methodSymbol)
                {
                    continue;
                }
                if (methodSymbol.IsPartialDefinition == false)
                {
                    continue;
                }
                var att = methodSymbol.GetAttributes().Where(p => p.AttributeClass!.ToDisplayString() == typeof(ClassMethodMetadataAttribute).FullName).FirstOrDefault();
                if (att is not null)
                {
                    var metadata = new ClassMethodMetadataData()
                    {
                        MethodSymbol = methodSymbol,
                    };
                    if (att.TryGetAttributeValue_CtorArgs(0, out ImmutableArray<TypedConstant> arrName) && arrName.TryReadImmutableArray(out byte[]? utf8Name))
                    {
                        metadata.Utf8MethodName = utf8Name;
                    }
                    if (att.TryGetAttributeValue_CtorArgs(1, out ImmutableArray<TypedConstant> arrType) && arrType.TryReadImmutableArray(out byte[]? utf8Type))
                    {
                        metadata.Utf8MethodReturnType = utf8Type;
                    }
                    metadata.RuntimeMethod = att.GetAttributeValue_NamedArgs(nameof(ClassMethodMetadataAttribute.RuntimeMethodAsThis), false);
                    metadata.Utf8MethodParameterTypes = [.. EnumMethodParameterTypes(methodSymbol).OrderBy(p => p.order).Select(p => p.utf8Parameter)];

                    yield return metadata;
                }
            }

            static IEnumerable<(byte[]? utf8Parameter, int order)> EnumMethodParameterTypes(IMethodSymbol methodSymbol)
            {
                var attributeDatas = methodSymbol.GetAttributes().Where(p => p.AttributeClass is not null && p.AttributeClass.ToDisplayString() == typeof(ClassMethodParameterMetadataAttribute).FullName).ToArray();

                foreach (var att in attributeDatas)
                {
                    byte[]? utf8Parameter = default;
                    if (att.TryGetAttributeValue_CtorArgs(0, out ImmutableArray<TypedConstant> arrType) && arrType.TryReadImmutableArray(out utf8Parameter))
                    {

                    }
                    if (att.TryGetAttributeValue_CtorArgs(1, out int order))
                    {

                    }
                    yield return (utf8Parameter, order);
                }
            }
        }

        public static void BuildMethodMemberExpression(this ClassMemberMetadataData classMember)
        {

        }


        public static void BuildMethodInCtorExpression(this ClassMemberMetadataData classMember)
        {

        }

        public static IEnumerable<FieldDeclarationSyntax> BuildPropertyOffsetMemberExpression(this ClassMemberMetadataData classMember)
        {
            //foreach (var prop in classMember.PropertyMetadataDatas)
            //{
            //    yield return SyntaxFactory.FieldDeclaration(
            //        SyntaxFactory.VariableDeclaration(
            //            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
            //            [SyntaxFactory.VariableDeclarator($"OFFSET_{prop.PropertySymbol.Name}")]
            //    )).WithModifiers([SyntaxFactory.Token(SyntaxKind.StaticKeyword)]);
            //}

            yield return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    [.. classMember.PropertyMetadataDatas.Select(p => SyntaxFactory.VariableDeclarator($"OFFSET_{p.PropertySymbol.Name}"))]
            )).WithModifiers([SyntaxFactory.Token(SyntaxKind.StaticKeyword)]);

        }

        public static IEnumerable<StatementSyntax> BuildAssignmentOffsetMemberExpression(this ClassMemberMetadataData classMemberMetadataData, IEnumerable<FieldDeclarationSyntax> fieldDeclarationSyntaxes, ulong code)
        {
            foreach (var fieldDeclaration in fieldDeclarationSyntaxes)
            {
                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    yield return SyntaxFactory.ExpressionStatement
                        (
                            SyntaxFactory.AssignmentExpression
                            (
                                 SyntaxKind.SimpleAssignmentExpression,
                                 SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(classMemberMetadataData.ContextSymbol.ToDisplayString()), SyntaxFactory.IdentifierName(variable.Identifier.Text)),
                                 SyntaxFactory.InvocationExpression
                                 (
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.ThisExpression(),
                                    SyntaxFactory.IdentifierName(nameof(IClassMetadataCollector.GetMemberFieldOffset)))
                                ).WithArgumentList(SyntaxFactory.ArgumentList(
                                    [
                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(code)))
                                    ]
                                    ))
                            )
                        );







                }



            }
        }

        public static void BuildPropertyInCtorExpression(this ClassMemberMetadataData classMember)
        {

        }
        #endregion




    }
}
