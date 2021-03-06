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

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using QuickConstructor.Attributes;
using QuickConstructor.Generator;
using Xunit;

public class DiagnosticsTests
{
    [Fact]
    public async Task Diagnostics_DuplicateConstructorParameter()
    {
        string sourceCode = @"
            [QuickConstructor]
            partial class TestClass
            {
                private readonly int value;
                public int Value { get; }
            }";

        await AssertDiagnostic(
            sourceCode,
            new DiagnosticResult(DiagnosticDescriptors.DuplicateConstructorParameter)
                .WithSpan(8, 27, 8, 36)
                .WithArguments("value", "TestClass"));
    }

    [Theory]
    [InlineData("1value")]
    [InlineData("a$")]
    [InlineData("!a")]
    [InlineData("")]
    [InlineData("with space")]
    public async Task Diagnostics_InvalidParameterName(string name)
    {
        string sourceCode = $@"
            [QuickConstructor]
            partial class TestClass
            {{
                [QuickConstructorParameter(Name = ""{name}"")]
                private readonly int value;
            }}";

        await AssertDiagnostic(
            sourceCode,
            new DiagnosticResult(DiagnosticDescriptors.InvalidParameterName)
                .WithSpan(8, 27, 8, 36)
                .WithArguments(name, "TestClass"));
    }

    private static async Task AssertDiagnostic(string sourceCode, DiagnosticResult diagnostic)
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
                },
                GeneratedSources = { },
                ExpectedDiagnostics =
                {
                    diagnostic
                }
            },
        };

        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorGenerator).Assembly);
        tester.TestState.AdditionalReferences.Add(typeof(QuickConstructorAttribute).Assembly);

        await tester.RunAsync();
    }
}
