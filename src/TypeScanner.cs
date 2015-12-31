namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using Nine.Injection;

    static class TypeScanner
    {
        public static IEnumerable<Type> GetTypesFromAssembliesInBasePath(Func<Assembly, bool> predicate = null)
        {
            var assemblies = GetAssembliesInBasePath();
            if (predicate != null)
            {
                assemblies = assemblies.Where(predicate);
            }
            return GetTypesFromAssembly(assemblies.ToArray());
        }
        
        public static IEnumerable<Type> GetTypesFromAssembly(params Assembly[] assemblies)
        {
            try
            {
                return assemblies.SelectMany(a => a.ExportedTypes.Where(ContainerExtensions.IsInjectible));
            }
            catch (Exception)
            {
                return new Type[0];
            }
        }

        public static IEnumerable<Assembly> GetAssembliesInBasePath()
        {
            string basePath;

            try
            {
                // http://stackoverflow.com/questions/22830345/unity-3-configuration-by-convention-not-finding-types-in-web-project
                basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (SecurityException)
            {
                return new Assembly[0];
            }

            return GetAssemblyNames(basePath).Select(an => LoadAssembly(Path.GetFileNameWithoutExtension(an))).Where(an => an != null);
        }

        private static IEnumerable<string> GetAssemblyNames(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path, "*.dll");
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private static Assembly LoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
