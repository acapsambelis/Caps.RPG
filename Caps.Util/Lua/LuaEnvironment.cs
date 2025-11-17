using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caps.Util.Lua
{
    public class LuaEnvironment
    {
        private readonly Script _script;

        public LuaEnvironment(string namespaceName)
        {
            _script = new Script();
            RegisterNamespace(namespaceName);
        }

        public LuaEntityLoader LoadFromFolder(string folderName)
        {
            return new LuaEntityLoader(folderName, _script);
        }

        private void RegisterNamespace(string namespaceName)
        {
            var rulesAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == namespaceName);

            if (rulesAssembly == null)
            {
                // Load from output directory
                var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{namespaceName}.dll");
                rulesAssembly = Assembly.LoadFrom(assemblyPath);
            }

            RegisterNamespacePrefixTypes(_script, namespaceName, rulesAssembly);
            //RegisterNamespacePrefixFunctions(_script, namespaceName, rulesAssembly);
        }

        /// <summary>
        /// Registers all types in the given namespace prefix with MoonSharp.
        /// </summary>
        private static void RegisterNamespacePrefixTypes(Script script, string namespacePrefix, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t =>
                    (t.IsClass || (t.IsValueType && !t.IsEnum && !t.IsPrimitive)) &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith(namespacePrefix)
                );

            List<string> registeredTypeNames = new List<string>();
            foreach (var type in types)
            {
                registeredTypeNames.Add(type.FullName ?? type.Name);
                UserData.RegisterType(type);
                script.Globals[type.Name] = UserData.CreateStatic(type);
            }
        }

        /// <summary>
        /// Registers all public static methods in the given namespace prefix as Lua global functions,
        /// wrapping them so that string arguments are converted to Type where needed.
        /// </summary>
        private static void RegisterNamespacePrefixFunctions(Script script, string namespacePrefix, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith(namespacePrefix)
                );

            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    // Only register if the method has at least one parameter of type Type
                    if (method.GetParameters().Any(p => p.ParameterType == typeof(Type)))
                    {
                        // If there are overloads, only register the first one found for each name.
                        if (!script.Globals.Keys.Any(k => k.String == method.Name))
                        {
                            script.Globals[method.Name] = LuaFunctionWrapper.WrapMethodWithTypeConversion(method);
                        }
                    }
                }
            }
        }
    }
}