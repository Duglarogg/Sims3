using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Duglarogg.AbductorSpace.Helpers
{
    public class AssemblyCheck
    {
        static Dictionary<string, bool> sAssemblies = new Dictionary<string, bool>();

        public static string GetNamespace(Assembly assembly)
        {
            Type type = assembly.GetType("NRaas.VersionStamp");

            if (type == null) return null;

            FieldInfo nameSpaceField = type.GetField("sNamespace", BindingFlags.Static | BindingFlags.Public);

            if (nameSpaceField == null) return null;

            return nameSpaceField.GetValue(null) as string;
        }

        public static bool IsInstalled(string assembly)
        {
            if (string.IsNullOrEmpty(assembly)) return false;

            assembly = assembly.ToLower();

            bool loaded;

            if (sAssemblies.TryGetValue(assembly, out loaded))
            {
                return loaded;
            }

            loaded = (FindAssembly(assembly) != null);
            sAssemblies.Add(assembly, loaded);

            return loaded;
        }

        public static Assembly FindAssembly(string name)
        {
            name = name.ToLower();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name.ToLower() == name)
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}
