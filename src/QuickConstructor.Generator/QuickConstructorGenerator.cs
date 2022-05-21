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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using QuickConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class QuickConstructorGenerator : IIncrementalGenerator
{
    private static readonly Regex _attributeSyntaxRegex = new("QuickConstructor(Attribute)?$", RegexOptions.Compiled);

    private readonly SourceRenderer _sourceRenderer = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassSymbolProcessor?> syntaxProvider =
            context.SyntaxProvider.CreateSyntaxProvider(IsCandidate, ProcessSyntaxNode)
            .WithComparer(ClassSymbolProcessorComparer.Default);

        IncrementalValuesProvider<(ConstructorDescriptor?, Diagnostic?)> model =
            syntaxProvider.Select(ProcessSymbol);

        context.RegisterSourceOutput(model, (context, result) =>
        {
            (ConstructorDescriptor? constructorDescriptor, Diagnostic? diagnostic) = result;

            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);

            if (constructorDescriptor != null)
            {
                context.AddSource(
                    constructorDescriptor.ClassSymbol.Name,
                    SourceText.From(_sourceRenderer.Render(constructorDescriptor), Encoding.UTF8));
            }
        });
    }

    public bool IsCandidate(SyntaxNode syntaxNode, CancellationToken cancel)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        if (attribute?.Parent?.Parent is not TypeDeclarationSyntax typeDeclaration)
            return false;

        if (!IsValidDeclaration(typeDeclaration))
            return false;

        if (!_attributeSyntaxRegex.IsMatch(attribute.Name.ToString()))
            return false;

        return true;
    }

    private ClassSymbolProcessor? ProcessSyntaxNode(
        GeneratorSyntaxContext syntaxContext,
        CancellationToken cancel)
    {
        if (syntaxContext.Node is not AttributeSyntax attributeSyntax)
            return null;

        if (attributeSyntax?.Parent?.Parent is not TypeDeclarationSyntax typeDeclaration)
            return null;

        if (!IsValidDeclaration(typeDeclaration))
            return null;

        ISymbol? symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, cancel);

        if (symbol is not INamedTypeSymbol classSymbol)
            return null;

        QuickConstructorAttribute? attribute = symbol.GetAttribute<QuickConstructorAttribute>();

        if (attribute == null)
            return null;

        return new ClassSymbolProcessor(classSymbol, typeDeclaration, attribute);
    }

    private (ConstructorDescriptor?, Diagnostic?) ProcessSymbol(
        ClassSymbolProcessor? processor,
        CancellationToken cancel)
    {
        if (processor == null)
            return default;

        try
        {
            return (processor.GetConstructorDescriptor(), null);
        }
        catch (DiagnosticException exception)
        {
            return (null, exception.Diagnostic);
        }
    }

    private static bool IsValidDeclaration(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration is ClassDeclarationSyntax
            || typeDeclaration is RecordDeclarationSyntax
            || typeDeclaration is StructDeclarationSyntax;
    }

    private class ClassSymbolProcessorComparer : IEqualityComparer<ClassSymbolProcessor?>
    {
        public static ClassSymbolProcessorComparer Default { get; } = new ClassSymbolProcessorComparer();

        public bool Equals(ClassSymbolProcessor? x, ClassSymbolProcessor? y)
        {
            return SymbolEqualityComparer.Default.Equals(x?.ClassSymbol, y?.ClassSymbol);
        }

        public int GetHashCode(ClassSymbolProcessor? obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj?.ClassSymbol);
        }
    }
}
