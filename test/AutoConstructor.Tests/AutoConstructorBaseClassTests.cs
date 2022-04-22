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
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using Xunit;

public class AutoConstructorBaseClassTests
{
    [Fact]
    public async Task BaseClass_ParentWithAttribute()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass : Parent
            {
                private readonly int fieldOne;
            }

            [AutoConstructor]
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
            [AutoConstructor]
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
            [AutoConstructor]
            partial class Grandparent
            {
                private readonly int grandparentClassField;
            }

            [AutoConstructor]
            partial class TestClass : Parent
            {
                private readonly int fieldOne;
            }

            [AutoConstructor]
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
                }
            },
        };

        tester.TestState.AdditionalReferences.Add(typeof(AutoConstructorGenerator).Assembly);

        foreach ((string name, string code) in expected)
        {
            tester.TestState.GeneratedSources.Add((
                typeof(AutoConstructorGenerator),
                $"{name}.g.cs",
                SourceText.From(CreateExpectedFile(code), Encoding.UTF8)));
        }

        await tester.RunAsync();
    }

    private static string CreateExpectedFile(string generatedCode)
    {
        string trimmedCode = generatedCode.TrimMultiline(8);

        string eol = Environment.NewLine;
        string fullGeneratedCode = $"#nullable enable{eol}namespace TestNamespace{eol}{{{trimmedCode}{eol}}}";
        return fullGeneratedCode;
    }
}
