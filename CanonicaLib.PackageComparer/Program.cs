using System.CommandLine;
using CanonicaLib.PackageComparer.Services;
using Microsoft.Extensions.Configuration;

namespace CanonicaLib.PackageComparer;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Load configuration from appsettings files
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Use the app's base directory, not current directory
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
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

        var apiKeyOption = new Option<string?>(
            name: "--api-key",
            description: "OpenAI API key for generating AI-powered migration guides",
            getDefaultValue: () => null);
        apiKeyOption.AddAlias("-k");

        var migrationGuideOption = new Option<string?>(
            name: "--migration-guide",
            description: "Output path for AI-generated migration guide (requires --api-key)",
            getDefaultValue: () => null);
        migrationGuideOption.AddAlias("-m");

        var rootCommand = new RootCommand("CanonicaLib Package Comparer - Compare functionality between two NuGet packages")
        {
            package1Argument,
            package2Argument,
            outputOption,
            formatOption,
            verboseOption,
            sourceOption,
            apiKeyOption,
            migrationGuideOption
        };

        rootCommand.SetHandler(async (package1, package2, output, format, verbose, source, apiKey, migrationGuidePath) =>
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

                // Generate AI migration guide if migration guide path provided
                if (!string.IsNullOrWhiteSpace(migrationGuidePath))
                {
                    // Resolve API key from: command line > environment variable > config file
                    var resolvedApiKey = apiKey 
                        ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                        ?? configuration["OpenAI:ApiKey"];

                    if (string.IsNullOrWhiteSpace(resolvedApiKey))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nError: OpenAI API key required for migration guide generation.");
                        Console.WriteLine("Provide it via:");
                        Console.WriteLine("  1. --api-key parameter");
                        Console.WriteLine("  2. OPENAI_API_KEY environment variable");
                        Console.WriteLine("  3. appsettings.Development.json file");
                        Console.ResetColor();
                        exitCode = 1;
                    }
                    else
                    {

                    Console.WriteLine();
                    Console.WriteLine("Generating AI-powered migration guide...");
                    
                    try
                    {
                        var migrationGenerator = new MigrationGuideGenerator(resolvedApiKey);
                        var detailedReport = reporter.GenerateReport(comparison, "markdown", verbose: true);
                        var migrationGuide = await migrationGenerator.GenerateMigrationGuideAsync(comparison, detailedReport);

                        // Save developer guide
                        var developerGuidePath = Path.Combine(
                            Path.GetDirectoryName(migrationGuidePath) ?? ".",
                            Path.GetFileNameWithoutExtension(migrationGuidePath) + "-developer.md");
                        await File.WriteAllTextAsync(developerGuidePath, migrationGuide.DeveloperGuide);
                        Console.WriteLine($"✓ Developer migration guide written to: {developerGuidePath}");

                        // Save AI assistant prompt
                        var aiPromptPath = Path.Combine(
                            Path.GetDirectoryName(migrationGuidePath) ?? ".",
                            Path.GetFileNameWithoutExtension(migrationGuidePath) + "-ai-prompt.txt");
                        await File.WriteAllTextAsync(aiPromptPath, migrationGuide.AIAssistantPrompt);
                        Console.WriteLine($"✓ AI assistant prompt written to: {aiPromptPath}");

                        Console.WriteLine();
                        Console.WriteLine("Migration guide generation complete!");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Warning: Failed to generate migration guide: {ex.Message}");
                        if (verbose)
                        {
                            Console.Error.WriteLine(ex.StackTrace);
                        }
                        exitCode = 1;
                    }
                    }
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
        }, package1Argument, package2Argument, outputOption, formatOption, verboseOption, sourceOption, apiKeyOption, migrationGuideOption);

        return await rootCommand.InvokeAsync(args);
    }
}
