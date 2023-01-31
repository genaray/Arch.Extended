using System.Diagnostics;
using System.Reflection;
using Arch.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MemoryStream = System.IO.MemoryStream;

namespace Arch.System.SourceGenerator.Tests
{
    public class GeneratorTests
    {
        [Test]
        public void SingleMethod()
        {
            var source = CSharpSyntaxTree.ParseText(File.ReadAllText($"../../../TestClass/{nameof(Tests.SingleMethod)}.cs"));
            (GeneratorDriverRunResult result, Compilation compilation, Diagnostic[] diagnostics) generator = CSharpGeneratorRunner.RunGenerator(source);
            var method = GetMatchMethod(generator.result,"DoJobQuery");
            Debug.Assert(method != null, "Method not found");
            MatchParameter(method, "World", typeof(CommonArgs).FullName);
            Debug.Assert(generator.diagnostics.Length == 0,string.Join("\n",generator.diagnostics.Select(x=>x.ToString())));
            using (var memory = new MemoryStream())
            {
                generator.compilation.Emit(memory);
                var ass = Assembly.Load(memory.ToArray());
                World world = World.Create();
                world.Create(new PositionComponent{x = 1,y = 1,z = 1});
                world.Create(new TestComponent(),new PositionComponent{x = 1,y = 1,z = 1});
                var system = (BaseSystem<World, CommonArgs>)Activator.CreateInstance(ass.GetType(typeof(SingleMethod).FullName), args: new[] { world });
                system.Initialize();
                var args = new CommonArgs(1);
                CheckSystemEvent(system, args);
                Debug.Assert((int)system.GetType().GetField("IsWork").GetValue(system) == 1,"system failed to execute.");
            }
        }
        
        [Test]
        public void MultipleMethod()
        {
            var source = CSharpSyntaxTree.ParseText(File.ReadAllText($"../../../TestClass/{nameof(Tests.MultipleMethod)}.cs"));
            (GeneratorDriverRunResult result, Compilation compilation, Diagnostic[] diagnostics) generator = CSharpGeneratorRunner.RunGenerator(source);
            for (int i = 0; i < 5; i++)
            {
                var method = GetMatchMethod(generator.result,"DoJob" + i + "Query");
                MatchParameter(method, "World");
                Debug.Assert(method != null, "Method not found");
            }
            using (var memory = new MemoryStream())
            {
                generator.compilation.Emit(memory);
                var ass = Assembly.Load(memory.ToArray());
                World world = World.Create();
                world.Create(new TestComponent(),new PositionComponent());
                var system = (BaseSystem<World, CommonArgs>)Activator.CreateInstance(ass.GetType(typeof(MultipleMethod).FullName), args: new[] { world });
                system.Initialize();
                var args = new CommonArgs(1);
                CheckSystemEvent(system, args);
                Debug.Assert((int)system.GetType().GetField("IsWork").GetValue(system) == 4,"system failed to execute.");
            }
            Debug.Assert(generator.diagnostics.Length == 0,string.Join("\n",generator.diagnostics.Select(x=>x.ToString())));
        }
        
        [Test]
        public void EmptyQueryParameter()
        {
            var source = CSharpSyntaxTree.ParseText(File.ReadAllText($"../../../TestClass/{nameof(Tests.EmptyQueryParameter)}.cs"));
            (GeneratorDriverRunResult result, Compilation compilation, Diagnostic[] diagnostics) generator = CSharpGeneratorRunner.RunGenerator(source);
            var method = GetMatchMethod(generator.result,"DoJobQuery");
            MatchParameter(method, "World");
            Debug.Assert(method != null, "Method not found");
            Debug.Assert(generator.diagnostics.Length == 0,string.Join("\n",generator.diagnostics.Select(x=>x.ToString())));
            using (var memory = new MemoryStream())
            {
                generator.compilation.Emit(memory);
                var ass = Assembly.Load(memory.ToArray());
                World world = World.Create();
                world.Create(new TestComponent(),new PositionComponent());
                var system = (BaseSystem<World, CommonArgs>)Activator.CreateInstance(ass.GetType(typeof(EmptyQueryParameter).FullName), args: new[] { world });
                system.Initialize();
                var args = new CommonArgs(1);
                CheckSystemEvent(system, args);
                Debug.Assert((int)system.GetType().GetField("IsWork").GetValue(system) == 1,"system failed to execute.");
            }
        }
        
        [Test]
        public void OnlyDataParameter()
        {
            var source = CSharpSyntaxTree.ParseText(File.ReadAllText($"../../../TestClass/{nameof(Tests.OnlyDataParameter)}.cs"));
            (GeneratorDriverRunResult result, Compilation compilation, Diagnostic[] diagnostics) generator = CSharpGeneratorRunner.RunGenerator(source);
            var method = GetMatchMethod(generator.result,"DoJobQuery");
            MatchParameter(method, "World",typeof(CommonArgs).FullName);
            Debug.Assert(method != null, "Method not found");
            Debug.Assert(generator.diagnostics.Length == 0,string.Join("\n",generator.diagnostics.Select(x=>x.ToString())));
            using (var memory = new MemoryStream())
            {
                generator.compilation.Emit(memory);
                var ass = Assembly.Load(memory.ToArray());
                World world = World.Create();
                world.Create(new TestComponent());
                var system = (BaseSystem<World, CommonArgs>)Activator.CreateInstance(ass.GetType(typeof(OnlyDataParameter).FullName), args: new[] { world });
                system.Initialize();
                CommonArgs args = new CommonArgs(1);
                //Call Update only, check if the Update data is passed properly
                //The cause of the error is that the Update function of the code generator uses the parameter name "Short Type Name" while the method internally calls Data,
                //so the real parameter is not passed down. I don't know if this is intentional.
                system.Update(args);
                CheckSystemEvent(system, args);
                Debug.Assert((int)system.GetType().GetField("IsWork").GetValue(system) == 1,"system failed to execute.");
            }
        }

        private static void CheckSystemEvent<T>(BaseSystem<World, T> system, T args)
        {
            system.BeforeUpdate(args);
            system.Update(args);
            system.AfterUpdate(args);
        }

        private static void MatchParameter(MethodDeclarationSyntax? method, params string[] parameters)
        {
            int count = method.ParameterList.Parameters.Count;
            if(parameters.Length != count)
                Debug.Assert(false, "Parameter not Match");
            for (var index = 0; index < count; index++)
            {
                var node = method.ParameterList.Parameters[index];
                if (node.Type.ToString() != parameters[index])
                    Debug.Assert(false, "Parameter not Match");
            }
        }

        private static MethodDeclarationSyntax? GetMatchMethod(GeneratorDriverRunResult result,string methodName)
        {
            MethodDeclarationSyntax? method = null;
            foreach (var node in result.Results.SelectMany(node => node.GeneratedSources))
            {
                SyntaxNode? first = null;
                foreach (var t1 in node.SyntaxTree.GetRoot().DescendantNodes())
                {
                    if (t1 is MethodDeclarationSyntax syntax && syntax.Identifier.Text == methodName)
                    {
                        first = t1;
                        break;
                    }
                }
                method = first as MethodDeclarationSyntax;
                if (method != null)
                    break;
            }
            return method;
        }
    }
}