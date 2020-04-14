// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using static Roslynator.Logger;

namespace Roslynator.CommandLine
{
    internal class MigrateCommand : AbstractCommand<MigrateCommandOptions>
    {
        private static readonly Version _version_1_0_0 = new Version(1, 0, 0);
        private static readonly Regex _versionRegex = new Regex(@"\A(?<version>\d+\.\d+\.\d+)(?<suffix>(-.*)?)\z");

        public MigrateCommand(MigrateCommandOptions options) : base(options)
        {
        }

        protected override CommandResult ExecuteCore(CancellationToken cancellationToken = default)
        {
            var result = CommandResult.NoMatch;

            foreach (string path in Options.Paths)
            {
                CommandResult result2 = ExecutePath(path, cancellationToken);

                if (result2 == CommandResult.Canceled)
                    return CommandResult.Canceled;

                if (result != CommandResult.Fail
                    && result != CommandResult.Success)
                {
                    result = result2;
                }
            }

            return CommandResult.Success;
        }

        private CommandResult ExecutePath(string path, CancellationToken cancellationToken)
        {
            if (Directory.Exists(path))
            {
                WriteLine($"Search directory '{path}'", Verbosity.Normal);
                return ExecuteDirectory(path, cancellationToken);
            }
            else if (File.Exists(path))
            {
                WriteLine($"Search file '{path}'", Verbosity.Normal);
                return ExecuteFile(path);
            }
            else
            {
                WriteLine($"File or directory not found: '{path}'", Verbosity.Normal);
            }

            return CommandResult.NoMatch;
        }

        private CommandResult ExecuteDirectory(string directoryPath, CancellationToken cancellationToken)
        {
            var result = CommandResult.NoMatch;

            var enumerationOptions = new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true };

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.*", enumerationOptions))
            {
                CommandResult result2 = ExecuteFile(filePath);

                if (result2 == CommandResult.Canceled)
                    return CommandResult.Canceled;

                if (result != CommandResult.Fail
                    && result != CommandResult.Success)
                {
                    result = result2;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        private CommandResult ExecuteFile(string path)
        {
            string extension = Path.GetExtension(path);

            if (string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".props", StringComparison.OrdinalIgnoreCase))
            {
                if (!GeneratedCodeUtility.IsGeneratedCodeFile(path))
                    return ExecuteProject(path);
            }
            else if (string.Equals(extension, ".ruleset", StringComparison.OrdinalIgnoreCase))
            {
                if (!GeneratedCodeUtility.IsGeneratedCodeFile(path))
                    return ExecuteRuleSet(path);
            }

            WriteLine(path, Verbosity.Diagnostic);
            return CommandResult.NoMatch;
        }

        private CommandResult ExecuteProject(string path)
        {
            WriteLine($"Analyze project file '{path}'", Verbosity.Normal);

            XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace);

            XElement root = document.Root;

            if (root.Attribute("Sdk")?.Value == "Microsoft.NET.Sdk")
            {
                return ExecuteNewProject(path, document);
            }
            else
            {
                //TODO: Migrate old project
                return CommandResult.NoMatch;
            }
        }

        private CommandResult ExecuteNewProject(string path, XDocument document)
        {
            IEnumerable<XElement> packageReferences = document.Root
                .Descendants("ItemGroup")
                .Elements("PackageReference");

            XElement analyzers = null;
            XElement formattingAnalyzers = null;

            foreach (XElement e in packageReferences)
            {
                string packageId = e.Attribute("Include")?.Value;

                if (packageId == null)
                    continue;

                if (packageId == "Roslynator.Formatting.Analyzers")
                    formattingAnalyzers = e;

                if (packageId == "Roslynator.Analyzers")
                    analyzers = e;
            }

            if (analyzers == null)
            {
                WriteLine($"Package reference 'Roslynator.Analyzers' not found in '{path}'.", Verbosity.Normal);
                return CommandResult.NoMatch;
            }

            if (formattingAnalyzers != null)
            {
                string versionText = formattingAnalyzers.Attribute("Version")?.Value;

                if (versionText == null)
                {
                    WriteLine($"Version attribute not found: '{formattingAnalyzers}'", Colors.Message_Warning, Verbosity.Normal);
                    return CommandResult.NoMatch;
                }

                if (versionText != null)
                {
                    Match match = _versionRegex.Match(versionText);

                    if (match?.Success != true)
                    {
                        WriteLine($"Invalid version: '{formattingAnalyzers}'", Colors.Message_Warning, Verbosity.Normal);
                        return CommandResult.NoMatch;
                    }

                    versionText = match.Groups["version"].Value;

                    string suffix = match.Groups["suffix"]?.Value;

                    if (!Version.TryParse(versionText, out Version version))
                    {
                        WriteLine($"Invalid version: '{formattingAnalyzers}'", Colors.Message_Warning, Verbosity.Normal);
                        return CommandResult.NoMatch;
                    }

                    if (version > _version_1_0_0
                        || suffix == null)
                    {
                        return CommandResult.NoMatch;
                    }
                }
            }

            if (formattingAnalyzers != null)
            {
                WriteLine("Updating 'Roslynator.Formatting.Analyzers' to '1.0.0'", Colors.Message_OK, Verbosity.Normal);
                formattingAnalyzers.SetAttributeValue("Version", "1.0.0");
            }
            else
            {
                WriteLine("Adding reference to package 'Roslynator.Formatting.Analyzers'", Colors.Message_OK, Verbosity.Normal);
                analyzers.AddAfterSelf(new XElement("PackageReference", new XAttribute("Include", "Roslynator.Formatting.Analyzers"), new XAttribute("Version", "1.0.0")));
            }

            WriteLine($"Save changes to '{path}'", Colors.Message_OK, Verbosity.Minimal);

            if (!Options.DryRun)
            {
                var settings = new XmlWriterSettings() { OmitXmlDeclaration = true };

                using (XmlWriter xmlWriter = XmlWriter.Create(path, settings))
                    document.Save(xmlWriter);
            }

            return CommandResult.Success;
        }

        private CommandResult ExecuteRuleSet(string path)
        {
            WriteLine($"Analyze ruleset file '{path}'", Verbosity.Normal);

            XDocument document = XDocument.Load(path);

            var ids = new Dictionary<string, XElement>();

            foreach (XElement element in document.Root.Elements("Rules").Elements("Rule"))
            {
                string id = element.Attribute("Id")?.Value;

                if (id != null)
                    ids[id] = element;
            }

            XElement analyzers = document.Root.Elements("Rules").LastOrDefault(f => f.Attribute("AnalyzerId")?.Value == "Roslynator.CSharp.Analyzers");

            if (analyzers == null)
            {
                WriteLine("Rules for 'Roslynator.CSharp.Analyzers' not found", Verbosity.Normal);
                return CommandResult.NoMatch;
            }

            XElement formattingAnalyzers = document.Root.Elements("Rules").FirstOrDefault(f => f.Attribute("AnalyzerId")?.Value == "Roslynator.Formatting.Analyzers");

            if (formattingAnalyzers == null)
            {
                formattingAnalyzers = new XElement(
                    "Rules",
                    new XAttribute("AnalyzerId", "Roslynator.Formatting.Analyzers"),
                    new XAttribute("RuleNamespace", "Roslynator.Formatting.Analyzers"));

                analyzers.AddAfterSelf(formattingAnalyzers);
            }

            bool shouldSave = false;

            foreach (KeyValuePair<string, XElement> kvp in ids)
            {
                if (!AnalyzersMapping.Mapping.TryGetValue(kvp.Key, out ImmutableArray<string> newIds))
                    continue;

                foreach (string newId in newIds)
                {
                    if (ids.ContainsKey(newId))
                        continue;

                    string action = kvp.Value.Attribute("Action")?.Value ?? "Info";
                    var newRule = new XElement(
                        "Rule",
                        new XAttribute("Id", newId),
                        new XAttribute("Action", action));

                    WriteLine($"Update rule '{kvp.Key}' to '{newId}' ({action})", Colors.Message_OK, Verbosity.Normal);

                    formattingAnalyzers.Add(newRule);

                    if (kvp.Value.Parent != null)
                        kvp.Value.Remove();

                    shouldSave = true;
                }
            }

            if (shouldSave)
            {
                WriteLine($"Save changes to '{path}'", Colors.Message_OK, Verbosity.Minimal);

                if (!Options.DryRun)
                {
                    if (analyzers.IsEmpty)
                        analyzers.Remove();

                    var settings = new XmlWriterSettings() { OmitXmlDeclaration = false, Indent = true };

                    using (XmlWriter xmlWriter = XmlWriter.Create(path, settings))
                    {
                        document.Save(xmlWriter);
                    }
                }
            }

            return CommandResult.NoMatch;
        }
    }
}
