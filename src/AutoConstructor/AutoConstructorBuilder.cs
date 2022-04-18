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

public class AutoConstructorBuilder
{
    private readonly SourceRenderer _sourceRenderer = new();

    private readonly INamedTypeSymbol _classSymbol;
    private readonly AutoConstructorAttribute _attribute;

    public AutoConstructorBuilder(INamedTypeSymbol classSymbol, AttributeData attributeData)
    {
        _classSymbol = classSymbol;
        _attribute = CreateAttribute<AutoConstructorAttribute>(attributeData);
    }

    public string Name { get => _classSymbol.Name; }

    public string CreateConstructor()
    {
        TypeAnalyzer typeAnalyzer = new(_classSymbol, _attribute);
        IList<ConstructorParameter> members = typeAnalyzer.GetMembers();
        return _sourceRenderer.Render(_classSymbol, members);
    }

    private static T CreateAttribute<T>(AttributeData attributeData)
    {
        T attribute = Activator.CreateInstance<T>();
        foreach (KeyValuePair<string, TypedConstant> pair in attributeData.NamedArguments)
        {
            typeof(T).GetProperty(pair.Key).SetValue(attribute, pair.Value.Value);
        }

        return attribute;
    }
}
