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

public class QuickConstructorBaseClassTests
{
    [Fact]
    public async Task BaseClass_ParentWithAttribute()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass : Parent
            {
                private readonly int fieldOne;
            }

            [QuickConstructor(Documentation = null)]
            partial class Parent
            {
                private readonly int parentClassField;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @parentClassField,
                    int @fieldOne)
                    : base(@parentClassField)
                {
                    this.@fieldOne = @fieldOne;
                }
            }";

        string parentClassGeneratedCode = @"
            partial class Parent
            {
                public Parent(
                    int @parentClassField)
                {
                    this.@parentClassField = @parentClassField;
                }
            }";

        await AssertGeneratedCode(sourceCode, ("TestClass", generatedCode), ("Parent", parentClassGeneratedCode));
    }

    [Fact]
    public async Task BaseClass_ParentWithDefaultConstructor()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class TestClass : System.Exception
            {
                private readonly int fieldOne;
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

        await AssertGeneratedCode(sourceCode, ("TestClass", generatedCode));
    }

    [Fact]
    public async Task BaseClass_Grandparent()
    {
        string sourceCode = @"
            [QuickConstructor(Documentation = null)]
            partial class Grandparent
            {
                private readonly int grandparentClassField;
            }

            [QuickConstructor(Documentation = null)]
            partial class TestClass : Parent
            {
                private readonly int fieldOne;
            }

            [QuickConstructor(Documentation = null)]
            partial class Parent : Grandparent
            {
                private readonly int parentClassField;
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int @grandparentClassField,
                    int @parentClassField,
                    int @fieldOne)
                    : base(@grandparentClassField, @parentClassField)
                {
                    this.@fieldOne = @fieldOne;
                }
            }";

        string parentClassGeneratedCode = @"
            partial class Parent
            {
                public Parent(
                    int @grandparentClassField,
                    int @parentClassField)
                    : base(@grandparentClassField)
                {
                    this.@parentClassField = @parentClassField;
                }
            }";

        string grandparentClassGeneratedCode = @"
            partial class Grandparent
            {
                public Grandparent(
                    int @grandparentClassField)
                {
                    this.@grandparentClassField = @grandparentClassField;
                }
            }";

        await AssertGeneratedCode(
            sourceCode,
            ("Grandparent", grandparentClassGeneratedCode),
            ("TestClass", generatedCode),
            ("Parent", parentClassGeneratedCode));
    }

    private static async Task AssertGeneratedCode(string sourceCode, params (string name, string code)[] expected)
    {
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
                }
            },
        };

        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorGenerator).Assembly);
        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorAttribute).Assembly);

        foreach ((string name, string code) in expected)
        {
            tester.TestState.GeneratedSources.Add((
                typeof(QuickConstructorGenerator),
                $"{name}.cs",
                SourceText.From(CreateExpectedFile(code), Encoding.UTF8)));
        }

        await tester.RunAsync();
    }

    private static string CreateExpectedFile(string generatedCode)
    {
        string fullGeneratedCode = $@"
            /// <auto-generated>
            ///   This code was generated by the {nameof(QuickConstructor)} source generator.
            /// </auto-generated>

            #nullable enable

            namespace TestNamespace
            {{
                {generatedCode}
            }}";

        return CodeFormatter.Format(fullGeneratedCode);
    }
}
