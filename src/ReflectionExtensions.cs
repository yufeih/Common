namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    static class ReflectionExtensions
    {
        public static IEnumerable<Type> TryGetExportedTypes(this Assembly assembly)
        {
            try
            {
                if (assembly.IsDynamic) return Enumerable.Empty<Type>();

                return assembly.ExportedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.WriteLine("Error loading assembly: " + assembly.FullName);
                Debug.WriteLine(e.LoaderExceptions[0]);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading assembly: " + assembly.FullName);
                Debug.WriteLine(e);
            }
            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Assembly> LoadReferencedAssemblies(this Assembly assembly, Func<AssemblyName, bool> predicate = null, bool recursive = false)
        {
            var result = new HashSet<Assembly>();
            LoadReferencedAssemblies(assembly, predicate, recursive, result);
            return result;
        }

        private static void LoadReferencedAssemblies(Assembly assembly, Func<AssemblyName, bool> predicate, bool recursive, HashSet<Assembly> result)
        {
            if (assembly != null)
            {
                foreach (var name in assembly.GetReferencedAssemblies())
                {
                    if (predicate == null || predicate(name))
                    {
                        try
                        {
                            var loaded = Assembly.Load(name);

                            if (!result.Add(loaded))
                            {
                                continue;
                            }

                            if (recursive)
                            {
                                LoadReferencedAssemblies(loaded, predicate, recursive, result);
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
