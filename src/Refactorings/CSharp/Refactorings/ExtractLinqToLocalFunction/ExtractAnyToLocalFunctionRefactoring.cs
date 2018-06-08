﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.ExtractLinqToLocalFunction
{
    internal class ExtractAnyToLocalFunctionRefactoring : ExtractLinqToLocalFunctionRefactoring
    {
        public override string MethodName
        {
            get { return "Any"; }
        }

        protected override ReturnStatementSyntax GetFirstReturnStatement()
        {
            return ReturnStatement(TrueLiteralExpression());
        }

        protected override ReturnStatementSyntax GetLastReturnStatement()
        {
            return ReturnStatement(FalseLiteralExpression());
        }
    }
}
