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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class AutoConstructorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        Dictionary<string, int> classNames = new(StringComparer.Ordinal);
        foreach (AutoConstructorBuilder augmentedType in GetClassSymbols(context, receiver))
        {
            string name;
            if (classNames.TryGetValue(augmentedType.Name, out int i))
            {
                name = $"{augmentedType.Name}{i + 1}";
                classNames[augmentedType.Name] = i + 1;
            }
            else
            {
                name = augmentedType.Name;
                classNames[augmentedType.Name] = 1;
            }

            context.AddSource(
                $"{name}.g.cs",
                SourceText.From(augmentedType.CreateConstructor(), Encoding.UTF8));
        }
    }

    private static IEnumerable<AutoConstructorBuilder> GetClassSymbols(
        GeneratorExecutionContext context,
        SyntaxReceiver receiver)
    {
        Compilation compilation = context.Compilation;

        return from type in receiver.CandidateClasses
               let model = compilation.GetSemanticModel(type.SyntaxTree)
               select model.GetDeclaredSymbol(type)
               into classSymbol
               let attribute = classSymbol.GetAttribute<AutoConstructorAttribute>()
               where attribute != null
               select new AutoConstructorBuilder(classSymbol, attribute);
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Any(list => list.Attributes.Any()))
            {
                CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}
