using System.CommandLine;
using CanonicaLib.PackageComparer.Services;

namespace CanonicaLib.PackageComparer;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var package1Argument = new Argument<string>(
            name: "package1",
            description: "First NuGet package (package ID and version, e.g., 'Newtonsoft.Json/13.0.1' or path to .nupkg file)");

        var package2Argument = new Argument<string>(
            name: "package2",
            description: "Second NuGet package (package ID and version, e.g., 'Newtonsoft.Json/13.0.2' or path to .nupkg file)");

        var outputOption = new Option<string?>(
            name: "--output",
            description: "Output file path for the comparison report (default: console output)",
            getDefaultValue: () => null);
        outputOption.AddAlias("-o");

        var formatOption = new Option<string>(
            name: "--format",
            description: "Output format (text, json, markdown)",
            getDefaultValue: () => "text");
        formatOption.AddAlias("-f");

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Show detailed comparison information",
            getDefaultValue: () => false);
        verboseOption.AddAlias("-v");

        var sourceOption = new Option<string?>(
            name: "--source",
            description: "NuGet package source URL (default: nuget.org)",
            getDefaultValue: () => null);
        sourceOption.AddAlias("-s");

        var rootCommand = new RootCommand("CanonicaLib Package Comparer - Compare functionality between two NuGet packages")
        {
            package1Argument,
            package2Argument,
            outputOption,
            formatOption,
            verboseOption,
            sourceOption
        };

        rootCommand.SetHandler(async (package1, package2, output, format, verbose, source) =>
        {
            int exitCode = 0;
            try
            {
                var extractor = new PackageExtractor(source);
                var analyzer = new AssemblyAnalyzer();
                var reporter = new ComparisonReporter();

                Console.WriteLine("Extracting first package...");
                var package1Info = await extractor.ExtractPackageAsync(package1);

                Console.WriteLine("Extracting second package...");
                var package2Info = await extractor.ExtractPackageAsync(package2);

                Console.WriteLine("Analyzing assemblies...");
                var comparison = analyzer.ComparePackages(package1Info, package2Info);

                Console.WriteLine("Generating report...");
                var report = reporter.GenerateReport(comparison, format, verbose);

                if (output != null)
                {
                    await File.WriteAllTextAsync(output, report);
                    Console.WriteLine($"Report written to: {output}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(report);
                }

                // Clean up temporary extraction directories
                extractor.Cleanup(package1Info);
                extractor.Cleanup(package2Info);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                if (verbose)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
                exitCode = 1;
            }

            Environment.ExitCode = exitCode;
        }, package1Argument, package2Argument, outputOption, formatOption, verboseOption, sourceOption);

        return await rootCommand.InvokeAsync(args);
    }
}
