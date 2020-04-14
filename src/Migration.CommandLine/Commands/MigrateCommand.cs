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

namespace Roslynator.CommandLine
{
    internal class MigrateCommand : AbstractCommand<MigrateCommandOptions>
    {
        private const string _mappingData = @"
RCS1023;FormatEmptyBlock;RCS0022;AddNewLineAfterOpeningBraceOfEmptyBlock
RCS1024;FormatAccessorList;RCS0025;AddNewLineBeforeAccessorOfFullProperty
RCS1024;FormatAccessorList;RCS0042;RemoveNewLinesFromAccessorListOfAutoProperty
RCS1024;FormatAccessorList;RCS0043;RemoveNewLinesFromAccessorWithSingleLineExpression
RCS1025;AddNewLineBeforeEnumMember;RCS0031;AddNewLineBeforeEnumMember
RCS1026;AddNewLineBeforeStatement;RCS0033;AddNewLineBeforeStatement
RCS1027;AddNewLineBeforeEmbeddedStatement;RCS0030;AddNewLineBeforeEmbeddedStatement
RCS1028;AddNewLineAfterSwitchLabel;RCS0024;AddNewLineAfterSwitchLabel
RCS1029;FormatBinaryOperatorOnNextLine;RCS0027;AddNewLineBeforeBinaryOperatorInsteadOfAfterIt
RCS1030;AddEmptyLineAfterEmbeddedStatement;RCS0001;AddEmptyLineAfterEmbeddedStatement
RCS1057;AddEmptyLineBetweenDeclarations;RCS0009;AddEmptyLineBetweenDeclarationAndDocumentationComment
RCS1057;AddEmptyLineBetweenDeclarations;RCS0010;AddEmptyLineBetweenDeclarations
RCS1076;FormatDeclarationBraces;RCS0023;AddNewLineAfterOpeningBraceOfTypeDeclaration
RCS1086;UseLinefeedAsNewLine;RCS0045;UseLinefeedAsNewLine
RCS1087;UseCarriageReturnAndLinefeedAsNewLine;RCS0044;UseCarriageReturnAndLinefeedAsNewLine
RCS1088;UseSpacesInsteadOfTab;RCS0046;UseSpacesInsteadOfTab
RCS1092;AddEmptyLineBeforeWhileInDoStatement;RCS0004;AddEmptyLineBeforeClosingBraceOfDoStatement
RCS1153;AddEmptyLineAfterClosingBrace;RCS0008;AddEmptyLineBetweenBlockAndStatement
RCS1183;FormatInitializerWithSingleExpressionOnSingleLine;RCS0048;RemoveNewlinesFromInitializerWithSingleLineExpression
RCS1184;FormatConditionalExpression;RCS0028;AddNewLineBeforeConditionalOperatorInsteadOfAfterIt
RCS1185;FormatSingleLineBlock;RCS0021;AddNewLineAfterOpeningBraceOfBlock
";

        private static readonly Version _version_1_0_0 = new Version(1, 0, 0);

        private static readonly Regex _versionRegex = new Regex(@"\A(?<version>\d+\.\d+\.\d+)(?<suffix>(-.*)?)\z");

        private static readonly ImmutableDictionary<string, ImmutableArray<string>> _mapping = LoadMapping();

        private static ImmutableDictionary<string, ImmutableArray<string>> LoadMapping()
        {
            ImmutableDictionary<string, ImmutableArray<string>>.Builder dic = ImmutableDictionary.CreateBuilder<string, ImmutableArray<string>>();

            using (var sr = new StringReader(_mappingData))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    string[] split = line.Split(';');

                    string oldId = split[0];

                    string newId = split[2];

                    dic[oldId] = (dic.TryGetValue(oldId, out ImmutableArray<string> newIds))
                        ? newIds.Add(newId)
                        : ImmutableArray.Create(newId);
                }
            }

            return dic.ToImmutableDictionary();
        }

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
                return ExecuteDirectory(path, cancellationToken);

            if (File.Exists(path))
                return ExecuteFile(path);

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
                return ExecuteProject(path);
            }

            if (string.Equals(extension, ".ruleset", StringComparison.OrdinalIgnoreCase))
                return ExecuteRuleSet(path);

            return CommandResult.NoMatch;
        }

        private CommandResult ExecuteProject(string path)
        {
            XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace);

            XElement root = document.Root;

            if (root.Attribute("Sdk")?.Value == "Microsoft.NET.Sdk")
            {
                return ExecuteNewProject(path, document);
            }
            else
            {
                return CommandResult.NoMatch;
            }
        }

        private CommandResult ExecuteNewProject(string path, XDocument document)
        {
            IEnumerable<XElement> packageReferences = document.Root.Descendants("ItemGroup").Elements("PackageReference");

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
                return CommandResult.NoMatch;

            if (formattingAnalyzers != null)
            {
                string versionText = formattingAnalyzers.Attribute("Version")?.Value;

                if (versionText != null)
                {
                    Match match = _versionRegex.Match(versionText);

                    if (match == null)
                    {
                        return CommandResult.NoMatch;
                    }

                    if (!match.Success)
                    {
                        return CommandResult.NoMatch;
                    }

                    versionText = match.Groups["version"].Value;

                    string suffix = match.Groups["suffix"]?.Value;

                    if (!Version.TryParse(versionText, out Version version))
                        return CommandResult.NoMatch;

                    if (version > _version_1_0_0
                        || suffix == null)
                    {
                        return CommandResult.NoMatch;
                    }
                }
            }

            if (formattingAnalyzers != null)
            {
                formattingAnalyzers.SetAttributeValue("Version", "1.0.0");
            }
            else
            {
                analyzers.AddAfterSelf(new XElement("PackageReference", new XAttribute("Include", "Roslynator.Formatting.Analyzers"), new XAttribute("Version", "1.0.0")));
            }

            using (XmlWriter xmlWriter = XmlWriter.Create(path, new XmlWriterSettings() { OmitXmlDeclaration = true }))
            {
                document.Save(xmlWriter);
            }

            return CommandResult.Success;
        }

        private CommandResult ExecuteRuleSet(string path)
        {
            XDocument document = XDocument.Load(path);

            var ids = new Dictionary<string, XElement>();

            foreach (XElement element in document.Root.Elements("Rules").Elements("Rule"))
            {
                string id = element.Attribute("Id")?.Value;

                if (id != null)
                    ids[id] = element;
            }

            XElement analyzers = document.Root.Elements("Rules").LastOrDefault(f => f.Attribute("AnalyzerId")?.Value == "Roslynator.CSharp.Analyzers");

            XElement formattingAnalyzers = document.Root.Elements("Rules").FirstOrDefault(f => f.Attribute("AnalyzerId")?.Value == "Roslynator.Formatting.Analyzers");

            if (formattingAnalyzers == null)
            {
                formattingAnalyzers = new XElement(
                    "Rules",
                    new XAttribute("AnalyzerId", "Roslynator.Formatting.Analyzers"),
                    new XAttribute("RuleNamespace", "Roslynator.Formatting.Analyzers"));

                if (analyzers != null)
                {
                    analyzers.AddAfterSelf(formattingAnalyzers);
                }
                else
                {
                    document.Root.Add(formattingAnalyzers);
                }
            }

            bool shouldSave = false;

            foreach (KeyValuePair<string, XElement> kvp in ids)
            {
                if (!_mapping.TryGetValue(kvp.Key, out ImmutableArray<string> newIds))
                    continue;

                foreach (string newId in newIds)
                {
                    if (ids.ContainsKey(newId))
                        continue;

                    var newRule = new XElement(
                        "Rule",
                        new XAttribute("Id", newId),
                        new XAttribute("Action", kvp.Value.Attribute("Action")?.Value ?? "Info"));

                    formattingAnalyzers.Add(newRule);

                    if (kvp.Value.Parent != null)
                        kvp.Value.Remove();

                    shouldSave = true;
                }
            }

            if (shouldSave)
            {
                if (analyzers.IsEmpty)
                    analyzers.Remove();

                var settings = new XmlWriterSettings() { OmitXmlDeclaration = false, Indent = true };

                using (XmlWriter xmlWriter = XmlWriter.Create(path, settings))
                    document.Save(xmlWriter);
            }

            return CommandResult.NoMatch;
        }
    }
}
