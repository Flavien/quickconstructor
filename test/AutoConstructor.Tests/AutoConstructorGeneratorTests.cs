// Copyright 2021 Flavien Charlon
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
            [AutoConstructor.AutoConstructor]
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

    private static async Task AssertGeneratedCode(string sourceCode, string generatedCode)
    {
        Regex trimSpaces = new(@"^[ ]{8}(.+?)$", RegexOptions.Multiline);

        string eol = Environment.NewLine;
        string fullGeneratedCode = $"namespace TestNamespace{eol}{{{trimSpaces.Replace(generatedCode, "$1")}{eol}}}";
        
        CSharpSourceGeneratorTest<AutoConstructorGenerator, XUnitVerifier> tester = new()
        {
            TestState =
            {
                Sources =
                {
                    $@"namespace TestNamespace
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
