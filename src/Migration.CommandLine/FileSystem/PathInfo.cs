// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.FileSystem
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct PathInfo
    {
        public PathInfo(string path, PathKind kind)
        {
            Path = path;
            Kind = kind;
        }

        public string Path { get; }

        public PathKind Kind { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Kind}  {Path}";
    }
}
