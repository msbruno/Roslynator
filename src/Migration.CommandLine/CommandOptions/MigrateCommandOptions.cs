// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;

namespace Roslynator.CommandLine
{
    internal class MigrateCommandOptions : AbstractCommandOptions
    {
        internal MigrateCommandOptions()
        {
        }

        public bool DryRun { get; internal set; }

        public string PackageName { get; internal set; }

        public ImmutableArray<string> Paths { get; internal set; }

        public Version Version { get; internal set; }
    }
}
