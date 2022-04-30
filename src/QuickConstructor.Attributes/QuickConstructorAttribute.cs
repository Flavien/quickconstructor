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
/// Specifies that a constructor should be automatically generated.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[Conditional("INCLUDE_AUTO_CONSTRUCTOR_ATTRIBUTES")]
public sealed class QuickConstructorAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating which fields should be initialized in the constructor.
    /// </summary>
    public IncludeFields Fields { get; set; } = IncludeFields.ReadOnlyFields;

    /// <summary>
    /// Gets or sets a value indicating which properties should be initialized in the constructor.
    /// </summary>
    public IncludeProperties Properties { get; set; } = IncludeProperties.ReadOnlyProperties;

    /// <summary>
    /// Gets or sets a value indicating how null checks should be emitted for the constructor parameters.
    /// </summary>
    public NullChecks NullChecks { get; set; } = NullChecks.NonNullableReferencesOnly;

    /// <summary>
    /// Gets or sets a value indicating which accessibility the constructor should have.
    /// </summary>
    public Accessibility ConstructorAccessibility { get; set; } = Accessibility.Public;

    /// <summary>
    /// Gets or sets the summary text used when emitting XML documentation for the constructor.
    /// </summary>
    public string? Documentation { get; set; } = "Initializes a new instance of the {0} class.";
}
