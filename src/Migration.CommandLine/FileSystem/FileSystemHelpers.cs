// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Roslynator.CommandLine;

namespace Roslynator.FileSystem
{
    internal static class FileSystemHelpers
    {
        public static IEnumerable<string> EnumerateFiles(string path, EnumerationOptions options)
        {
            return Directory.EnumerateFiles(path, "*", options);
        }

        public static IEnumerable<string> EnumerateDirectories(string path, EnumerationOptions options)
        {
            return Directory.EnumerateDirectories(path, "*", options);
        }

        public static string[] GetFiles(string path, EnumerationOptions options)
        {
            return Directory.GetFiles(path, "*", options);
        }

        public static string[] GetDirectories(string path, EnumerationOptions options)
        {
            return Directory.GetDirectories(path, "*", options);
        }

        public static bool IsEmptyFile(string path)
        {
            var fileInfo = new FileInfo(path);

            if (fileInfo.Length == 0)
                return true;

            if (fileInfo.Length <= 4)
            {
                using (FileStream stream = fileInfo.OpenRead())
                {
                    Encoding encoding = EncodingHelpers.DetectEncoding(stream);

                    return encoding?.Preamble.Length == stream.Length;
                }
            }

            return false;
        }

        public static bool IsEmptyDirectory(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static int GetFileNameIndex(string path)
        {
            int rootLength = Path.GetPathRoot(path).Length;

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (i < rootLength
                    || IsDirectorySeparator(path[i]))
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static int GetExtensionIndex(string path)
        {
            int length = path.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                char ch = path[i];

                if (ch == '.')
                    return i;

                if (IsDirectorySeparator(ch))
                    break;
            }

            return path.Length;
        }

        public static bool IsDirectorySeparator(char ch)
        {
            return ch == Path.DirectorySeparatorChar
                || ch == Path.AltDirectorySeparatorChar;
        }
    }
}
