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
using AutoConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Accessibility = Attributes.Accessibility;

public record ConstructorDescriptor
{
    public ConstructorDescriptor(
        INamedTypeSymbol classSymbol,
        Accessibility accessibility,
        IReadOnlyList<ConstructorParameter> constructorParameters,
        IReadOnlyList<ConstructorParameter> baseClassConstructorParameters)
    {
        ClassSymbol = classSymbol;
        Accessibility = accessibility;
        ConstructorParameters = constructorParameters;
        BaseClassConstructorParameters = baseClassConstructorParameters;
    }

    public INamedTypeSymbol ClassSymbol { get; }

    public Accessibility Accessibility { get; }

    public IReadOnlyList<ConstructorParameter> ConstructorParameters { get; }

    public IReadOnlyList<ConstructorParameter> BaseClassConstructorParameters { get; }
}
