@echo off
REM Example usage of CanonicaLib Package Comparer

echo.
echo ====================================================================
echo CanonicaLib Package Comparer - Example Usage
echo ====================================================================
echo.

REM Example 1: Compare two versions of a package from NuGet.org
echo Example 1: Comparing Newtonsoft.Json 13.0.1 vs 13.0.3
echo --------------------------------------------------------------------
dotnet run --project CanonicaLib.PackageComparer -- "Newtonsoft.Json/13.0.1" "Newtonsoft.Json/13.0.3"

echo.
echo.
echo ====================================================================
echo.

REM Example 2: Generate a markdown report
echo Example 2: Generating markdown report
echo --------------------------------------------------------------------
dotnet run --project CanonicaLib.PackageComparer -- "Newtonsoft.Json/13.0.1" "Newtonsoft.Json/13.0.3" -f markdown -o comparison-report.md -v

echo.
echo Report saved to comparison-report.md
echo.

REM Example 3: Generate a JSON report for automation
REM echo Example 3: Generating JSON report
REM echo --------------------------------------------------------------------
REM dotnet run --project CanonicaLib.PackageComparer -- "YourPackage/1.0.0" "YourPackage/2.0.0" -f json -o report.json

pause
