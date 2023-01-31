using System.Runtime.CompilerServices;
using Arch.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Arch.System.SourceGenerator.Tests
{
public static class CSharpGeneratorRunner
{
    static Compilation baseCompilation = default!;

    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        // running .NET Core system assemblies dir path
        var baseAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var systemAssemblies = Directory.GetFiles(baseAssemblyPath)
            .Where(x =>
            {
                var fileName = Path.GetFileName(x);
                if (fileName.EndsWith("Native.dll")) return false;
                return fileName.StartsWith("System") || (fileName is "mscorlib.dll" or "netstandard.dll");
            });

        var references = systemAssemblies
            .Append(typeof(QueryAttribute).Assembly.Location)
            .Append(typeof(World).Assembly.Location)
            .Append(typeof(CommonArgs).Assembly.Location)
            .Append(typeof(CommunityToolkit.HighPerformance.ArrayExtensions).Assembly.Location)
            .Select(x => MetadataReference.CreateFromFile(x))
            .ToArray();

        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        baseCompilation = compilation;
    }

    public static (GeneratorDriverRunResult,Compilation,Diagnostic[]) RunGenerator(SyntaxTree tree)
    {
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(new QueryGenerator());
        var compilation = baseCompilation.AddSyntaxTrees(tree);
        var x = driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
        GeneratorDriverRunResult result = x.GetRunResult();
        var compilationDiagnostics = newCompilation.GetDiagnostics();
        return (result, newCompilation, diagnostics.Concat(compilationDiagnostics).Where(x => x.Severity == DiagnosticSeverity.Error).ToArray());
    }
}
}