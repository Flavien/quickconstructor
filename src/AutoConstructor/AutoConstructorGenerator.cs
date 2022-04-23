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

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AutoConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator(LanguageNames.CSharp)]
public class AutoConstructorGenerator : IIncrementalGenerator
{
    private static readonly Regex _attributeSyntaxRegex = new("AutoConstructor(Attribute)?$", RegexOptions.Compiled);

    private readonly SourceRenderer _sourceRenderer = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<(ConstructorDescriptor?, Diagnostic?)> syntaxProvider =
            context.SyntaxProvider.CreateSyntaxProvider(IsSynataxEligible, ProcessSyntaxNode);

        context.RegisterSourceOutput(syntaxProvider, (context, result) =>
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

    public bool IsSynataxEligible(SyntaxNode syntaxNode, CancellationToken cancel)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        if (attribute?.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax)
            return false;

        if (!_attributeSyntaxRegex.IsMatch(attribute.Name.ToString()))
            return false;

        return true;
    }

    private (ConstructorDescriptor?, Diagnostic?) ProcessSyntaxNode(
        GeneratorSyntaxContext syntaxContext,
        CancellationToken cancel)
    {
        if (syntaxContext.Node is not AttributeSyntax attributeSyntax)
            return default;

        if (attributeSyntax?.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax)
            return default;

        ISymbol? symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancel);

        if (symbol is not INamedTypeSymbol classSymbol)
            return default;

        AutoConstructorAttribute? attribute = symbol.GetAttribute<AutoConstructorAttribute>();

        if (attribute == null)
            return default;

        ClassSymbolProcessor processor = new(classSymbol, classDeclarationSyntax, attribute);

        try
        {
            return (processor.GetConstructorDescriptor(), null);
        }
        catch (AutoConstructorException exception)
        {
            return (null, exception.Diagnostic);
        }
    }
}
