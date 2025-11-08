using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caps.Util.Lua
{
    public static class LuaRegistrations
    {
        public static void RegisterNamespacePrefixTypes(string namespacePrefix, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t =>
                    (t.IsClass || (t.IsValueType && !t.IsEnum && !t.IsPrimitive)) &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith(namespacePrefix)
                );

            foreach (var type in types)
            {
                UserData.RegisterType(type);
            }
        }
    }
}
