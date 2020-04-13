// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine;
using CommandLine.Text;
using static Roslynator.CommandLine.ParseHelpers;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args != null)
            {
                if (args.Length == 1)
                {
                    if (IsHelpOption(args[0]))
                    {
                        Console.Write(HelpProvider.GetHelpText());
                        return 0;
                    }
                }
                else if (args.Length == 2)
                {
                    if (args?.Length == 2
                        && IsHelpOption(args[1]))
                    {
                        Command command = CommandLoader.LoadCommand(typeof(HelpCommand).Assembly, args[0]);

                        if (command != null)
                        {
                            Console.Write(HelpProvider.GetHelpText(command));
                            return 0;
                        }
                    }
                }
            }

            try
            {
                ParserSettings defaultSettings = Parser.Default.Settings;

                var parser = new Parser(settings =>
                {
                    settings.AutoHelp = false;
                    settings.AutoVersion = defaultSettings.AutoVersion;
                    settings.CaseInsensitiveEnumValues = defaultSettings.CaseInsensitiveEnumValues;
                    settings.CaseSensitive = defaultSettings.CaseSensitive;
                    settings.EnableDashDash = true;
                    settings.HelpWriter = null;
                    settings.IgnoreUnknownArguments = defaultSettings.IgnoreUnknownArguments;
                    settings.MaximumDisplayWidth = defaultSettings.MaximumDisplayWidth;
                    settings.ParsingCulture = defaultSettings.ParsingCulture;
                });

                ParserResult<object> parserResult = parser.ParseArguments<
                    AbstractCommandLineOptions,
                    MigrateCommandLineOptions
                    >(args);

                bool help = false;
                bool success = true;

                parserResult.WithNotParsed(_ =>
                {
                    var helpText = new HelpText(SentenceBuilder.Create(), HelpProvider.GetHeadingText());

                    helpText = HelpText.DefaultParsingErrorsHandler(parserResult, helpText);

                    VerbAttribute verbAttribute = parserResult.TypeInfo.Current.GetCustomAttribute<VerbAttribute>();

                    if (verbAttribute != null)
                    {
                        helpText.AddPreOptionsText(Environment.NewLine + HelpProvider.GetFooterText(verbAttribute.Name));
                    }

                    Console.Error.WriteLine(helpText);

                    success = false;
                });

                parserResult.WithParsed<AbstractCommandLineOptions>(options =>
                {
                    if (options.Help)
                    {
                        string commandName = options.GetType().GetCustomAttribute<VerbAttribute>().Name;

                        Console.WriteLine(HelpProvider.GetHelpText(commandName));

                        help = true;
                        return;
                    }

                    success = false;

                    var defaultVerbosity = Verbosity.Normal;

                    if (options.Verbosity == null
                        || TryParseVerbosity(options.Verbosity, out defaultVerbosity))
                    {
                        ConsoleOut.Verbosity = defaultVerbosity;

                        if (TryParseOutputOptions(options.Output, OptionNames.Output, out string filePath, out Verbosity fileVerbosity, out Encoding encoding, out bool append))
                        {
                            if (filePath != null)
                            {
                                FileMode fileMode = (append)
                                    ? FileMode.Append
                                    : FileMode.Create;

                                var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
                                var writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: false);
                                Out = new TextWriterWithVerbosity(writer) { Verbosity = fileVerbosity };
                            }

                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                });

                if (help)
                    return 0;

                if (!success)
                    return 2;

                return parserResult.MapResult(
                    (MigrateCommandLineOptions options) => Migrate(options),
                    _ => 2);
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
            finally
            {
                Out?.Dispose();
                Out = null;
            }

            return 2;

            static bool IsHelpOption(string value)
            {
                if (value.StartsWith("--"))
                    return string.Compare(value, 2, OptionNames.Help, 0, OptionNames.Help.Length, StringComparison.Ordinal) == 0;

                return value.Length == 2
                    && value[0] == '-'
                    && value[1] == OptionShortNames.Help;
            }
        }

        private static int Migrate(MigrateCommandLineOptions commandLineOptions)
        {
            var options = new MigrateCommandOptions();

            if (!commandLineOptions.TryParse(options))
                return 2;

            return Execute(new MigrateCommand(options));
        }

        private static int Execute<TOptions>(AbstractCommand<TOptions> command) where TOptions : AbstractCommandOptions
        {
            CommandResult result = command.Execute();

            switch (result)
            {
                case CommandResult.Success:
                    return 0;
                case CommandResult.NoMatch:
                    return 1;
                case CommandResult.Fail:
                case CommandResult.Canceled:
                    return 2;
                default:
                    throw new InvalidOperationException($"Unknown enum value '{result}'.");
            }
        }
    }
}
