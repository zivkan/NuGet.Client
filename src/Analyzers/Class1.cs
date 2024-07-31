// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypesShouldBeUpperCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MY0001";

        private const string Title = "Type name contains lowercase letters";
        private const string MessageFormat = "Type name '{0}' contains lowercase letters";
        private const string Description = "Type names should be all uppercase.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ForEachStatement);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var statement = (ForEachStatementSyntax)context.Node;

            if (context.SemanticModel.GetDiagnostics(statement.Expression.GetLocation().SourceSpan).Any())
            {
                return;
            }

            var typeInfo = context.SemanticModel.GetTypeInfo(statement.Expression);
            if (typeInfo.Type is INamedTypeSymbol namedTypeSymbol)
            {
                var members = namedTypeSymbol.GetMembers();

                ISymbol? enumeratorType = namedTypeSymbol.GetMembers("GetEnumerator").FirstOrDefault();

                if (enumeratorType is IMethodSymbol method)
                {
                    var returnType = method.ReturnType;
                    if (returnType.IsValueType)
                    {
                        return;
                    }

                    var diagnostic = Diagnostic.Create(Rule, statement.Expression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
                else if (namedTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    var diagnostic = Diagnostic.Create(Rule, statement.Expression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
