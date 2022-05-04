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

namespace QuickConstructor.Tests;

using System.Text;
using System.Threading.Tasks;
using CSharpier;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using QuickConstructor.Attributes;
using QuickConstructor.Generator;
using Xunit;
using Accessibility = QuickConstructor.Attributes.Accessibility;

public class GeneratedCodeTests
{
    [Fact]
    public async Task IncludeFields_ReadOnlyFields()
    {
        string sourceCode = @"
            [QuickConstructor(Fields = IncludeFields.ReadOnlyFields, Documentation = null)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private static readonly int fieldTwo;
                private int fieldThree;
                private readonly int fieldFour = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne)
                {
                    this.@fieldOne = @fieldOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task IncludeFields_AllFields()
    {
        string sourceCode = @"
            [QuickConstructor(Fields = IncludeFields.AllFields, Documentation = null)]
            partial class TestClass
            {
                private readonly int fieldOne;
                private static readonly int fieldTwo;
                private int fieldThree;
                private readonly int fieldFour = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @fieldThree)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldThree = @fieldThree;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task IncludeFields_QuickConstructorParameterAttribute()
    {
        string sourceCode = @"
            [QuickConstructor(Fields = IncludeFields.ReadOnlyFields, Documentation = null)]
            partial class TestClass
            {
                [QuickConstructorParameter]
                private readonly int fieldOne;
                [QuickConstructorParameter]
                private static readonly int fieldTwo;
                [QuickConstructorParameter]
                private int fieldThree;
                [QuickConstructorParameter]
                private readonly int fieldFour = 10;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @fieldThree)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldThree = @fieldThree;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task IncludeProperties_None()
    {
        string sourceCode = @"
            [QuickConstructor(Properties = IncludeProperties.None, Documentation = null)]
            partial class TestClass
            {
                public int PropertyOne { get; }
                public static int PropertyTwo { get; }
                public int PropertyThree { get; set; }
                public int PropertyFour { get; private set; }
                public int PropertyFive { get => 10; }
                public int PropertySix { get; } = 10;
                public int PropertySeven
                {
                    get => 0;
                    set { }
                }
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
    public async Task IncludeProperties_ReadOnlyProperties()
    {
        string sourceCode = @"
            [QuickConstructor(Properties = IncludeProperties.ReadOnlyProperties, Documentation = null)]
            partial class TestClass
            {
                public int PropertyOne { get; }
                public static int PropertyTwo { get; }
                public int PropertyThree { get; set; }
                public int PropertyFour { get; private set; }
                public int PropertyFive { get => 10; }
                public int PropertySix { get; } = 10;
                public int PropertySeven
                {
                    get => 0;
                    set { }
                }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @propertyOne)
                {
                    this.@PropertyOne = @propertyOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task IncludeProperties_AllProperties()
    {
        string sourceCode = @"
            [QuickConstructor(Properties = IncludeProperties.AllProperties, Documentation = null)]
            partial class TestClass
            {
                public int PropertyOne { get; }
                public static int PropertyTwo { get; }
                public int PropertyThree { get; set; }
                public int PropertyFour { get; private set; }
                public int PropertyFive { get => 10; }
                public int PropertySix { get; } = 10;
                public int PropertySeven
                {
                    get => 0;
                    set { }
                }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @propertyOne,
                    int @propertyThree,
                    int @propertyFour,
                    int @propertySeven)
                {
                    this.@PropertyOne = @propertyOne;
                    this.@PropertyThree = @propertyThree;
                    this.@PropertyFour = @propertyFour;
                    this.@PropertySeven = @propertySeven;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task IncludeProperties_QuickConstructorParameterAttribute()
    {
        string sourceCode = @"
            [QuickConstructor(Properties = IncludeProperties.None, Documentation = null)]
            partial class TestClass
            {
                [QuickConstructorParameter]
                public int PropertyOne { get; }
                [QuickConstructorParameter]
                public static int PropertyTwo { get; }
                [QuickConstructorParameter]
                public int PropertyThree { get; set; }
                [QuickConstructorParameter]
                public int PropertyFour { get; private set; }
                [QuickConstructorParameter]
                public int PropertyFive { get => 10; }
                [QuickConstructorParameter]
                public int PropertySix { get; } = 10;
                [QuickConstructorParameter]
                public int PropertySeven
                {
                    get => 0;
                    set { }
                }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @propertyOne,
                    int @propertyThree,
                    int @propertyFour,
                    int @propertySeven)
                {
                    this.@PropertyOne = @propertyOne;
                    this.@PropertyThree = @propertyThree;
                    this.@PropertyFour = @propertyFour;
                    this.@PropertySeven = @propertySeven;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ParameterName_OverrideName()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                [QuickConstructorParameter(Name = ""modifiedField"")]
                private readonly int fieldOne;

                [QuickConstructorParameter(Name = ""@modifiedProperty"")]
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
    public async Task ParameterName_ToCamelCase()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                private readonly string _underscoreField;
                private readonly string number1;
                private readonly string É;
                private readonly string 你好;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    string @underscoreField,
                    string @number1,
                    string @é,
                    string @你好)
                {
                    if (@underscoreField == null)
                        throw new global::System.ArgumentNullException(nameof(@underscoreField));

                    if (@number1 == null)
                        throw new global::System.ArgumentNullException(nameof(@number1));

                    if (@é == null)
                        throw new global::System.ArgumentNullException(nameof(@é));

                    if (@你好 == null)
                        throw new global::System.ArgumentNullException(nameof(@你好));

                    this.@_underscoreField = @underscoreField;
                    this.@number1 = @number1;
                    this.@É = @é;
                    this.@你好 = @你好;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ParameterName_ReservedKeyword()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                private readonly string @class;
                public string Return { get; }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    string @class,
                    string @return)
                {
                    if (@class == null)
                        throw new global::System.ArgumentNullException(nameof(@class));

                    if (@return == null)
                        throw new global::System.ArgumentNullException(nameof(@return));

                    this.@class = @class;
                    this.@Return = @return;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ParameterName_Unchanged()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                private readonly string @_;
                private readonly string _1;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    string @_,
                    string @_1)
                {
                    if (@_ == null)
                        throw new global::System.ArgumentNullException(nameof(@_));

                    if (@_1 == null)
                        throw new global::System.ArgumentNullException(nameof(@_1));

                    this.@_ = @_;
                    this.@_1 = @_1;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task Rendering_EmptyClass()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
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
    public async Task Rendering_ArgumentTypes()
    {
        string sourceCode = @"
            using System.Collections.Generic;
            using L = System.Collections.Generic.LinkedList<System.ApplicationException>;

            [QuickConstructor(NullChecks = NullChecks.Never, Documentation = null)]
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
            using System.Collections.Generic;

            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                [DataType(""Applicable"", ErrorMessageResourceType = typeof(List<int>))]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldOne;

                [QuickConstructorParameter]
                [DataType(""Applicable"", ErrorMessageResourceType = typeof(List<int>))]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldTwo;

                [QuickConstructorParameter(IncludeAttributes = false)]
                [DataType(""Applicable"", ErrorMessageResourceType = typeof(List<int>))]
                [DisplayFormat(DataFormatString = ""Not applicable"")]
                private readonly int fieldThree;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    [global::System.ComponentModel.DataAnnotations.DataTypeAttribute(""Applicable"", ErrorMessageResourceType = typeof(System.Collections.Generic.List<int>))] int @fieldOne,
                    [global::System.ComponentModel.DataAnnotations.DataTypeAttribute(""Applicable"", ErrorMessageResourceType = typeof(System.Collections.Generic.List<int>))] int @fieldTwo,
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
                [QuickConstructor]
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
                    /// <summary>
                    /// Initializes a new instance of the <see cref=""TestClass"" /> class.
                    /// </summary>
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
    public async Task Rendering_Documentation()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = ""This is a constructor for {0}."")]
            partial class TestClass<T> where T : struct
            {
                private readonly T fieldOne;
            }";

        string generatedCode = @"
            partial class TestClass<T>
            {
                /// <summary>
                /// This is a constructor for <see cref=""TestClass{T}"" />.
                /// </summary>
                public TestClass(
                    T @fieldOne)
                {
                    this.@fieldOne = @fieldOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Theory]
    [InlineData(Accessibility.Public, "public")]
    [InlineData(Accessibility.Protected, "protected")]
    [InlineData(Accessibility.Private, "private")]
    [InlineData(Accessibility.Internal, "internal")]
    public async Task Rendering_Accessibility(Accessibility accessibility, string keyword)
    {
        string sourceCode = $@"
            [QuickConstructor(ConstructorAccessibility = Accessibility.{accessibility}, Documentation = null)]
            partial class TestClass
            {{
                private readonly int fieldOne;
            }}";

        string generatedCode = $@"
            partial class TestClass
            {{
                {keyword} TestClass(
                    int @fieldOne)
                {{
                    this.@fieldOne = @fieldOne;
                }}
            }}";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task SyntaxTree_Partial()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass
            {
                private readonly int fieldOne;
            }

            partial class TestClass
            {
                private readonly int fieldTwo;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @fieldOne,
                    int @fieldTwo)
                {
                    this.@fieldOne = @fieldOne;
                    this.@fieldTwo = @fieldTwo;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Theory]
    [InlineData("QuickConstructor")]
    [InlineData("QuickConstructor()")]
    [InlineData("QuickConstructorAttribute")]
    [InlineData("QuickConstructor.Attributes.QuickConstructor")]
    [InlineData("QuickConstructor.Attributes.QuickConstructorAttribute")]
    [InlineData("global::QuickConstructor.Attributes.QuickConstructor")]
    [InlineData("global::QuickConstructor.Attributes.QuickConstructorAttribute")]
    [InlineData("global::QuickConstructor.Attributes.QuickConstructorAttribute()")]
    public async Task SyntaxTree_AttributeSyntax(string attributeSyntax)
    {
        string sourceCode = $@"
            [{attributeSyntax}]
            partial class TestClass
            {{
                private readonly int fieldOne;
            }}";

        string generatedCode = @"
            partial class TestClass
            {
                /// <summary>
                /// Initializes a new instance of the <see cref=""TestClass"" /> class.
                /// </summary>
                public TestClass(
                    int @fieldOne)
                {
                    this.@fieldOne = @fieldOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task NullChecks_NonNullableReferencesOnly()
    {
        string sourceCode = @"
            [QuickConstructor(NullChecks = NullChecks.NonNullableReferencesOnly, Documentation = null)]
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
                        throw new global::System.ArgumentNullException(nameof(@fieldThree));

                    if (@fieldFive == null)
                        throw new global::System.ArgumentNullException(nameof(@fieldFive));

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
            [QuickConstructor(NullChecks = NullChecks.Always, Documentation = null)]
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
                        throw new global::System.ArgumentNullException(nameof(@fieldThree));

                    if (@fieldFour == null)
                        throw new global::System.ArgumentNullException(nameof(@fieldFour));

                    if (@fieldFive == null)
                        throw new global::System.ArgumentNullException(nameof(@fieldFive));

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
            [QuickConstructor(NullChecks = NullChecks.Never, Documentation = null)]
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
            [QuickConstructor(NullChecks = NullChecks.NonNullableReferencesOnly, Documentation = null)]
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
            [QuickConstructor(NullChecks = NullChecks.NonNullableReferencesOnly, Documentation = null)]
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
                        throw new global::System.ArgumentNullException(nameof(@fieldOne));

                    this.@fieldOne = @fieldOne;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    private static async Task AssertGeneratedCode(string sourceCode, string generatedCode)
    {
        string fullGeneratedCode = CreateExpectedFile(generatedCode);

        CSharpIncrementalGeneratorTest<QuickConstructorGenerator, XUnitVerifier> tester = new()
        {
            TestState =
            {
                Sources =
                {
                    $@"
                    #nullable enable
                    using QuickConstructor.Attributes;
                    namespace TestNamespace
                    {{
                        {sourceCode}
                    }}"
                },
                GeneratedSources =
                {
                    (
                        typeof(QuickConstructorGenerator),
                        $"TestClass.cs",
                        SourceText.From(fullGeneratedCode, Encoding.UTF8)
                    ),
                }
            },
        };

        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorGenerator).Assembly);
        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorAttribute).Assembly);

        await tester.RunAsync();
    }

    private static string CreateExpectedFile(string generatedCode)
    {
        string fullGeneratedCode = $@"
            /// <auto-generated>
            /// This code was generated by the {nameof(QuickConstructor)} source generator.
            /// </auto-generated>

            namespace TestNamespace
            {{
                {generatedCode}
            }}";

        return CodeFormatter.Format(fullGeneratedCode);
    }
}
