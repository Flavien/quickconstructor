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

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

public class AutoConstructorDiagnosticsTests
{
    [Fact]
    public async Task Diagnostics_ClassMustBePartial()
    {
        string sourceCode = @"
            [AutoConstructor]
            public class TestClass
            {
            }";

        await AssertDiagnostic(
            sourceCode,
            new DiagnosticResult(DiagnosticDescriptors.ClassMustBePartial)
                .WithSpan(7, 13, 10, 14)
                .WithArguments("TestClass"));
    }

    [Fact]
    public async Task Diagnostics_DuplicateConstructorParameter()
    {
        string sourceCode = @"
            [AutoConstructor]
            partial class TestClass
            {
                private readonly int value;
                public int Value { get; }
            }";

        await AssertDiagnostic(
            sourceCode,
            new DiagnosticResult(DiagnosticDescriptors.DuplicateConstructorParameter)
                .WithSpan(7, 13, 12, 14)
                .WithArguments("value", "TestClass"));
    }

    private static async Task AssertDiagnostic(string sourceCode, DiagnosticResult diagnostic)
    {
        CSharpIncrementalGeneratorTest<AutoConstructorGenerator, XUnitVerifier> tester = new()
        {
            TestState =
            {
                Sources =
                {
                    $@"
                    #nullable enable
                    using AutoConstructor.Attributes;
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

        tester.TestState.AdditionalReferences.Add(typeof(AutoConstructorGenerator).Assembly);

        await tester.RunAsync();
    }
}
