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

using Microsoft.CodeAnalysis;

public class DiagnosticDescriptors
{
    public static DiagnosticDescriptor ClassMustBePartial { get; } = new(
        id: "AC0001",
        title: "Classes decorated with the [AutoConstructor] attribute must be marked partial",
        messageFormat: "Declare '{0}' as partial or remove the [AutoConstructor] attribute.",
        category: "AutoConstructor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor DuplicateConstructorParameter { get; } = new(
        id: "AC0002",
        title: "Duplicate parameter name for auto-generated constructor",
        messageFormat: "The parameter '{0}' is duplicated in the auto-generated constructor for '{1}'.",
        category: "AutoConstructor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor BaseClassMustHaveAttribute { get; } = new(
        id: "AC0003",
        title: "The base class of classes decorated with [AutoConstructor] must be decorated with [AutoConstructor]"
            + " or have a parameterless constructor",
        messageFormat: "Decorate the parent class of '{0}' with [AutoConstructor].",
        category: "AutoConstructor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
