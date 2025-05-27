using System.Reflection;
using System.Runtime.ExceptionServices;
using Arch.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Arch.System.SourceGenerator.Tests;

/// <summary>
///     Tests <see cref="QueryGenerator"/> by individually compiling and running each test system from
///     Arch.System.SourceGenerator.TestCompilation.
///     The tests themselves are run in the Arch.System.SourceGenerator.TestCompilation project as well,
///     but here they are run a second time in complete isolation.
///     These tests also ensure that the generated code matches the expected output.
/// </summary>
/// <remarks>
///     For future devs: If you're having trouble with specific test errors, first make sure
///     your system does not use Primary Constructors or Collection Initializers.
/// </remarks>
[TestFixture]
internal class SnapshotTest
{
    public static string ProjectName { get; } =
        $"{nameof(Arch)}.{nameof(System)}.{nameof(SourceGenerator)}.Tests";

    /// <summary>
    ///     Loads the compilation into memory and tests the specified system.
    /// </summary>
    /// <param name="compilation">The compilation to test.</param>
    /// <param name="testSystemName">The name of the system to test.</param>
    private static void TestSystem(Compilation compilation, string testSystemName)
    {
        // Load the assembly
        using var memory = new MemoryStream();
        compilation.Emit(memory);
        var assembly = Assembly.Load(memory.ToArray());

        // A system needs a world
        using var world = World.Create();

        // Find the system type in the assembly
        var systemType = assembly.GetType($"{ProjectName}.{testSystemName}");
        Assert.That(systemType, Is.Not.Null, $"System type {testSystemName} not found in assembly {assembly.FullName}.");

        // Create an instance of the system
        var system = Activator.CreateInstance(systemType, new[] { world }) as BaseSystem<World, int>;
        Assert.That(system, Is.Not.Null, $"Ensure {testSystemName} has a constructor that takes a single World param.");

        // Find the Test and Setup methods in the system
        var testMethod = system.GetType().GetMethod("Test", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(testMethod, Is.Not.Null, $"Method 'Test' not found in system {testSystemName}.");
        var setupMethod = system.GetType().GetMethod("Setup", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(setupMethod, Is.Not.Null, $"Method 'Setup' not found in system {testSystemName}.");

        try
        {
            // Test the system!
            setupMethod.Invoke(system, []);
            testMethod.Invoke(system, []);
        }
        catch (TargetInvocationException ex)
        {
            // Throw the inner exception for a more useful log
            ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
        }
    }

    /// <summary>
    /// Verifies the compilation of the specified folder from Arch.System.SourceGenerator.TestCompilation,
    /// and tests the system, if provided.
    /// </summary>
    /// <param name="compilationFolder">The folder containing the compilation files.</param>
    /// <param name="testSystemName">The name of the system to test, or null if the compilation doesn't point to a BaseTestSystem.</param>
    private static void VerifyCompilation(string compilationFolder, string? testSystemName)
    {
        var solutionPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        Assert.That(solutionPath, Is.Not.Null, "Could not find project path.");

        var projectPath = Path.Combine(solutionPath, ProjectName);
        Assert.That(projectPath, Is.Not.Null, "Could not find project path.");

        var compilationDirectory = new DirectoryInfo(Path.Combine(projectPath, compilationFolder));
        Assert.That(compilationDirectory.Exists, Is.True, $"Could not find {compilationFolder} compilation directory.");

        // Get all .cs files in the compilation directory
        var csFiles = compilationDirectory.GetFiles("*.cs", SearchOption.AllDirectories);

        // Ensure that the expected files are NOT present,
        // so they don't conflict with the source-generated output.
        var sourceCsFiles = csFiles
            .Where(file => !file.FullName.Contains("ExpectedGeneration"));
        Assert.That(sourceCsFiles, Is.Not.Empty, "No source files found for compilation.");

        // Include shared files in the compilation
        var sharedDirectory = new DirectoryInfo(Path.Combine(projectPath, "Shared"));
        sourceCsFiles = sourceCsFiles.Concat(sharedDirectory.GetFiles("*.cs", SearchOption.AllDirectories));

        // Parse all the source files
        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest);
        var sourceTrees = sourceCsFiles
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file.FullName), parseOptions));

        // Get the system assembly references
        var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var systemAssemblies = Directory.GetFiles(baseAssemblyPath!)
           .Where(x => !x.EndsWith("Native.dll"))
           .Where(x =>
           {
               var filename = Path.GetFileName(x);
               return filename.StartsWith("System") || (filename is "mscorlib.dll" or "netstandard.dll");
           });

        // Get any additional references needed for the compilation
        IEnumerable<string> references = [
            typeof(World).Assembly.Location,
            typeof(Assert).Assembly.Location,
            typeof(QueryAttribute).Assembly.Location,
            typeof(CommunityToolkit.HighPerformance.ArrayExtensions).Assembly.Location
        ];

        references = references.Concat(systemAssemblies);

        // Run the compilation with the source trees and references
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release);
        var inputCompilation = CSharpCompilation.Create(compilationFolder, sourceTrees,
            references: references.Select(r => MetadataReference.CreateFromFile(r)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Run the generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new QueryGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // Check for compilation diagnostics
        var outputDiagnostics = outputCompilation.GetDiagnostics();
        var errors = outputDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        var warnings = outputDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
        Assert.Multiple(() =>
        {
            Assert.That(diagnostics, Is.Empty, "Generator diagnostics should be empty." + string.Join("\n", diagnostics));
            Assert.That(errors, Is.Empty,
                "Output compilation should not have errors.\n" + string.Join("\n", errors));
            Warn.Unless(warnings, Is.Empty,
                "Output compilation should not have warnings.\n" + string.Join("\n", warnings));
        });

        // Compare the generated files with the expected files
        var expectedFiles = csFiles
            .Where(file => file.FullName.Contains("ExpectedGeneration"))
            .Select(file => (Name: file.Name, Text: File.ReadAllText(file.FullName)))
            .OrderBy(x => x.Name).ToArray();

        var generatedFiles = driver.GetRunResult().GeneratedTrees
            .Select(tree => (Name: Path.GetFileName(tree.FilePath), Text: tree.GetText().ToString()))
            .Where(x => x.Name != "Attributes.g.cs") // Skip the attributes file
            .OrderBy(x => x.Name).ToArray();

        Assert.That(generatedFiles, Is.EqualTo(expectedFiles));

        // If we're not testing a specific BaseTestSystem, we're done!
        if (testSystemName is not null)
        {
            TestSystem(outputCompilation, testSystemName);
        }
    }

    [Test]
    public void BasicCompilation()
    {
        VerifyCompilation(nameof(BasicCompilation), "BasicSystem");
    }

    [Test]
    public void AttributeQueryCompilation()
    {
        VerifyCompilation(nameof(AttributeQueryCompilation), "AttributeQuerySystem");
    }

    [Test]
    public void ParamQueryCompilation()
    {
        VerifyCompilation(nameof(ParamQueryCompilation), "ParamQuerySystem");
    }

    [Test]
    public void DataParamCompilation()
    {
        VerifyCompilation(nameof(DataParamCompilation), "DataParamSystem");
    }

    [Test]
    public void GeneratedUpdateCompilation()
    {
        VerifyCompilation(nameof(GeneratedUpdateCompilation), "GeneratedUpdateSystem");
    }
}
