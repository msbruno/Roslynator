// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Roslynator.CommandLine
{
    internal static class OptionValueProviders
    {
        private static ImmutableDictionary<string, OptionValueProvider> _providersByName;

        public static OptionValueProvider VerbosityProvider { get; } = new OptionValueProvider(MetaValues.Verbosity,
            SimpleOptionValue.Create(Verbosity.Quiet),
            SimpleOptionValue.Create(Verbosity.Minimal),
            SimpleOptionValue.Create(Verbosity.Normal),
            SimpleOptionValue.Create(Verbosity.Detailed),
            SimpleOptionValue.Create(Verbosity.Diagnostic, shortValue: "di")
        );

        public static OptionValueProvider OutputFlagsProvider { get; } = new OptionValueProvider(MetaValues.OutputOptions,
            OptionValues.Encoding,
            OptionValues.Verbosity,
            OptionValues.Output_Append
        );

        public static ImmutableDictionary<string, OptionValueProvider> ProvidersByName
        {
            get
            {
                if (_providersByName == null)
                    Interlocked.CompareExchange(ref _providersByName, LoadProviders(), null);

                return _providersByName;

                static ImmutableDictionary<string, OptionValueProvider> LoadProviders()
                {
                    PropertyInfo[] fieldInfo = typeof(OptionValueProviders).GetProperties();

                    return fieldInfo
                        .Where(f => f.PropertyType.Equals(typeof(OptionValueProvider)))
                        .OrderBy(f => f.Name)
                        .Select(f => (OptionValueProvider)f.GetValue(null))
                        .ToImmutableDictionary(f => f.Name, f => f);
                }
            }
        }
    }
}
