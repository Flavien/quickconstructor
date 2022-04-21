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

namespace AutoConstructor.Tests;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using Xunit;

public class AutoConstructorGeneratedCodeTests
{
    [Fact]
    public async Task MemberSelection_EmptyClass()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass()
                {
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task MemberSelection_Default()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                private readonly int fieldOne;
                private static readonly int fieldTwo;
                private int fieldThree;
                private readonly int fieldFour = 10;

                public int PropertyOne { get; }
                public static int PropertyTwo { get; }
                public int PropertyThree { get; set; }
                public int PropertyFour { get; private set; }
                public int PropertyFive { get => 10; }
                public int PropertySix { get; } = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @propertyOne)
                {
                    this.@fieldOne = @fieldOne;
                    this.@PropertyOne = @propertyOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task MemberSelection_IncludeNonReadOnlyMembers()
    {
        string sourceCode = @"
            [AutoConstructor(IncludeNonReadOnlyMembers = true)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private static readonly int fieldTwo;
                private int fieldThree;
                private readonly int fieldFour = 10;

                public int PropertyOne { get; }
                public static int PropertyTwo { get; }
                public int PropertyThree { get; set; }
                public int PropertyFour { get; private set; }
                public int PropertyFive { get => 10; }
                public int PropertySix { get; } = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @fieldThree,
                    int @propertyOne,
                    int @propertyThree,
                    int @propertyFour)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldThree = @fieldThree;
                    this.@PropertyOne = @propertyOne;
                    this.@PropertyThree = @propertyThree;
                    this.@PropertyFour = @propertyFour;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task MemberSelection_AutoConstructorParameterAttribute()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                [AutoConstructorParameter]
                private readonly int fieldOne;
                [AutoConstructorParameter]
                private static readonly int fieldTwo;
                [AutoConstructorParameter]
                private int fieldThree;
                [AutoConstructorParameter]
                private readonly int fieldFour = 10;

                [AutoConstructorParameter]
                public int PropertyOne { get; }
                [AutoConstructorParameter]
                public static int PropertyTwo { get; }
                [AutoConstructorParameter]
                public int PropertyThree { get; set; }
                [AutoConstructorParameter]
                public int PropertyFour { get; private set; }
                [AutoConstructorParameter]
                public int PropertyFive { get => 10; }
                [AutoConstructorParameter]
                public int PropertySix { get; } = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @fieldThree,
                    int @propertyOne,
                    int @propertyThree,
                    int @propertyFour)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldThree = @fieldThree;
                    this.@PropertyOne = @propertyOne;
                    this.@PropertyThree = @propertyThree;
                    this.@PropertyFour = @propertyFour;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ParameterName_AutoConstructorParameter()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                [AutoConstructorParameter(Name = ""modifiedField"")]
                private readonly int fieldOne;

                [AutoConstructorParameter(Name = ""@modifiedProperty"")]
                public int PropertyOne { get; }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @modifiedField,
                    int @modifiedProperty)
                {
                    this.@fieldOne = @modifiedField;
                    this.@PropertyOne = @modifiedProperty;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ParameterName_Default()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                private readonly string @class;
                private readonly string _underscoreField;
                public string Return { get; }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    string @class,
                    string @underscoreField,
                    string @return)
                {
                    if (@class == null)
                        throw new System.ArgumentNullException(nameof(@class));
                    if (@underscoreField == null)
                        throw new System.ArgumentNullException(nameof(@underscoreField));
                    if (@return == null)
                        throw new System.ArgumentNullException(nameof(@return));
                    this.@class = @class;
                    this.@_underscoreField = @underscoreField;
                    this.@Return = @return;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task Rendering_ArgumentTypes()
    {
        string sourceCode = @"
            using System.Collections.Generic;
            using L = System.Collections.Generic.LinkedList<System.ApplicationException>;

            [AutoConstructor(NullChecks = NullChecksSettings.Never)]
            partial class TestClass<T> where T : class
            {
                private readonly T fieldOne;
                private readonly System.IO.Stream fieldTwo;
                private readonly System.Collections.Generic.List<System.IO.Stream> fieldThree;
                private readonly List<System.IO.Stream> fieldFour;
                private readonly L fieldFive;
            }";

        string generatedCode = @"
            partial class TestClass<T>
            {
                public TestClass(
                    T @fieldOne,
                    global::System.IO.Stream @fieldTwo,
                    global::System.Collections.Generic.List<global::System.IO.Stream> @fieldThree,
                    global::System.Collections.Generic.List<global::System.IO.Stream> @fieldFour,
                    global::System.Collections.Generic.LinkedList<global::System.ApplicationException> @fieldFive)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                    this.@fieldFour = @fieldFour;
                    this.@fieldFive = @fieldFive;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task Rendering_Attributes()
    {
        string sourceCode = @"
            using System.ComponentModel.DataAnnotations;

            [AutoConstructor]
            partial class TestClass
            {
                [Display(AutoGenerateField = true, Description = ""Applicable"")]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldOne;

                [AutoConstructorParameter]
                [Display(AutoGenerateField = true, Description = ""Applicable"")]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldTwo;

                [AutoConstructorParameter(IncludeAttributes = false)]
                [Display(AutoGenerateField = true, Description = ""Applicable"")]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldThree;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    [System.ComponentModel.DataAnnotations.DisplayAttribute(AutoGenerateField = true, Description = ""Applicable"")] int @fieldOne,
                    [System.ComponentModel.DataAnnotations.DisplayAttribute(AutoGenerateField = true, Description = ""Applicable"")] int @fieldTwo,
                    int @fieldThree)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task Rendering_NestedClass()
    {
        string sourceCode = @"
            partial class Parent
            {
                [AutoConstructor]
                partial class TestClass
                {
                    private readonly int fieldOne;
                }
            }";

        string generatedCode = @"
            partial class Parent
            {
                partial class TestClass
                {
                    public TestClass(
                        int @fieldOne)
                    {
                        this.@fieldOne = @fieldOne;
                    }
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task NullChecks_NonNullableReferencesOnly()
    {
        string sourceCode = @"
            [AutoConstructor(NullChecks = NullChecksSettings.NonNullableReferencesOnly)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private readonly int? fieldTwo;
                private readonly string fieldThree;
                private readonly string? fieldFour;
                private readonly System.Collections.Generic.List<string?> fieldFive;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int? @fieldTwo,
                    string @fieldThree,
                    string? @fieldFour,
                    global::System.Collections.Generic.List<string?> @fieldFive)
                {
                    if (@fieldThree == null)
                        throw new System.ArgumentNullException(nameof(@fieldThree));
                    if (@fieldFive == null)
                        throw new System.ArgumentNullException(nameof(@fieldFive));
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                    this.@fieldFour = @fieldFour;
                    this.@fieldFive = @fieldFive;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task NullChecks_Always()
    {
        string sourceCode = @"
            [AutoConstructor(NullChecks = NullChecksSettings.Always)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private readonly int? fieldTwo;
                private readonly string fieldThree;
                private readonly string? fieldFour;
                private readonly System.Collections.Generic.List<string?> fieldFive;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int? @fieldTwo,
                    string @fieldThree,
                    string? @fieldFour,
                    global::System.Collections.Generic.List<string?> @fieldFive)
                {
                    if (@fieldThree == null)
                        throw new System.ArgumentNullException(nameof(@fieldThree));
                    if (@fieldFour == null)
                        throw new System.ArgumentNullException(nameof(@fieldFour));
                    if (@fieldFive == null)
                        throw new System.ArgumentNullException(nameof(@fieldFive));
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                    this.@fieldFour = @fieldFour;
                    this.@fieldFive = @fieldFive;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task NullChecks_Never()
    {
        string sourceCode = @"
            [AutoConstructor(NullChecks = NullChecksSettings.Never)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private readonly int? fieldTwo;
                private readonly string fieldThree;
                private readonly string? fieldFour;
                private readonly System.Collections.Generic.List<string?> fieldFive;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int? @fieldTwo,
                    string @fieldThree,
                    string? @fieldFour,
                    global::System.Collections.Generic.List<string?> @fieldFive)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                    this.@fieldFour = @fieldFour;
                    this.@fieldFive = @fieldFive;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task NullChecks_NullableReferenceTypesDisabled()
    {
        string sourceCode = @"
            #nullable disable
            [AutoConstructor(NullChecks = NullChecksSettings.NonNullableReferencesOnly)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private readonly int? fieldTwo;
                private readonly string fieldThree;
                private readonly string? fieldFour;
                private readonly System.Collections.Generic.List<string?> fieldFive;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int? @fieldTwo,
                    string @fieldThree,
                    string? @fieldFour,
                    global::System.Collections.Generic.List<string?> @fieldFive)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                    this.@fieldThree = @fieldThree;
                    this.@fieldFour = @fieldFour;
                    this.@fieldFive = @fieldFive;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("System.Uri")]
    [InlineData("class?")]
    [InlineData("System.Uri?")]
    [InlineData("notnull")]
    public async Task NullChecks_Generics(string constraint)
    {
        string sourceCode = $@"
            [AutoConstructor(NullChecks = NullChecksSettings.NonNullableReferencesOnly)]
            partial class TestClass<T> where T : {constraint}
            {{
                private readonly T fieldOne;
            }}";

        string generatedCode = @"
            partial class TestClass<T>
            {
                public TestClass(
                    T @fieldOne)
                {
                    if (@fieldOne == null)
                        throw new System.ArgumentNullException(nameof(@fieldOne));
                    this.@fieldOne = @fieldOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    private static async Task AssertGeneratedCode(string sourceCode, string generatedCode)
    {
        string trimmedCode = generatedCode.TrimMultiline(8);

        string eol = Environment.NewLine;
        string fullGeneratedCode = $"#nullable enable{eol}namespace TestNamespace{eol}{{{trimmedCode}{eol}}}";

        CSharpSourceGeneratorTest<AutoConstructorGenerator, XUnitVerifier> tester = new()
        {
            TestState =
            {
                Sources =
                {
                    $@"
                    #nullable enable
                    using AutoConstructor;
                    namespace TestNamespace
                    {{
                        {sourceCode}
                    }}"
                },
                GeneratedSources =
                {
                    (
                        typeof(AutoConstructorGenerator),
                        $"TestClass.g.cs",
                        SourceText.From(fullGeneratedCode, Encoding.UTF8)
                    ),
                }
            },
        };

        tester.TestState.AdditionalReferences.Add(typeof(AutoConstructorGenerator).Assembly);

        await tester.RunAsync();
    }
}
