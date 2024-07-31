// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Analyzers.Tests
{
    public class TypesShouldBeUpperCaseTests
    {
        [Theory]
        [InlineData("IReadOnlyList<string>", true)]
        [InlineData("IList<string>", true)]
        [InlineData("string[]", false)]
        [InlineData("List<string>", false)]
        [InlineData("ImmutableArray<string>", false)]
        public async void Test1(string listType, bool expectDiagnostic)
        {
            var test = new CSharpAnalyzerTest<TypesShouldBeUpperCaseAnalyzer, DefaultVerifier>
            {
                TestCode = @"
using System.Collections.Generic;
using System.Collections.Immutable;
class Class1
{
    void Method(" + listType + @" list)
    {
        foreach (var str in " + (expectDiagnostic ? "[|list|]" : "list") + @") { }
    }
}",
                ReferenceAssemblies = ReferenceAssemblies.Default
            };

            await test.RunAsync();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Test2(bool hasStructEnumerator)
        {
            var test = new CSharpAnalyzerTest<TypesShouldBeUpperCaseAnalyzer, DefaultVerifier>
            {
                TestCode = @"
using System;
using System.Collections.Generic;
public struct StructEnumerator
{
    public string Current => null;
    public bool MoveNext() => false;
}
public class Class1
{
    public " + (hasStructEnumerator ? "StructEnumerator" : "IEnumerator<string>") + @" GetEnumerator() => throw new NotImplementedException();

    public void Test()
    {
        foreach (var value in " + (hasStructEnumerator ? "this" : "[|this|]") + @") { }
    }
}
"
            };

            await test.RunAsync();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BaseClass(bool hasStructEnumerator)
        {
            var test = new CSharpAnalyzerTest<TypesShouldBeUpperCaseAnalyzer, DefaultVerifier>
            {
                TestCode = @"
using System;
using System.Collections.Generic;
public struct StructEnumerator
{
    public string Current => null;
    public bool MoveNext() => false;
}
public abstract class Class1
{
    public " + (hasStructEnumerator ? "StructEnumerator" : "IEnumerator<string>") + @" GetEnumerator() => throw new NotImplementedException();
}
public class Class2 : Class1
{
    public void Test()
    {
        foreach (var value in " + (hasStructEnumerator ? "this" : "[|this|]") + @") { }
    }
}
"
            };

            await test.RunAsync();
        }
    }
}
