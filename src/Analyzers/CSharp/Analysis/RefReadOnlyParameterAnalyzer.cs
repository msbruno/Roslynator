﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RefReadOnlyParameterAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.MakeParameterRefReadOnly,
                    DiagnosticDescriptors.DoNotPassNonReadOnlyStructByReadOnlyReference);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion <= LanguageVersion.CSharp7_1)
                    return;

                //TODO: AnalyzeIndexerDeclaration
                startContext.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeOperatorDeclaration, SyntaxKind.OperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
            });
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.Modifiers.ContainsAny(SyntaxKind.AsyncKeyword, SyntaxKind.OverrideKeyword))
                return;

            Analyze(context, methodDeclaration, methodDeclaration.ParameterList, methodDeclaration.BodyOrExpressionBody());
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            Analyze(context, constructorDeclaration, constructorDeclaration.ParameterList, constructorDeclaration.BodyOrExpressionBody());
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            Analyze(context, operatorDeclaration, operatorDeclaration.ParameterList, operatorDeclaration.BodyOrExpressionBody());
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            Analyze(context, operatorDeclaration, operatorDeclaration.ParameterList, operatorDeclaration.BodyOrExpressionBody());
        }

        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;

            if (localFunction.Modifiers.Contains(SyntaxKind.AsyncKeyword))
                return;

            Analyze(context, localFunction, localFunction.ParameterList, localFunction.BodyOrExpressionBody());
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            SyntaxNode declaration,
            ParameterListSyntax parameterList,
            CSharpSyntaxNode bodyOrExpressionBody)
        {
            if (parameterList == null)
                return;

            if (bodyOrExpressionBody == null)
                return;

            if (!parameterList.Parameters.Any())
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

            SyntaxWalker walker = null;

            foreach (IParameterSymbol parameter in methodSymbol.Parameters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ITypeSymbol type = parameter.Type;

                if (type.Kind == SymbolKind.ErrorType)
                    continue;

                if (CSharpFacts.IsSimpleType(type.SpecialType))
                    continue;

                if (!type.IsReadOnlyStruct())
                {
                    if (parameter.RefKind == RefKind.In
                        && type.TypeKind == TypeKind.Struct)
                    {
                        var parameterSyntax = (ParameterSyntax)parameter.GetSyntax(cancellationToken);

                        Debug.Assert(parameterSyntax.Modifiers.Contains(SyntaxKind.InKeyword), "");

                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.DoNotPassNonReadOnlyStructByReadOnlyReference, parameterSyntax.Identifier);
                    }

                    continue;
                }

                if (parameter.RefKind != RefKind.None)
                    continue;

                if (walker == null)
                {
                    if (methodSymbol.ImplementsInterfaceMember(allInterfaces: true))
                        break;

                    walker = SyntaxWalker.GetInstance();
                }
                else if (walker.Parameters.ContainsKey(parameter.Name))
                {
                    walker.Parameters.Clear();
                    break;
                }

                walker.Parameters.Add(parameter.Name, parameter);
            }

            if (walker == null)
                return;

            if (walker.Parameters.Count > 0)
            {
                walker.SemanticModel = semanticModel;
                walker.CancellationToken = cancellationToken;

                if (bodyOrExpressionBody is BlockSyntax body)
                {
                    walker.VisitBlock(body);
                }
                else
                {
                    walker.VisitArrowExpressionClause((ArrowExpressionClauseSyntax)bodyOrExpressionBody);
                }

                if (walker.Parameters.Count > 0
                    && !IsReferencedAsMethodGroup())
                {
                    foreach (KeyValuePair<string, IParameterSymbol> kvp in walker.Parameters)
                    {
                        if (kvp.Value.GetSyntaxOrDefault(cancellationToken) is ParameterSyntax parameter)
                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.MakeParameterRefReadOnly, parameter.Identifier);
                    }
                }
            }

            SyntaxWalker.Free(walker);

            bool IsReferencedAsMethodGroup()
            {
                switch (declaration.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                        return MethodReferencedAsMethodGroupWalker.IsReferencedAsMethodGroup(declaration.Parent, methodSymbol, semanticModel, cancellationToken);
                    case SyntaxKind.LocalFunctionStatement:
                        return MethodReferencedAsMethodGroupWalker.IsReferencedAsMethodGroup(declaration.FirstAncestor<MemberDeclarationSyntax>(), methodSymbol, semanticModel, cancellationToken);
                    default:
                        return false;
                }
            }
        }

        private class SyntaxWalker : AssignedExpressionWalker
        {
            [ThreadStatic]
            private static SyntaxWalker _cachedInstance;

            private bool _isInAssignedExpression;
            private int _localFunctionDepth;
            private int _anonymousFunctionDepth;

            public Dictionary<string, IParameterSymbol> Parameters { get; } = new Dictionary<string, IParameterSymbol>();

            public SemanticModel SemanticModel { get; set; }

            public CancellationToken CancellationToken { get; set; }

            public void Reset()
            {
                Parameters.Clear();
                SemanticModel = null;
                CancellationToken = default;
                _isInAssignedExpression = false;
                _localFunctionDepth = 0;
                _anonymousFunctionDepth = 0;
            }

            protected override bool ShouldVisit
            {
                get { return Parameters.Count > 0; }
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                CancellationToken.ThrowIfCancellationRequested();

                string name = node.Identifier.ValueText;

                if (Parameters.TryGetValue(name, out IParameterSymbol parameterSymbol)
                    && SymbolEqualityComparer.Default.Equals(parameterSymbol, SemanticModel.GetSymbol(node, CancellationToken)))
                {
                    if (_isInAssignedExpression
                        || _localFunctionDepth > 0
                        || _anonymousFunctionDepth > 0)
                    {
                        Parameters.Remove(name);
                    }
                }

                base.VisitIdentifierName(node);
            }

            public override void VisitAssignedExpression(ExpressionSyntax expression)
            {
                Debug.Assert(!_isInAssignedExpression);

                _isInAssignedExpression = true;
                Visit(expression);
                _isInAssignedExpression = false;
            }

            public override void VisitYieldStatement(YieldStatementSyntax node)
            {
                if (_localFunctionDepth == 0)
                {
                    Parameters.Clear();
                }
                else
                {
                    base.VisitYieldStatement(node);
                }
            }

            public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
            {
                _anonymousFunctionDepth++;
                base.VisitAnonymousMethodExpression(node);
                _anonymousFunctionDepth--;
            }

            public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                _anonymousFunctionDepth++;
                base.VisitSimpleLambdaExpression(node);
                _anonymousFunctionDepth--;
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
                _anonymousFunctionDepth++;
                base.VisitParenthesizedLambdaExpression(node);
                _anonymousFunctionDepth--;
            }

            public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
            {
                _localFunctionDepth++;
                base.VisitLocalFunctionStatement(node);
                _localFunctionDepth--;
            }

            public static SyntaxWalker GetInstance()
            {
                SyntaxWalker walker = _cachedInstance;

                if (walker != null)
                {
                    Debug.Assert(walker.Parameters.Count == 0);
                    Debug.Assert(walker.SemanticModel == null);
                    Debug.Assert(walker.CancellationToken == default);

                    _cachedInstance = null;
                    return walker;
                }

                return new SyntaxWalker();
            }

            public static void Free(SyntaxWalker walker)
            {
                walker.Reset();

                _cachedInstance = walker;
            }
        }
    }
}
