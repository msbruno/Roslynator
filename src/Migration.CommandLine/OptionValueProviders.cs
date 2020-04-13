// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Roslynator.FileSystem;

namespace Roslynator.CommandLine
{
    internal static class OptionValueProviders
    {
        private static ImmutableDictionary<string, OptionValueProvider> _providersByName;

        public static OptionValueProvider RegexOptionsProvider { get; } = new OptionValueProvider(MetaValues.RegexOptions,
            SimpleOptionValue.Create(RegexOptions.Compiled, description: "Compile the regular expression to an assembly."),
            SimpleOptionValue.Create(RegexOptions.CultureInvariant, shortValue: "ci", helpValue: "c[ulture]-i[nvariant]", description: "Ignore cultural differences between languages."),
            SimpleOptionValue.Create(RegexOptions.ECMAScript, value: "ecma-script", shortValue: "es", helpValue: "e[cma]-s[cript]", description: "Enable ECMAScript-compliant behavior for the expression."),
            SimpleOptionValue.Create(RegexOptions.ExplicitCapture, shortValue: "n", description: "Do not capture unnamed groups."),
            SimpleOptionValue.Create(RegexOptions.IgnoreCase, description: "Use case-insensitive matching."),
            SimpleOptionValue.Create(RegexOptions.IgnorePatternWhitespace, shortValue: "x", description: "Exclude unescaped white-space from the pattern and enable comments after a number sign (#)."),
            SimpleOptionValue.Create(RegexOptions.Multiline, description: "^ and $ match the beginning and end of each line (instead of the beginning and end of the input string)."),
            SimpleOptionValue.Create(RegexOptions.RightToLeft, description: "Specifies that the search will be from right to left."),
            SimpleOptionValue.Create(RegexOptions.Singleline, description: "The period (.) matches every character (instead of every character except \\n).")
        );

        public static OptionValueProvider VerbosityProvider { get; } = new OptionValueProvider(MetaValues.Verbosity,
            SimpleOptionValue.Create(Verbosity.Quiet),
            SimpleOptionValue.Create(Verbosity.Minimal),
            SimpleOptionValue.Create(Verbosity.Normal),
            SimpleOptionValue.Create(Verbosity.Detailed),
            SimpleOptionValue.Create(Verbosity.Diagnostic, shortValue: "di")
        );

        public static OptionValueProvider FileSystemAttributesProvider { get; } = new OptionValueProvider(MetaValues.Attributes,
            SimpleOptionValue.Create(FileSystemAttributes.Archive, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.Compressed, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.Directory),
            SimpleOptionValue.Create(FileSystemAttributes.Empty),
            SimpleOptionValue.Create(FileSystemAttributes.Encrypted, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.File),
            SimpleOptionValue.Create(FileSystemAttributes.Hidden),
            SimpleOptionValue.Create(FileSystemAttributes.IntegrityStream, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.Normal, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.NoScrubData, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.NotContentIndexed, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.Offline, shortValue: ""),
            SimpleOptionValue.Create(FileSystemAttributes.ReadOnly),
            SimpleOptionValue.Create(FileSystemAttributes.ReparsePoint, shortValue: "rp", helpValue: "r[eparse]-p[oint]"),
            SimpleOptionValue.Create(FileSystemAttributes.SparseFile, shortValue: "", hidden: true),
            SimpleOptionValue.Create(FileSystemAttributes.System),
            SimpleOptionValue.Create(FileSystemAttributes.Temporary, shortValue: "")
        );

        public static OptionValueProvider FileSystemAttributesToSkipProvider { get; } = new OptionValueProvider(OptionValueProviderNames.FileSystemAttributesToSkip,
            FileSystemAttributesProvider.Values.Where(f => f.Name != nameof(FileSystemAttributes.File) && f.Name != nameof(FileSystemAttributes.Directory))
        );

        public static OptionValueProvider NamePartKindProvider { get; } = new OptionValueProvider(MetaValues.NamePart,
            SimpleOptionValue.Create(NamePartKind.Extension, description: "Search in file extension."),
            SimpleOptionValue.Create(NamePartKind.FullName, description: "Search in full path."),
            SimpleOptionValue.Create(NamePartKind.Name, description: "Search in file name and its extension."),
            SimpleOptionValue.Create(NamePartKind.NameWithoutExtension, shortValue: "w", description: "Search in file name without extension.")
        );

        public static OptionValueProvider HighlightOptionsProvider { get; } = new OptionValueProvider(MetaValues.Highlight,
            SimpleOptionValue.Create(HighlightOptions.None, description: "No highlighting."),
            SimpleOptionValue.Create(HighlightOptions.Match, description: "Highlight match value."),
            SimpleOptionValue.Create(HighlightOptions.Replacement, description: "Highlight replacement value."),
            SimpleOptionValue.Create(HighlightOptions.Split, description: "Highlight split value."),
            SimpleOptionValue.Create(HighlightOptions.EmptyMatch, shortValue: "em", helpValue: "e[mpty-]m[atch]", description: "Highlight match value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.EmptyReplacement, shortValue: "er", helpValue: "e[mpty-]r[eplacement]", description: "Highlight replacement value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.EmptySplit, shortValue: "es", helpValue: "e[mpty-]s[plit]", description: "Highlight split value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.Empty, description: "Highlight value that is empty string."),
            SimpleOptionValue.Create(HighlightOptions.Boundary, description: "Highlight start and end of the value."),
            SimpleOptionValue.Create(HighlightOptions.Tab, description: "Highlight tab character."),
            SimpleOptionValue.Create(HighlightOptions.CarriageReturn, shortValue: "cr", helpValue: "c[arriage-]r[eturn]", description: "Highlight carriage return character."),
            SimpleOptionValue.Create(HighlightOptions.Linefeed, shortValue: "lf", helpValue: "l[ine]f[eed]", description: "Highlight linefeed character."),
            SimpleOptionValue.Create(HighlightOptions.NewLine, shortValue: "nl", helpValue: "n[ew-]l[ine]", description: "Highlight carriage return and linefeed characters."),
            SimpleOptionValue.Create(HighlightOptions.Space, shortValue: "", description: "Highlight space character.")
        );

        public static OptionValueProvider DeleteHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.DeleteHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                    case nameof(HighlightOptions.Boundary):
                    case nameof(HighlightOptions.Tab):
                    case nameof(HighlightOptions.Linefeed):
                    case nameof(HighlightOptions.CarriageReturn):
                    case nameof(HighlightOptions.NewLine):
                    case nameof(HighlightOptions.Space):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider FindHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.FindHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider MatchHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.MatchHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider RenameHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.RenameHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.EmptyReplacement):
                    case nameof(HighlightOptions.Split):
                    case nameof(HighlightOptions.Boundary):
                    case nameof(HighlightOptions.Tab):
                    case nameof(HighlightOptions.Linefeed):
                    case nameof(HighlightOptions.CarriageReturn):
                    case nameof(HighlightOptions.NewLine):
                    case nameof(HighlightOptions.Space):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider ReplaceHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.ReplaceHighlightOptions,
            HighlightOptionsProvider.Values.Where(f => f.Name != nameof(HighlightOptions.Split))
        );

        public static OptionValueProvider SplitHighlightOptionsProvider { get; } = new OptionValueProvider(OptionValueProviderNames.SplitHighlightOptions,
            HighlightOptionsProvider.Values.Where(f =>
            {
                switch (f.Name)
                {
                    case nameof(HighlightOptions.Match):
                    case nameof(HighlightOptions.Replacement):
                    case nameof(HighlightOptions.EmptyMatch):
                    case nameof(HighlightOptions.EmptyReplacement):
                        return false;
                }

                return true;
            })
        );

        public static OptionValueProvider OutputFlagsProvider { get; } = new OptionValueProvider(MetaValues.OutputOptions,
            OptionValues.Encoding,
            OptionValues.Verbosity,
            OptionValues.Output_Append
        );

        public static OptionValueProvider AskModeProvider { get; } = new OptionValueProvider(MetaValues.AskMode,
            SimpleOptionValue.Create(AskMode.File, description: "Ask for confirmation after each file."),
            SimpleOptionValue.Create(AskMode.Value, description: "Ask for confirmation after each value.")
        );

        public static OptionValueProvider MaxOptionsProvider { get; } = new OptionValueProvider(MetaValues.MaxOptions,
            SimpleOptionValue.Create("MatchesInFile", "<NUM>", shortValue: "", description: "Stop searching after <NUM> matches (or matches in file when searching in file's content)."),
            OptionValues.MaxMatches,
            OptionValues.MaxMatchingFiles
        );

        public static OptionValueProvider DisplayProvider { get; } = new OptionValueProvider(MetaValues.DisplayOptions,
            OptionValues.Display_Content,
            OptionValues.Display_Count,
            OptionValues.Display_CreationTime,
            OptionValues.Display_Indent,
            OptionValues.Display_LineNumber,
            OptionValues.Display_ModifiedTime,
            OptionValues.Display_Path,
            OptionValues.Display_Size,
            OptionValues.Display_Separator,
            OptionValues.Display_Summary,
            OptionValues.Display_TrimLine
        );

        public static OptionValueProvider DisplayProvider_MatchAndSplit { get; } = new OptionValueProvider(OptionValueProviderNames.Display_MatchAndSplit,
            OptionValues.Display_Content,
            OptionValues.Display_Indent,
            OptionValues.Display_Separator,
            OptionValues.Display_Summary
        );

        public static OptionValueProvider DisplayProvider_NonContent { get; } = new OptionValueProvider(OptionValueProviderNames.Display_NonContent,
            OptionValues.Display_CreationTime,
            OptionValues.Display_Indent,
            OptionValues.Display_ModifiedTime,
            OptionValues.Display_Path,
            OptionValues.Display_Size,
            OptionValues.Display_Separator,
            OptionValues.Display_Summary
        );

        public static OptionValueProvider SortFlagsProvider { get; } = new OptionValueProvider(MetaValues.SortOptions,
            SimpleOptionValue.Create(SortFlags.Ascending, description: "Sort items in ascending order."),
            SimpleOptionValue.Create(SortFlags.CreationTime, shortValue: "ct", helpValue: "c[reation-]t[ime]", description: "Sort items by creation time."),
            SimpleOptionValue.Create(SortFlags.Descending, description: "Sort items in descending order."),
            OptionValues.MaxCount,
            SimpleOptionValue.Create(SortFlags.ModifiedTime, shortValue: "mt", helpValue: "m[odified-]t[ime]", description: "Sort items by last modified time."),
            SimpleOptionValue.Create(SortFlags.Name, description: "Sort items by full name."),
            SimpleOptionValue.Create(SortFlags.Size, description: "Sort items by size.")
        );

        public static OptionValueProvider FilePropertiesProvider { get; } = new OptionValueProvider(MetaValues.FileProperties,
            OptionValues.FileProperty_CreationTime,
            OptionValues.FileProperty_ModifiedTime,
            OptionValues.FileProperty_Size
        );

        public static OptionValueProvider ConflictResolutionProvider { get; } = new OptionValueProvider(MetaValues.ConflictResolution,
            OptionValues.ConflictResolution_Ask,
            OptionValues.ConflictResolution_Overwrite,
            OptionValues.ConflictResolution_Rename,
            OptionValues.ConflictResolution_Skip
        );

        public static OptionValueProvider FileCompareOptionsProvider { get; } = new OptionValueProvider(MetaValues.CompareOptions,
            SimpleOptionValue.Create(FileCompareOptions.None, description: "Compare files only by name."),
            SimpleOptionValue.Create(FileCompareOptions.Attributes, description: "Compare file attributes."),
            SimpleOptionValue.Create(FileCompareOptions.Content, description: "Compare file content."),
            SimpleOptionValue.Create(FileCompareOptions.ModifiedTime, shortValue: "mt", helpValue: "m[odified-]t[ime]", description: "Compare time a file was last modified."),
            SimpleOptionValue.Create(FileCompareOptions.Size, description: "Compare file size.")
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
