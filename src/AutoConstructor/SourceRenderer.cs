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
using CSharpier;
using Microsoft.CodeAnalysis;

public class SourceRenderer
{
    private static readonly SymbolDisplayFormat _parameterFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public string Render(ConstructorDescriptor constructorDescriptor)
    {
        INamedTypeSymbol classSymbol = constructorDescriptor.ClassSymbol;
        IReadOnlyList<ConstructorParameter> parameters = constructorDescriptor.ConstructorParameters;
        IReadOnlyList<ConstructorParameter> baseClassParameters = constructorDescriptor.BaseClassConstructorParameters;

        IEnumerable<ConstructorParameter> allParameters = baseClassParameters.Concat(parameters);
        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        string accessibility = GetAccessModifier(constructorDescriptor.Accessibility);
        string classDeclaration = classSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        string parameterDeclarations = string.Join(",", allParameters.Select(parameter => RenderParameter(parameter)));
        string baseClassConstructor = CreateBaseClassConstructor(baseClassParameters);
        string nullChecks = string.Concat(parameters.Select(parameter => RenderNullCheck(parameter)));
        string assignments = string.Concat(parameters.Select(parameter => RenderAssignment(parameter)));
        string namespaceContents = $@"
                partial class {classDeclaration}
                {{
                    {accessibility} {classSymbol.Name}({parameterDeclarations})
                        {baseClassConstructor}
                    {{
                        {nullChecks}

                        {assignments}
                    }}
                }}";

        INamedTypeSymbol currentSymbol = classSymbol;
        while (currentSymbol.ContainingType != null)
        {
            currentSymbol = currentSymbol.ContainingType;
            namespaceContents = $@"
                partial class {currentSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}
                {{
                    {namespaceContents}
                }}";
        }

        string source = $@"
            #nullable enable

            namespace {namespaceName}
            {{
                {namespaceContents}
            }}";

        return CodeFormatter.Format(source);
    }

    public string RenderParameter(ConstructorParameter parameter)
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine();

        foreach (AttributeData attribute in parameter.Attributes)
            stringBuilder.Append($"[{attribute}] ");

        stringBuilder.Append(parameter.Type.ToDisplayString(_parameterFormat));
        stringBuilder.Append(" @");
        stringBuilder.Append(parameter.ParameterName);

        return stringBuilder.ToString();
    }

    private string RenderNullCheck(ConstructorParameter parameter)
    {
        if (!parameter.NullCheck)
            return string.Empty;

        return @$"
            if (@{ parameter.ParameterName } == null)
                throw new System.ArgumentNullException(nameof(@{ parameter.ParameterName }));
        ";
    }

    private string RenderAssignment(ConstructorParameter parameter)
    {
        return $@"
            this.@{parameter.Symbol.Name} = @{parameter.ParameterName};";
    }

    private string CreateBaseClassConstructor(IReadOnlyList<ConstructorParameter> baseClassParameters)
    {
        if (baseClassParameters.Count == 0)
            return string.Empty;

        IEnumerable<string> argumentList = baseClassParameters.Select(argument => "@" + argument.ParameterName);

        return $": base({string.Join(", ", argumentList)})";
    }

    private string GetAccessModifier(Attributes.Accessibility accessibility)
    {
        return accessibility switch
        {
            Attributes.Accessibility.Private => "private",
            Attributes.Accessibility.Internal => "internal",
            Attributes.Accessibility.Protected => "protected",
            _ => "public"
        };
    }
}
