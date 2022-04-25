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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Accessibility = Attributes.Accessibility;

public record ConstructorDescriptor
{
    public ConstructorDescriptor(
        INamedTypeSymbol classSymbol,
        Accessibility accessibility,
        ImmutableArray<ConstructorParameter> constructorParameters,
        ImmutableArray<ConstructorParameter> baseClassConstructorParameters,
        string? documentation)
    {
        ClassSymbol = classSymbol;
        Accessibility = accessibility;
        ConstructorParameters = constructorParameters;
        BaseClassConstructorParameters = baseClassConstructorParameters;
        Documentation = documentation;
    }

    public INamedTypeSymbol ClassSymbol { get; }

    public Accessibility Accessibility { get; }

    public ImmutableArray<ConstructorParameter> ConstructorParameters { get; }

    public ImmutableArray<ConstructorParameter> BaseClassConstructorParameters { get; }

    public string? Documentation { get; }
}
