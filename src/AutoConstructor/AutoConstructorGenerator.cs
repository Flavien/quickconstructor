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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        IEnumerable<INamedTypeSymbol> classSymbols = GetClassSymbols(context, receiver);
        Dictionary<string, int> classNames = new();
        foreach (INamedTypeSymbol classSymbol in classSymbols)
        {
            string name;
            if (classNames.TryGetValue(classSymbol.Name, out int i))
            {
                name = $"{classSymbol.Name}{i + 1}";
                classNames[classSymbol.Name] = i + 1;
            }
            else
            {
                name = classSymbol.Name;
                classNames[classSymbol.Name] = 1;
            }

            AutoConstructorBuilder autoConstructorBuilder = new(classSymbol);

            context.AddSource(
                $"{name}.g.cs",
                SourceText.From(autoConstructorBuilder.CreateConstructor(), Encoding.UTF8));
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetClassSymbols(
        GeneratorExecutionContext context,
        SyntaxReceiver receiver)
    {
        Compilation compilation = context.Compilation;

        return from type in receiver.CandidateClasses
               let model = compilation.GetSemanticModel(type.SyntaxTree)
               select model.GetDeclaredSymbol(type)
               into classSymbol
               where HasAttribute(classSymbol, nameof(AutoConstructorAttribute))
               select classSymbol;
    }

    private static bool HasAttribute(ISymbol symbol, string name)
    {
        return symbol.GetAttributes().Any(x => x.AttributeClass?.Name == name);
    }
}
