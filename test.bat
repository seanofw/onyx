@echo off
dotnet test --no-build --configuration Debug /p:AltCover=true /p:AltCoverAssemblyExcludeFilter="Microsoft.Test|Microsoft.VisualStudio|NUnit3|AltCover|testhost" -v m
