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

namespace AutoConstructor;

using System;
using System.Text.RegularExpressions;

public static class StringOperations
{
    private static readonly Regex _removeBlankLines = new(@"(\s*\n)+", RegexOptions.Compiled);
    private static readonly Regex _indent = new("(\n|\r\n)", RegexOptions.Compiled);

    public static string TrimMultiline(this string source, int indentation)
    {
        Regex trimSpaces = new($@"^[ ]{{{indentation}}}(.+?)$", RegexOptions.Multiline);

        return trimSpaces.Replace(source, "$1");
    }

    public static string RemoveBlankLines(this string source)
    {
        return _removeBlankLines.Replace(source, Environment.NewLine);
    }

    public static string Indent(this string source, int spaces)
    {
        return _indent.Replace(source, "$1" + new string(' ', spaces));
    }
}
