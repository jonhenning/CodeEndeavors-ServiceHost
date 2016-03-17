using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CodeEndeavors.ServiceHost
{
    public class Resources
    {
        public static List<string> GetNames(Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            var name = assembly.GetName().Name;
            return assembly.GetManifestResourceNames().Select(n => n.Substring(name.Length + 1)).ToList();
        }

        public static List<string> GetNames(string ns, Assembly assembly = null)
        {
            return GetNames(assembly).Where(n => n.StartsWith(ns)).ToList();
        }

        public static string GetText(string name, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();

            var ns = assembly.GetName().Name;
            var resourceName = ns + "." + name;

            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }

    }
}