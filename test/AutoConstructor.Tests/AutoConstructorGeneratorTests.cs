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
    public async Task Default()
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
                    int fieldOne,
                    int propertyOne)
                {
                    this.fieldOne = fieldOne;
                    this.PropertyOne = propertyOne;
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
                    int fieldOne,
                    int fieldThree,
                    int propertyOne,
                    int propertyThree,
                    int propertyFour)
                {
                    this.fieldOne = fieldOne;
                    this.fieldThree = fieldThree;
                    this.PropertyOne = propertyOne;
                    this.PropertyThree = propertyThree;
                    this.PropertyFour = propertyFour;
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
