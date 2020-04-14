// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandLine;
using static Roslynator.CommandLine.ParseHelpers;

namespace Roslynator.CommandLine
{
    [Verb("migrate", HelpText = "Migrates Roslynator analyzers to a new version.")]
    internal sealed class MigrateCommandLineOptions : AbstractCommandLineOptions
    {
        [Value(index: 0,
            HelpText = "A path to a directory, project file or a ruleset file.",
            MetaName = ArgumentMetaNames.Path)]
        public IEnumerable<string> Path { get; set; }

        [Option(longName: OptionNames.DryRun,
            HelpText = "Execute migration but do not save changes to a disk.")]
        public bool DryRun { get; set; }

        [Option(longName: OptionNames.Package,
            HelpText = "A package to be migrated.",
            MetaValue = MetaValues.PackageName)]
        public string Package { get; set; }

        [Option(longName: OptionNames.TargetVersion,
            Required = true,
            HelpText = "A package version to migrate to.",
            MetaValue = MetaValues.Version)]
        public string Version { get; set; }

        public bool TryParse(MigrateCommandOptions options)
        {
            if (!TryParsePaths(out ImmutableArray<string> paths))
                return false;

            if (!TryParseVersion(Version, out Version version))
                return false;

            options.DryRun = DryRun;
            options.PackageName = Package;
            options.Paths = paths;
            options.Version = version;

            return true;
        }

        private bool TryParsePaths(out ImmutableArray<string> paths)
        {
            paths = ImmutableArray<string>.Empty;

            if (Path.Any()
                && !TryEnsureFullPath(Path, out paths))
            {
                return false;
            }

            if (Console.IsInputRedirected)
            {
                ImmutableArray<string> pathsFromInput = ConsoleHelpers.ReadRedirectedInputAsLines()
                   .Where(f => !string.IsNullOrEmpty(f))
                   .ToImmutableArray();

                paths = paths.AddRange(pathsFromInput);
            }

            if (paths.IsEmpty)
                paths = ImmutableArray.Create(Environment.CurrentDirectory);

            return true;
        }
    }
}
