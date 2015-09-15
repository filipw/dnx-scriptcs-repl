using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScriptCs;
using ScriptCs.Hosting;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Microsoft.Dnx.Runtime;
using ScriptCs.Engine.Mono;

namespace ScriptCsRepl
{
    public class Program
    {
        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
        private readonly ILibraryManager _libraryManager;

        public Program(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        private static readonly IConsole Console = new ScriptConsole();

        public void Main(string[] args)
        {
            var assemblyReslover = new AspNet5AssemblyResolver(_libraryManager);
            var scriptServicesBuilder =
                new ScriptServicesBuilder(Console, new DefaultLogProvider()).Cache(false).Repl(true);

            scriptServicesBuilder = IsMono ? scriptServicesBuilder.ScriptEngine<MonoScriptEngine>() : scriptServicesBuilder.ScriptEngine<RoslynScriptEngine>();

            ((ScriptServicesBuilder)scriptServicesBuilder).Overrides[typeof (IAssemblyResolver)] = assemblyReslover;

            var scriptcs = scriptServicesBuilder.Build();

            scriptcs.Repl.Initialize(assemblyReslover.GetAssemblyPaths(string.Empty), Enumerable.Empty<IScriptPack>());

            try
            {
                while (ExecuteLine(scriptcs.Repl))
                {
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = oldColor;
            }

        }

        private static bool ExecuteLine(IRepl repl)
        {
            Console.Write(string.IsNullOrWhiteSpace(repl.Buffer) ? "> " : "* ");

            try
            {
                var line = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    repl.Execute(line);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
