using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Caps.Util.Lua
{
    public static class LuaFunctionWrapper
    {
        public static CallbackFunction WrapMethodWithTypeConversion(MethodInfo method, object target = null)
        {
            return new CallbackFunction((context, args) =>
            {
                var parameters = method.GetParameters();
                var convertedArgs = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var arg = args[i].ToObject(paramType);

                    // If the parameter expects a Type and the Lua value is a string, convert it
                    if (paramType == typeof(Type) && args[i].Type == DataType.String)
                    {
                        arg = LuaEntityWrapper.ResolveTypeFromString(args[i].String);
                    }

                    convertedArgs[i] = arg;
                }

                var result = method.Invoke(target, convertedArgs);
                return DynValue.FromObject(context.GetScript(), result);
            });
        }
    }
}
