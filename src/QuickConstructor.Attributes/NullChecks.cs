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

/// <summary>
/// Represents a strategy for emitting null checks in an automatically generated constructor.
/// </summary>
public enum NullChecks
{
    /// <summary>
    /// Null checks are generated for any field or property whose type is a reference type.
    /// </summary>
    Always,
    /// <summary>
    /// Null checks are not generated for this constructor.
    /// </summary>
    Never,
    /// <summary>
    /// When null-state analysis is enabled (C# 8.0 and later), a null check will be generated only if a type is
    /// marked as non-nullable. When null-state analysis is disabled, no null check is generated.
    /// </summary>
    NonNullableReferencesOnly
}
