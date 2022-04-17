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
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class TypeAnalyzer
{
    private readonly INamedTypeSymbol _classSymbol;
    private readonly AutoConstructorAttribute _attribute;

    public TypeAnalyzer(INamedTypeSymbol classSymbol, AutoConstructorAttribute attribute)
    {
        _classSymbol = classSymbol;
        _attribute = attribute;
    }

    public IList<ConstructorParameter> GetMembers()
    {
        IEnumerable<ConstructorParameter> fields = GetFields();
        IEnumerable<ConstructorParameter> properties = GetProperties();

        return fields.Concat(properties).ToList();
    }

    private IEnumerable<ConstructorParameter> GetFields()
    {
        foreach (IFieldSymbol field in _classSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (!field.CanBeReferencedByName)
                continue;

            if (field.IsStatic)
                continue;

            if (!_attribute.IncludeNonReadOnlyMembers && !field.IsReadOnly)
                continue;

            if (HasFieldInitializer(field))
                continue;

            yield return new ConstructorParameter(
                symbol: field,
                type: field.Type,
                parameterName: ToCamelCase(field.Name));
        }
    }

    private IEnumerable<ConstructorParameter> GetProperties()
    {
        foreach (IPropertySymbol property in _classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (!property.CanBeReferencedByName)
                continue;

            if (property.IsStatic)
                continue;

            if (!_attribute.IncludeNonReadOnlyMembers && !property.IsReadOnly)
                continue;

            if (!IsAutoProperty(property))
                continue;

            if (HasPropertyInitializer(property))
                continue;

            yield return new ConstructorParameter(
                symbol: property,
                type: property.Type,
                parameterName: ToCamelCase(property.Name));
        }
    }

    private static string ToCamelCase(string name)
    {
        name = name.TrimStart('_');
        return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
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
}
