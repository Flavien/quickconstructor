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
using System.Text.RegularExpressions;
using AutoConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class AutoConstructorGenerator : ISourceGenerator
{
    private static readonly Regex _attributeSyntaxRegex = new("AutoConstructor(Attribute)?$", RegexOptions.Compiled);

    private readonly SourceRenderer _sourceRenderer = new();

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        Dictionary<string, int> classNames = new(StringComparer.Ordinal);
        foreach (ClassSymbolProcessor classSymbolProcessor in GetClassSymbols(context, receiver))
        {
            ConstructorDescriptor constructorDescriptor;
            try
            {
                constructorDescriptor = classSymbolProcessor.AnalyzeType();
            }
            catch (AutoConstructorException exception)
            {
                context.ReportDiagnostic(exception.Diagnostic);
                continue;
            }

            string symbolName = classSymbolProcessor.ClassSymbol.Name;
            string name;
            if (classNames.TryGetValue(symbolName, out int i))
            {
                name = $"{symbolName}{i + 1}";
                classNames[symbolName] = i + 1;
            }
            else
            {
                name = symbolName;
                classNames[symbolName] = 1;
            }

            context.AddSource(
                $"{name}.g.cs",
                SourceText.From(_sourceRenderer.Render(constructorDescriptor), Encoding.UTF8));
        }
    }

    private static IEnumerable<ClassSymbolProcessor> GetClassSymbols(
        GeneratorExecutionContext context,
        SyntaxReceiver receiver)
    {
        Compilation compilation = context.Compilation;

        return from type in receiver.CandidateClasses
               let classSymbol = compilation.GetSemanticModel(type.SyntaxTree).GetDeclaredSymbol(type)
               let attribute = classSymbol.GetAttribute<AutoConstructorAttribute>()
               where attribute != null
               select new ClassSymbolProcessor(classSymbol, type, attribute);
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not AttributeSyntax attribute)
                return;

            if (attribute?.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax)
                return;

            if (!_attributeSyntaxRegex.IsMatch(attribute.Name.ToString()))
                return;

            CandidateClasses.Add(classDeclarationSyntax);
        }
    }
}
