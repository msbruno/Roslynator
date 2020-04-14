@echo off

"%ProgramFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild" "..\src\Migration.CommandLine\Migration.CommandLine.csproj" ^
 /t:Clean,Build ^
 /p:Configuration=Debug,RunCodeAnalysis=false ^
 /nr:false ^
 /v:normal ^
 /m

if errorlevel 1 (
 pause
 exit
)

dotnet pack -c Debug --no-build -v normal "..\src\Migration.CommandLine\Migration.CommandLine.csproj"

dotnet tool uninstall roslynator.migration.dotnet.cli -g

dotnet tool install roslynator.migration.dotnet.cli --version 0.1.0-rc -g --add-source "..\src\Migration.CommandLine\bin\Debug"

pause