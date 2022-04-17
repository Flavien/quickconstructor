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

public class AutoConstructorGeneratorTests
{
    [Fact]
    public async Task EmptyClass()
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
    public async Task IncludedMembers()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                private readonly int field;
                public int Property { get; }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int field,
                    int property)
                {
                    this.field = field;
                    this.Property = property;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    [Fact]
    public async Task ExcludedMembers()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                private static readonly int fieldOne;
                private int fieldTwo;
                private readonly int fieldThree = 10;
                public static int PropertyOne { get; }
                public int PropertyTwo { get; set; }
                public int PropertyThree { get; private set; }
                public int PropertyFour { get => 10; }
                public int PropertyFive { get; } = 10;
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
    public async Task IncludeNonReadOnlyMembers()
    {
        string sourceCode = @"
            [AutoConstructor(IncludeNonReadOnlyMembers = true)]
            partial class TestClass
            {
                private int fieldTwo;
                public int PropertyTwo { get; set; }
                public int PropertyThree { get; private set; }
            }";

        string generatedCode = @"
            partial class TestClass
            {
                public TestClass(
                    int fieldTwo,
                    int propertyTwo,
                    int propertyThree)
                {
                    this.fieldTwo = fieldTwo;
                    this.PropertyTwo = propertyTwo;
                    this.PropertyThree = propertyThree;
                }
            }";

        await AssertGeneratedCode(sourceCode, generatedCode);
    }

    private static async Task AssertGeneratedCode(string sourceCode, string generatedCode)
    {
        string trimmedCode = StringOperations.TrimMultiline(generatedCode, 8);

        string eol = Environment.NewLine;
        string fullGeneratedCode = $"namespace TestNamespace{eol}{{{trimmedCode}{eol}}}";
        
        CSharpSourceGeneratorTest<AutoConstructorGenerator, XUnitVerifier> tester = new()
        {
            TestState =
            {
                Sources =
                {
                    $@"
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
