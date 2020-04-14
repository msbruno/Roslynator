// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Roslynator.CommandLine
{
    internal static class OptionNames
    {
        public const string DryRun = "dry-run";
        public const string Help = "help";
        public const string Manual = "manual";
        public const string Output = "output";
        public const string Package = "package";
        public const string TargetVersion = "target-version";
        public const string Values = "values";
        public const string Verbosity = "verbosity";

        private static ImmutableDictionary<string, string> _namesToShortNames;

        public static ImmutableDictionary<string, string> NamesToShortNames
        {
            get
            {
                if (_namesToShortNames == null)
                    Interlocked.CompareExchange(ref _namesToShortNames, Create(), null);

                return _namesToShortNames;

                static ImmutableDictionary<string, string> Create()
                {
                    FieldInfo[] names = typeof(OptionNames).GetFields();

                    return names.Join(
                        typeof(OptionShortNames).GetFields(),
                        f => f.Name,
                        f => f.Name,
                        (f, g) => new KeyValuePair<string, string>((string)f.GetValue(null), g.GetValue(null).ToString())).ToImmutableDictionary();
                }
            }
        }

        public static string GetHelpText(string name)
        {
            if (NamesToShortNames.TryGetValue(name, out string shortName))
            {
                return $"-{shortName}, --{name}";
            }
            else
            {
                return $"--{name}";
            }
        }
    }
}
