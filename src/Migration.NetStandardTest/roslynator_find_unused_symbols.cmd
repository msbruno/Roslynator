@echo off

set _roslynatorPath=..
set _msbuildPath="C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"

%_msbuildPath%\msbuild "%_roslynatorPath%\CommandLine.sln" /t:Build /p:Configuration=Debug /v:m /m

"%_roslynatorPath%\CommandLine\bin\Debug\net472\roslynator" find-symbols "..\Migration.sln" ^
 --msbuild-path %_msbuildPath% ^
 --visibility public internal private ^
 --symbol-groups type member ^
 --without-attributes ^
  "System.ObsoleteAttribute" ^
 --unused-only ^
 --verbosity n ^
 --file-log "roslynator.log" ^
 --file-log-verbosity diag

pause
