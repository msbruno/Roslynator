// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Roslynator.FileSystem;

namespace Roslynator
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Filter
    {
        public Filter(
            Regex regex,
            NamePartKind namePart = NamePartKind.Name,
            int groupNumber = -1,
            bool isNegative = false,
            Func<Capture, bool> predicate = null)
        {
            Regex = regex ?? throw new ArgumentNullException(nameof(regex));

            Debug.Assert(groupNumber < 0 || regex.GetGroupNumbers().Contains(groupNumber), groupNumber.ToString());

            GroupNumber = groupNumber;
            IsNegative = isNegative;
            NamePart = namePart;
            Predicate = predicate;
        }

        public Regex Regex { get; }

        public bool IsNegative { get; }

        public int GroupNumber { get; }

        public NamePartKind NamePart { get; }

        public Func<Capture, bool> Predicate { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Negative = {IsNegative}  Part = {NamePart}  Group {GroupNumber}  {Regex}";

        internal bool IsMatch(in NamePart part)
        {
            return IsMatch(Regex.Match(part.Path, part.Index, part.Length));
        }

        internal bool IsMatch(Match match)
        {
            return (IsNegative) ? !IsMatchImpl(match) : IsMatchImpl(match);
        }

        private bool IsMatchImpl(Match match)
        {
            return IsMatchImpl((GroupNumber < 1) ? match : match.Groups[GroupNumber]);
        }

        private bool IsMatchImpl(Group group)
        {
            return group.Success && Predicate?.Invoke(group) != false;
        }
    }
}
