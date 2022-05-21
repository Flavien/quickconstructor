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

namespace QuickConstructor.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class QuickConstructorExtensions
{
    public static T? GetAttribute<T>(this ISymbol symbol)
        where T : Attribute
    {
        string name = typeof(T).Name;
        AttributeData? attribute = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == name);

        if (attribute == null)
            return null;

        return CreateAttribute<T>(attribute);
    }

    public static T CreateAttribute<T>(this AttributeData attributeData)
    {
        T attribute = Activator.CreateInstance<T>();
        foreach (KeyValuePair<string, TypedConstant> pair in attributeData.NamedArguments)
        {
            typeof(T).GetProperty(pair.Key).SetValue(attribute, pair.Value.Value);
        }

        return attribute;
    }

    public static string GetDeclarationKeywords(this INamedTypeSymbol symbol)
    {
        StringBuilder stringBuilder = new();

        if (symbol.IsRecord)
        {
            stringBuilder.Append(SyntaxFactory.Token(SyntaxKind.RecordKeyword).ToFullString());
            stringBuilder.Append(' ');
        }

        if (symbol.TypeKind == TypeKind.Struct)
            stringBuilder.Append(SyntaxFactory.Token(SyntaxKind.StructKeyword).ToFullString());
        else
            stringBuilder.Append(SyntaxFactory.Token(SyntaxKind.ClassKeyword).ToFullString());

        return stringBuilder.ToString();
    }
}
