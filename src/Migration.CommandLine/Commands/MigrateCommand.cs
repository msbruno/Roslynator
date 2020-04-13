// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Roslynator.CommandLine
{
    internal class MigrateCommand : AbstractCommand<MigrateCommandOptions>
    {
        public MigrateCommand(MigrateCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            return CommandResult.Success;
        }
    }
}
