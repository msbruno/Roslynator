# How to: Migrate Analyzers to Version 3.0

18 analyzers from package **Roslynator.Analyzers** (Roslynator.CSharp.Analyzers.dll) were rewritten
and moved to package **Roslynator.Formatting.Analyzers** (Roslynator.Formatting.Analyzers.dll).

These analyzers are "formatting" analyzers which means that they:

* are not enabled by default (are opinion-based)
* manipulate only white-space, not the syntax

## List of Analyzers

| Old analyzer                                                                                            | New analyzer                                                                                            |
| ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| [RCS1023](analyzers/RCS1023.md) \(Format empty block\.\)                                            | [RCS0022](analyzers/RCS0022.md) \(Add newline after opening brace of empty block\.\)                |
| [RCS1024](analyzers/RCS1024.md) \(Format accessor list\.\)                                          | [RCS0025](analyzers/RCS0025.md) \(Add newline before accessor of full property\.\)                  |
| [RCS1024](analyzers/RCS1024.md) \(Format accessor list\.\)                                          | [RCS0042](analyzers/RCS0042.md) \(Remove newlines from accessor list of auto\-property\.\)          |
| [RCS1024](analyzers/RCS1024.md) \(Format accessor list\.\)                                          | [RCS0043](analyzers/RCS0043.md) \(Remove newlines from accessor with single\-line expression\.\)    |
| [RCS1025](analyzers/RCS1025.md) \(Add new line before enum member\.\)                               | [RCS0031](analyzers/RCS0031.md) \(Add newline before enum member\.\)                                |
| [RCS1026](analyzers/RCS1026.md) \(Add new line before statement\.\)                                 | [RCS0033](analyzers/RCS0033.md) \(Add newline before statement\.\)                                  |
| [RCS1027](analyzers/RCS1027.md) \(Add new line before embedded statement\.\)                        | [RCS0030](analyzers/RCS0030.md) \(Add newline before embedded statement\.\)                         |
| [RCS1028](analyzers/RCS1028.md) \(Add new line after switch label\.\)                               | [RCS0024](analyzers/RCS0024.md) \(Add newline after switch label\.\)                                |
| [RCS1029](analyzers/RCS1029.md) \(Format binary operator on next line\.\)                           | [RCS0027](analyzers/RCS0027.md) \(Add newline before binary operator instead of after it\.\)        |
| [RCS1030](analyzers/RCS1030.md) \(Add empty line after embedded statement\.\)                       | [RCS0001](analyzers/RCS0001.md) \(Add empty line after embedded statement\.\)                       |
| [RCS1057](analyzers/RCS1057.md) \(Add empty line between declarations\.\)                           | [RCS0009](analyzers/RCS0009.md) \(Add empty line between declaration and documentation comment\.\)  |
| [RCS1057](analyzers/RCS1057.md) \(Add empty line between declarations\.\)                           | [RCS0010](analyzers/RCS0010.md) \(Add empty line between declarations\.\)                           |
| [RCS1076](analyzers/RCS1076.md) \(Format declaration braces\.\)                                     | [RCS0023](analyzers/RCS0023.md) \(Add newline after opening brace of type declaration\.\)           |
| [RCS1086](analyzers/RCS1086.md) \(Use linefeed as newline\.\)                                       | [RCS0045](analyzers/RCS0045.md) \(Use linefeed as newline\.\)                                       |
| [RCS1087](analyzers/RCS1087.md) \(Use carriage return \+ linefeed as newline\.\)                    | [RCS0044](analyzers/RCS0044.md) \(Use carriage return \+ linefeed as newline\.\)                    |
| [RCS1088](analyzers/RCS1088.md) \(Use space\(s\) instead of tab\.\)                                 | [RCS0046](analyzers/RCS0046.md) \(Use spaces instead of tab\.\)                                     |
| [RCS1092](analyzers/RCS1092.md) \(Add empty line before 'while' keyword in 'do' statement\.\)       | [RCS0004](analyzers/RCS0004.md) \(Add empty line before closing brace of 'do' statement\.\)         |
| [RCS1153](analyzers/RCS1153.md) \(Add empty line after closing brace\.\)                            | [RCS0008](analyzers/RCS0008.md) \(Add empty line between block and statement\.\)                    |
| [RCS1183](analyzers/RCS1183.md) \(Format initializer with single expression on single line\.\)      | [RCS0048](analyzers/RCS0048.md) \(Remove newlines from initializer with single\-line expression\.\) |
| [RCS1184](analyzers/RCS1184.md) \(Format conditional expression \(format ? and : on next line\)\.\) | [RCS0028](analyzers/RCS0028.md) \(Add newline before conditional operator instead of after it\.\)   |
| [RCS1185](analyzers/RCS1185.md) \(Format single\-line block\.\)                                     | [RCS0021](analyzers/RCS0021.md) \(Add newline after opening brace of block\.\)                      |


## How to Automatically Migrate to 3.0

If you use any of these analyzers, it is recommended to use command-line tool to migrate them to the new version.

### 1) Install .NET Core global tool 'Roslynator.DotNet.Cli'

```
dotnet tool install roslynator.dotnet.cli --version 0.1.0-rc3 -g
```

### 2) Run command 'roslynator migrate':

```
roslynator migrate <PATH> --identifier roslynator.analyzers --target-version 3.0 [-d|--dry-run]
```

Argument `<PATH>` can represent directory path, csproj/props file path or ruleset file path. Current directory will be used if the argument is not specified.

It is recommended to run analyzers first with `-d|--dry-run` option to see which files will be updated.

Command will do the following:
* Add/update package reference to **Roslynator.Formatting.Analyzers** if the csproj/props contains package reference to **Roslynator.Analyzers**
* Update rules in the ruleset
