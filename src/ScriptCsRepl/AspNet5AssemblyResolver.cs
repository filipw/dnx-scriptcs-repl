using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Runtime;
using ScriptCs.Contracts;

namespace ScriptCsRepl
{
    public class AspNet5AssemblyResolver : IAssemblyResolver
    {
        private readonly ILibraryManager _libraryManager;

        public AspNet5AssemblyResolver(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public IEnumerable<string> GetAssemblyPaths(string path, bool binariesOnly = false)
        {
            var assemblies = _libraryManager.GetLibraries().SelectMany(x => x.Assemblies).Select(x =>
            {
                try
                {
                    return Assembly.Load(x);
                }
                catch (Exception)
                {
                }

                return null;
            });

            return assemblies.Where(x => !string.IsNullOrEmpty(x?.Location)).Select(x => x.Location);
        }
    }
}