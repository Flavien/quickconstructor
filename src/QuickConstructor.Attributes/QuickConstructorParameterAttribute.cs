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

namespace QuickConstructor.Attributes;

using System;
using System.Diagnostics;

/// <summary>
/// Specifies that a field or property should be initialized through the generated constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[Conditional("INCLUDE_AUTO_CONSTRUCTOR_ATTRIBUTES")]
public class QuickConstructorParameterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name to give to the constructor parameter from which the field or property is initialized.
    /// If null, the name will be derived from the field or property name by turning it to camel case.
    /// </summary>
    public string? Name { get; set; } = null;

    /// <summary>
    /// Gets or sets a boolean value that indicates whether to copy attributes applied to the field or property to the
    /// constructor parameter.
    /// </summary>
    public bool IncludeAttributes { get; set; } = true;
}
