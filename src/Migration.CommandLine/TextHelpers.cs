// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Roslynator
{
    internal static class TextHelpers
    {
        internal static string Join(string separator, string lastSeparator, IEnumerable<string> values)
        {
            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    var sb = new StringBuilder();

                    sb.Append(en.Current);

                    if (en.MoveNext())
                    {
                        string previous = en.Current;

                        while (true)
                        {
                            if (en.MoveNext())
                            {
                                sb.Append(separator);
                                sb.Append(previous);
                                previous = en.Current;
                            }
                            else
                            {
                                sb.Append(lastSeparator);
                                sb.Append(previous);
                                break;
                            }
                        }
                    }

                    return sb.ToString();
                }

                return "";
            }
        }
    }
}
