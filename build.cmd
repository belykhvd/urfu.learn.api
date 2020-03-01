@echo off
dotnet build %~dp0\source\WebService.sln -c Release --no-incremental -nr:false -m -nologo -v m
dotnet publish --no-build -c Release -o %~dp0\build\ %~dp0\source\Core\Core.csproj -nr:false -m -nologo -v m -clp:Summary
pause