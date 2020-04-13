// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Roslynator.FileSystem;

namespace Roslynator.CommandLine
{
    internal static class OptionValues
    {
        public static readonly SimpleOptionValue ConflictResolution_Ask = SimpleOptionValue.Create(ConflictResolution.Ask, description: "Ask when a file or already exists.");
        public static readonly SimpleOptionValue ConflictResolution_Overwrite = SimpleOptionValue.Create(ConflictResolution.Overwrite, description: "Overwrite a file when it already exists.");
        public static readonly SimpleOptionValue ConflictResolution_Rename = SimpleOptionValue.Create(ConflictResolution.Rename, description: "Create new file name if it already exists.");
        public static readonly SimpleOptionValue ConflictResolution_Skip = SimpleOptionValue.Create(ConflictResolution.Skip, description: "Do not copy or move a file if it already exists.");
        public static readonly SimpleOptionValue Display_Count = SimpleOptionValue.Create("Count", description: "Include number of matches in file.");
        public static readonly SimpleOptionValue Display_CreationTime = SimpleOptionValue.Create("CreationTime", shortValue: "ct", helpValue: "c[reation-]t[ime]", description: "Include file creation time.");
        public static readonly SimpleOptionValue Display_LineNumber = SimpleOptionValue.Create("LineNumber", description: "Include line number.");
        public static readonly SimpleOptionValue Display_ModifiedTime = SimpleOptionValue.Create("ModifiedTime", shortValue: "mt", helpValue: "m[odified-]t[ime]", description: "Include file last modified time.");
        public static readonly SimpleOptionValue Display_Size = SimpleOptionValue.Create("Size", description: "Include file size.");
        public static readonly SimpleOptionValue Display_Summary = SimpleOptionValue.Create("Summary", shortValue: "su", description: "Include summary.");
        public static readonly SimpleOptionValue Display_TrimLine = SimpleOptionValue.Create("TrimLine", shortValue: "", description: "Trim leading and trailing white-space from a line.");
        public static readonly SimpleOptionValue Output_Append = SimpleOptionValue.Create("Append", description: "If the file exists output will be appended to the end of the file.");

        public static readonly KeyValuePairOptionValue Display_Content = KeyValuePairOptionValue.Create("content", MetaValues.ContentDisplay, shortKey: "c");
        public static readonly KeyValuePairOptionValue Display_Indent = KeyValuePairOptionValue.Create("indent", "<INDENT>", shortKey: "", description: "Indentation for a list of results. Default indentation are 2 spaces.");
        public static readonly KeyValuePairOptionValue Display_Path = KeyValuePairOptionValue.Create("path", MetaValues.PathDisplay, shortKey: "p");
        public static readonly KeyValuePairOptionValue Display_Separator = KeyValuePairOptionValue.Create("separator", "<SEPARATOR>", description: "String that separate each value.");
        public static readonly KeyValuePairOptionValue Encoding = KeyValuePairOptionValue.Create("encoding", MetaValues.Encoding, shortKey: "e");
        public static readonly KeyValuePairOptionValue FileProperty_CreationTime = KeyValuePairOptionValue.Create("creation-time", "<DATE>", shortKey: "ct", helpValue: "c[reation-]t[ime]", description: "Filter files by creation time (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue FileProperty_ModifiedTime = KeyValuePairOptionValue.Create("modified-time", "<DATE>", shortKey: "mt", helpValue: "m[odified-]t[ime]", description: "Filter files by modified time (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue FileProperty_Size = KeyValuePairOptionValue.Create("size", "<NUM>", description: "Filter files by size (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue Group = KeyValuePairOptionValue.Create("group", "<GROUP_NAME>", shortKey: "g");
        public static readonly KeyValuePairOptionValue Length = KeyValuePairOptionValue.Create("length", "<NUM>", shortKey: "", description: "Include matches whose length matches the expression (See 'Expression syntax' for other expressions).");
        public static readonly KeyValuePairOptionValue ListSeparator = KeyValuePairOptionValue.Create("list-separator", "<SEPARATOR>", shortKey: "ls", helpValue: "l[ist-]s[eparator]", description: "String that separate each value in a list. Default value is comma (,) or newline if the list is loaded from a file.");
        public static readonly KeyValuePairOptionValue MaxCount = KeyValuePairOptionValue.Create("max-count", "<NUM>", description: "Show only <NUM> items.");
        public static readonly KeyValuePairOptionValue MaxMatches = KeyValuePairOptionValue.Create("matches", "<NUM>", description: "Stop searching after <NUM> matches.");
        public static readonly KeyValuePairOptionValue MaxMatchingFiles = KeyValuePairOptionValue.Create("matching-files", "<NUM>", shortKey: "mf", helpValue: "m[atching-]f[iles]", description: "Stop searching after <NUM> matching files.");
        public static readonly KeyValuePairOptionValue Part = KeyValuePairOptionValue.Create("part", MetaValues.NamePart, shortKey: "p", description: "The part of a file or a directory name that should be matched.");
        public static readonly KeyValuePairOptionValue SortBy = KeyValuePairOptionValue.Create("sort-by", MetaValues.SortProperty, shortKey: "", description: "");
        public static readonly KeyValuePairOptionValue Timeout = KeyValuePairOptionValue.Create("timeout", "<NUM>", shortKey: "", description: "Match time-out interval in seconds.");
        public static readonly KeyValuePairOptionValue Verbosity = KeyValuePairOptionValue.Create("verbosity", MetaValues.Verbosity, shortKey: "v");
    }
}
