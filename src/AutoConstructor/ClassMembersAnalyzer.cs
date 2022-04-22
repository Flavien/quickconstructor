// Copyright 2022 Flavien Charlon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace AutoConstructor;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class ClassMembersAnalyzer
{
    private readonly INamedTypeSymbol _classSymbol;
    private readonly AutoConstructorAttribute _attribute;

    public ClassMembersAnalyzer(
        INamedTypeSymbol classSymbol,
        AutoConstructorAttribute attribute)
    {
        _classSymbol = classSymbol;
        _attribute = attribute;
    }

    public ImmutableArray<ConstructorParameter> GetConstructorParameters()
    {
        return ImmutableArray.CreateRange(GetFields().Concat(GetProperties()));
    }

    private IEnumerable<ConstructorParameter> GetFields()
    {
        foreach (IFieldSymbol field in _classSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            AutoConstructorParameterAttribute? attribute = field.GetAttribute<AutoConstructorParameterAttribute>();

            if (ExcludeMember(field))
                continue;

            if (!_attribute.IncludeNonReadOnlyMembers && !field.IsReadOnly && attribute == null)
                continue;

            if (HasFieldInitializer(field))
                continue;

            yield return CreateParameter(field, field.Type, attribute);
        }
    }

    private IEnumerable<ConstructorParameter> GetProperties()
    {
        foreach (IPropertySymbol property in _classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            AutoConstructorParameterAttribute? attribute = property.GetAttribute<AutoConstructorParameterAttribute>();

            if (ExcludeMember(property))
                continue;

            if (!_attribute.IncludeNonReadOnlyMembers && !property.IsReadOnly && attribute == null)
                continue;

            if (!IsAutoProperty(property))
                continue;

            if (HasPropertyInitializer(property))
                continue;

            yield return CreateParameter(property, property.Type, attribute);
        }
    }

    public static bool ExcludeMember(ISymbol member)
    {
        return !member.CanBeReferencedByName
            || member.IsStatic;
    }

    private static bool HasFieldInitializer(IFieldSymbol symbol)
    {
        SyntaxNode? syntaxNode = symbol.DeclaringSyntaxReferences.ElementAtOrDefault(0)?.GetSyntax();
        VariableDeclaratorSyntax? field = syntaxNode as VariableDeclaratorSyntax;

        return field?.Initializer != null;
    }

    private static bool IsAutoProperty(IPropertySymbol propertySymbol)
    {
        return propertySymbol.ContainingType.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(field => !field.CanBeReferencedByName
                && SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, propertySymbol));
    }

    private static bool HasPropertyInitializer(IPropertySymbol symbol)
    {
        SyntaxNode? syntaxNode = symbol.DeclaringSyntaxReferences.ElementAtOrDefault(0)?.GetSyntax();
        PropertyDeclarationSyntax? property = syntaxNode as PropertyDeclarationSyntax;

        return property?.Initializer != null;
    }

    private ConstructorParameter CreateParameter(
        ISymbol member,
        ITypeSymbol type,
        AutoConstructorParameterAttribute? parameterAttribute)
    {
        string parameterName;
        if (parameterAttribute?.Name == null)
            parameterName = GetParameterName(member.Name);
        else
            parameterName = parameterAttribute.Name.TrimStart('@');

        List<AttributeData> attributeData = new();
        if (parameterAttribute?.IncludeAttributes != false)
        {
            foreach (AttributeData attribute in member.GetAttributes())
            {
                if (attribute.AttributeClass == null)
                    continue;

                AttributeData? attributeUsage = attribute.AttributeClass
                    .GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass?.Name == nameof(AttributeUsageAttribute));

                if (attributeUsage == null)
                    continue;

                TypedConstant validOn = attributeUsage.ConstructorArguments[0];
                if (validOn.Value is not int targets)
                    continue;

                if (((AttributeTargets)targets).HasFlag(AttributeTargets.Parameter))
                {
                    attributeData.Add(attribute);
                }
            }
        }

        bool nullCheck;
        if (_attribute.NullChecks == NullChecksSettings.NonNullableReferencesOnly)
            nullCheck = !type.IsValueType && type.NullableAnnotation == NullableAnnotation.NotAnnotated;
        else if (_attribute.NullChecks == NullChecksSettings.Always)
            nullCheck = !type.IsValueType;
        else
            nullCheck = false;

        return new ConstructorParameter(
                symbol: member,
                type: type,
                parameterName: parameterName,
                nullCheck: nullCheck,
                attributes: attributeData);
    }

    private static string GetParameterName(string symbolName)
    {
        symbolName = symbolName.TrimStart('_', '@');
        return symbolName.Substring(0, 1).ToLowerInvariant() + symbolName.Substring(1);
    }
}
