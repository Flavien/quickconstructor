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

public class SourceRenderer
{
    public string Render(INamedTypeSymbol classSymbol, IList<ConstructorParameter> parameters)
    {
        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        string classDeclaration = classSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string parameterDeclarations = string.Join(",", parameters.Select(parameter => RenderParameter(parameter)));
        string assignments = string.Concat(parameters.Select(parameter => RenderAssignment(parameter)));
        StringBuilder source = new($@"
            namespace {namespaceName}
            {{
                partial class {classDeclaration}
                {{
                    public {classSymbol.Name}({parameterDeclarations})
                    {{
                        {assignments}
                    }}
                }}
            }}");

        return source.ToString()
            .TrimMultiline(12)
            .RemoveBlankLines()
            .TrimStart('\r', '\n');
    }

    public string RenderParameter(ConstructorParameter parameter)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine();
        stringBuilder.Append(' ', 24);

        foreach (AttributeData attribute in parameter.Attributes)
            stringBuilder.Append($"[{attribute}] ");

        stringBuilder.Append(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        stringBuilder.Append(" @");
        stringBuilder.Append(parameter.ParameterName);

        return stringBuilder.ToString();
    }

    private string RenderAssignment(ConstructorParameter parameter)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine();
        stringBuilder.Append(' ', 24);
        stringBuilder.Append("this.@");
        stringBuilder.Append(parameter.Symbol.Name);
        stringBuilder.Append(" = @");
        stringBuilder.Append(parameter.ParameterName);
        stringBuilder.Append(';');

        return stringBuilder.ToString();
    }
}
