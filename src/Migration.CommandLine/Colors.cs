// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator
{
    internal static class Colors
    {
        public static ConsoleColors Message_OK { get; } = new ConsoleColors(ConsoleColor.Green);
        public static ConsoleColors Message_DryRun { get; } = new ConsoleColors(ConsoleColor.DarkGray);
        public static ConsoleColors Message_Warning { get; } = new ConsoleColors(ConsoleColor.Yellow);
    }
}
