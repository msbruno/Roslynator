// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CommandLine
{
    internal static class OptionValues
    {
        public static readonly SimpleOptionValue Output_Append = SimpleOptionValue.Create("Append", description: "If the file exists output will be appended to the end of the file.");

        public static readonly KeyValuePairOptionValue Encoding = KeyValuePairOptionValue.Create("encoding", MetaValues.Encoding, shortKey: "e");
        public static readonly KeyValuePairOptionValue Verbosity = KeyValuePairOptionValue.Create("verbosity", MetaValues.Verbosity, shortKey: "v");
    }
}
