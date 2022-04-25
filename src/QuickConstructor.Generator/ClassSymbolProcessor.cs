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
using System.Collections.Immutable;
using System.Linq;
using QuickConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class ClassSymbolProcessor
{
    private readonly INamedTypeSymbol _classSymbol;
    private readonly ClassDeclarationSyntax _declarationSyntax;
    private readonly QuickConstructorAttribute _attribute;

    public ClassSymbolProcessor(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax declarationSyntax,
        QuickConstructorAttribute attribute)
    {
        _classSymbol = classSymbol;
        _declarationSyntax = declarationSyntax;
        _attribute = attribute;
    }

    public INamedTypeSymbol ClassSymbol { get => _classSymbol; }

    public ConstructorDescriptor GetConstructorDescriptor()
    {
        ClassMembersAnalyzer classMembersAnalyzer = new(_classSymbol, _attribute);
        ImmutableArray<ConstructorParameter> members = classMembersAnalyzer.GetConstructorParameters();

        ImmutableArray<ConstructorParameter> baseClassMembers = ImmutableArray
            .CreateRange(GetRecursiveClassMembers(_classSymbol.BaseType));

        ILookup<string, ConstructorParameter> lookup = members
            .ToLookup(member => member.ParameterName, StringComparer.Ordinal);

        IList<ConstructorParameter> duplicates = lookup
            .Where(nameGroup => nameGroup.Count() > 1)
            .Select(nameGroup => nameGroup.Last())
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new DiagnosticException(Diagnostic.Create(
                DiagnosticDescriptors.DuplicateConstructorParameter,
                _declarationSyntax.Identifier.GetLocation(),
                duplicates[0].ParameterName,
                _classSymbol.Name));
        }

        return new ConstructorDescriptor(
            _classSymbol,
            _attribute.ConstructorAccessibility,
            constructorParameters: members,
            baseClassConstructorParameters: baseClassMembers,
            documentation: _attribute.Documentation);
    }

    private static IEnumerable<ConstructorParameter> GetRecursiveClassMembers(INamedTypeSymbol? classSymbol)
    {
        if (classSymbol != null)
        {
            QuickConstructorAttribute? attribute = classSymbol.GetAttribute<QuickConstructorAttribute>();
            if (attribute != null)
            {
                ClassMembersAnalyzer analyzer = new(classSymbol, attribute);
                IReadOnlyList<ConstructorParameter> parameters = analyzer.GetConstructorParameters();

                return GetRecursiveClassMembers(classSymbol.BaseType).Concat(parameters);
            }
        }

        return ImmutableArray<ConstructorParameter>.Empty;
    }
}
