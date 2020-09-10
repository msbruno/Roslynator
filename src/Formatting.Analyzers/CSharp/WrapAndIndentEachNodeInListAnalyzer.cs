﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WrapAndIndentEachNodeInListAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.WrapAndIndentEachNodeInList); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeTypeArgumentList, SyntaxKind.TypeArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeBracketedArgumentList, SyntaxKind.BracketedArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeAttributeList, SyntaxKind.AttributeList);
            context.RegisterSyntaxNodeAction(AnalyzeAttributeArgumentList, SyntaxKind.AttributeArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeBaseList, SyntaxKind.BaseList);
            context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
            context.RegisterSyntaxNodeAction(AnalyzeBracketedParameterList, SyntaxKind.BracketedParameterList);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
        }

        private static void AnalyzeTypeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var typeArgumentList = (TypeArgumentListSyntax)context.Node;

            Analyze(context, typeArgumentList.Arguments);
        }

        private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentList = (ArgumentListSyntax)context.Node;

            Analyze(context, argumentList.Arguments);
        }

        private static void AnalyzeBracketedArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentList = (BracketedArgumentListSyntax)context.Node;

            Analyze(context, argumentList.Arguments);
        }

        private static void AnalyzeAttributeList(SyntaxNodeAnalysisContext context)
        {
            var attributeList = (AttributeListSyntax)context.Node;

            Analyze(context, attributeList.Attributes);
        }

        private static void AnalyzeAttributeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var attributeArgumentList = (AttributeArgumentListSyntax)context.Node;

            Analyze(context, attributeArgumentList.Arguments);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;

            Analyze(context, baseList.Types);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;

            Analyze(context, parameterList.Parameters);
        }

        private static void AnalyzeBracketedParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (BracketedParameterListSyntax)context.Node;

            Analyze(context, parameterList.Parameters);
        }

        private static void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            var typeParameterList = (TypeParameterListSyntax)context.Node;

            Analyze(context, typeParameterList.Parameters);
        }

        private static void Analyze<TNode>(SyntaxNodeAnalysisContext context, SeparatedSyntaxList<TNode> nodes) where TNode : SyntaxNode
        {
            int count = nodes.Count;

            if (count <= 1)
                return;

            SyntaxTree syntaxTree = nodes[0].SyntaxTree;

            bool isSingleLine = true;

            for (int i = 1; i < count; i++)
            {
                TNode node1 = nodes[i - 1];
                TNode node2 = nodes[i];

                bool isSingleLine1 = IsSingleLine(node1.Span);
                bool isSingleLine2 = IsSingleLine(node2.Span);
                bool isSingleLineBetween = IsSingleLine(TextSpan.FromBounds(node1.Span.End, node2.SpanStart));

                if (isSingleLine1)
                {
                    if (isSingleLine2)
                    {
                        if (i > 1
                            && isSingleLineBetween != isSingleLine)
                        {
                            ReportDiagnostic();
                            return;
                        }
                    }
                    else if (isSingleLineBetween)
                    {
                        if (i < count - 1
                            || !(node2 is ArgumentSyntax argument)
                            || !argument.Expression.IsKind(SyntaxKind.AnonymousMethodExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression))
                        {
                            ReportDiagnostic();
                            return;
                        }
                    }
                    else if (i > 1
                        && isSingleLine)
                    {
                        ReportDiagnostic();
                        return;
                    }
                }
                else if (isSingleLine2)
                {
                    if (isSingleLineBetween)
                    {
                        ReportDiagnostic();
                        return;
                    }
                    else if (i > 1
                        && isSingleLine)
                    {
                        ReportDiagnostic();
                        return;
                    }
                }
                else if (isSingleLineBetween)
                {
                    ReportDiagnostic();
                    return;
                }
                else
                {
                    isSingleLine = false;
                }

                if (isSingleLine)
                {
                    isSingleLine = isSingleLine1 && isSingleLine2 && isSingleLineBetween;
                }
            }

            bool IsSingleLine(TextSpan span)
            {
                return syntaxTree.GetLineSpan(span, context.CancellationToken).IsSingleLine();
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.WrapAndIndentEachNodeInList,
                    Location.Create(syntaxTree, TextSpan.FromBounds(nodes[0].SpanStart, nodes.Last().Span.End)));
            }
        }
    }
}
