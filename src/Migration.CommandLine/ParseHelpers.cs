// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal static class ParseHelpers
    {
        public static bool TryParseOutputOptions(
            IEnumerable<string> values,
            string optionName,
            out string path,
            out Verbosity verbosity,
            out Encoding encoding,
            out bool append)
        {
            path = null;
            verbosity = Verbosity.Normal;
            encoding = Encoding.UTF8;
            append = false;

            if (!values.Any())
                return true;

            if (!TryEnsureFullPath(values.First(), out path))
                return false;

            foreach (string value in values.Skip(1))
            {
                string option = value;

                int index = option.IndexOf('=');

                if (index >= 0)
                {
                    string key = option.Substring(0, index);
                    string value2 = option.Substring(index + 1);

                    if (OptionValues.Verbosity.IsKeyOrShortKey(key))
                    {
                        if (!TryParseVerbosity(value2, out verbosity))
                            return false;
                    }
                    else if (OptionValues.Encoding.IsKeyOrShortKey(key))
                    {
                        if (!TryParseEncoding(value2, out encoding))
                            return false;
                    }
                    else
                    {
                        WriteParseError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                        return false;
                    }
                }
                else if (OptionValues.Output_Append.IsValueOrShortValue(value))
                {
                    append = true;
                }
                else
                {
                    WriteParseError(value, optionName, OptionValueProviders.OutputFlagsProvider);
                    return false;
                }
            }

            return true;
        }

        public static bool TryParseAsEnumFlags<TEnum>(
            IEnumerable<string> values,
            string optionName,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = null) where TEnum : struct
        {
            result = (TEnum)(object)0;

            if (values?.Any() != true)
            {
                if (defaultValue != null)
                {
                    result = (TEnum)(object)defaultValue;
                }

                return true;
            }

            int flags = 0;

            foreach (string value in values)
            {
                if (!TryParseAsEnum(value, optionName, out TEnum result2, provider: provider))
                    return false;

                flags |= (int)(object)result2;
            }

            result = (TEnum)(object)flags;

            return true;
        }

        public static bool TryParseAsEnumValues<TEnum>(
            IEnumerable<string> values,
            string optionName,
            out ImmutableArray<TEnum> result,
            ImmutableArray<TEnum> defaultValue = default,
            OptionValueProvider provider = null) where TEnum : struct
        {
            if (values?.Any() != true)
            {
                result = (defaultValue.IsDefault) ? ImmutableArray<TEnum>.Empty : defaultValue;

                return true;
            }

            ImmutableArray<TEnum>.Builder builder = ImmutableArray.CreateBuilder<TEnum>();

            foreach (string value in values)
            {
                if (!TryParseAsEnum(value, optionName, out TEnum result2, provider: provider))
                    return false;

                builder.Add(result2);
            }

            result = builder.ToImmutableArray();

            return true;
        }

        public static bool TryParseAsEnum<TEnum>(
            string value,
            string optionName,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = null) where TEnum : struct
        {
            if (!TryParseAsEnum(value, out result, defaultValue, provider))
            {
                WriteParseError(value, optionName, provider?.GetHelpText() ?? OptionValue.GetDefaultHelpText<TEnum>());
                return false;
            }

            return true;
        }

        public static bool TryParseAsEnum<TEnum>(
            string value,
            out TEnum result,
            TEnum? defaultValue = null,
            OptionValueProvider provider = default) where TEnum : struct
        {
            if (value == null
                && defaultValue != null)
            {
                result = defaultValue.Value;
                return true;
            }

            if (provider != null)
            {
                return provider.TryParseEnum(value, out result);
            }
            else
            {
                return Enum.TryParse(value?.Replace("-", ""), ignoreCase: true, out result);
            }
        }

        public static bool TryParseVerbosity(string value, out Verbosity verbosity)
        {
            return TryParseAsEnum(value, OptionNames.Verbosity, out verbosity, provider: OptionValueProviders.VerbosityProvider);
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding#remarks
        public static bool TryParseEncoding(string name, out Encoding encoding)
        {
            if (name == "utf-8-no-bom")
            {
                encoding = EncodingHelpers.UTF8NoBom;
                return true;
            }

            try
            {
                encoding = Encoding.GetEncoding(name);
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteError(ex);

                encoding = null;
                return false;
            }
        }

        public static bool TryParseVersion(string value, out Version version)
        {
            if (!Version.TryParse(value, out version))
            {
                WriteError($"Could not parse '{value}' as version.");
                return false;
            }

            return true;
        }

        public static bool TryEnsureFullPath(IEnumerable<string> paths, out ImmutableArray<string> fullPaths)
        {
            ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();

            foreach (string path in paths)
            {
                if (!TryEnsureFullPath(path, out string fullPath))
                    return false;

                builder.Add(fullPath);
            }

            fullPaths = builder.ToImmutableArray();
            return true;
        }

        public static bool TryEnsureFullPath(string path, out string result)
        {
            try
            {
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(path);

                result = path;
                return true;
            }
            catch (ArgumentException ex)
            {
                WriteError($"Path '{path}' is invalid: {ex.Message}.");
                result = null;
                return false;
            }
        }

        private static void WriteParseError(string value, string optionName, OptionValueProvider provider)
        {
            string helpText = provider.GetHelpText();

            WriteParseError(value, optionName, helpText);
        }

        private static void WriteParseError(string value, string optionName, string helpText)
        {
            WriteError($"Option '{OptionNames.GetHelpText(optionName)}' has invalid value '{value}'. Allowed values: {helpText}.");
        }
    }
}
