using MoonSharp.Interpreter;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Caps.Util.Lua
{
    public class LuaEntityWrapper
    {
        private readonly Entity _entity;
        private readonly Dictionary<Table, Entity> _tableLookup;
        private readonly Script _script;
        public Entity Entity => _entity;

        public LuaEntityWrapper(Entity entity, Dictionary<Table, Entity> tableLookup, Script script)
        {
            _entity = entity;
            _tableLookup = tableLookup;
            _script = script;
            Debug.WriteLine($"[LuaEntityWrapper] Created for Entity Id={entity.Id}");
        }

        public void AddComponent(string componentName, Table data, List<(object target, object fieldOrIndex, Table table, Type type)> pendingReferences = null)
        {
            Debug.WriteLine($"[LuaEntityWrapper] Adding component '{componentName}' to Entity Id={_entity.Id}");
            Type componentType = LuaEntityLoader.ResolveComponentType(componentName);
            if (componentType == null)
            {
                Debug.WriteLine($"[LuaEntityWrapper] Component type '{componentName}' not found.");
                throw new Exception($"Component type '{componentName}' not found in any loaded assembly.");
            }

            var instance = Activator.CreateInstance(componentType);

            foreach (var pair in data.Pairs)
            {
                string fieldName = pair.Key.String;
                DynValue value = pair.Value;

                var field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                if (field != null)
                {
                    if (value.Type == DataType.Function && typeof(Delegate).IsAssignableFrom(field.FieldType))
                    {
                        var del = BindLuaFunction(data, fieldName, field.FieldType, _script);
                        field.SetValue(instance, del);
                        continue;
                    }
                    if (value.Type != DataType.Nil)
                    {
                        object convertedValue;
                        if (value.Type == DataType.Table && (field.FieldType.IsClass || field.FieldType.IsGenericType))
                            convertedValue = ConvertLuaTableToObject(value.Table, field.FieldType, pendingReferences, instance, field);
                        else
                            convertedValue = value.ToObject(field.FieldType);

                        field.SetValue(instance, convertedValue);
                        Debug.WriteLine($"[LuaEntityWrapper] Set field '{fieldName}' on '{componentName}' to '{convertedValue}'");
                    }
                    continue;
                }

                var prop = componentType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                if (prop != null && prop.CanWrite)
                {
                    if (value.Type == DataType.Function && typeof(Delegate).IsAssignableFrom(prop.PropertyType))
                    {
                        var del = BindLuaFunction(data, fieldName, prop.PropertyType, _script);
                        prop.SetValue(instance, del);
                        continue;
                    }
                    if (value.Type != DataType.Nil)
                    {
                        object convertedValue;
                        if (value.Type == DataType.Table && (prop.PropertyType.IsClass || prop.PropertyType.IsGenericType))
                            convertedValue = ConvertLuaTableToObject(value.Table, prop.PropertyType, pendingReferences, instance, null);
                        else
                            convertedValue = value.ToObject(prop.PropertyType);

                        prop.SetValue(instance, convertedValue);
                        Debug.WriteLine($"[LuaEntityWrapper] Set property '{fieldName}' on '{componentName}' to '{convertedValue}'");
                    }
                    continue;
                }

                throw new ArgumentException($"Could not find object with name {fieldName}");
            }

            ValidateRequiredFields(data, componentType);
            ValidateAllFields(data, componentType);

            _entity.AddComponent(instance);
            Debug.WriteLine($"[LuaEntityWrapper] Component '{componentName}' added to Entity Id={_entity.Id}");
        }

        private void ValidateRequiredFields(Table table, Type targetType)
        {
            // Only validate for classes (not primitives or strings)
            if (!targetType.IsClass || targetType == typeof(string))
                return;

            var missingFields = new List<string>();
            foreach (var field in targetType.GetFields())
            {
                // Only check fields marked with [Required]
                if (field.GetCustomAttribute(typeof(Required)) != null)
                {
                    if (table.Get(field.Name).Type == DataType.Nil)
                        missingFields.Add(field.Name);
                }
            }

            if (missingFields.Count > 0)
            {
                var keys = string.Join(", ", table.Keys.Select(k => k.ToPrintString()));
                throw new Exception(
                    $"Lua table is missing required fields for type '{targetType.Name}': {string.Join(", ", missingFields)}. " +
                    $"Found keys: [{keys}]. " +
                    $"Check your Lua file for correct field names and casing."
                );
            }
        }

        private void ValidateAllFields(Table table, Type targetType)
        {
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var key in table.Keys)
            {
                string keyName = key.String;
                bool foundInFields = fields.Any(f => f.Name == keyName);
                bool foundInProperties = properties.Any(p => p.Name == keyName);

                if (!foundInFields && !foundInProperties)
                {
                    throw new Exception(
                        $"Lua table contains key '{keyName}' which does not match any public field or property of type '{targetType.Name}'."
                    );
                }
            }
        }

        private object ConvertLuaTableToObject(
            Table table,
            Type targetType,
            List<(object target, object fieldOrIndex, Table table, Type type)> pendingReferences = null,
            object parentInstance = null,
            FieldInfo parentField = null)
        {
            Debug.WriteLine($"[LuaEntityWrapper] Began converting an object of type {targetType.Name}.");
            if (targetType.Name.Equals("CreatureInventory"))
                Debug.WriteLine($"[LuaEntityWrapper] Breakpoint.");

            if (_tableLookup != null && _tableLookup.TryGetValue(table, out var entity))
            {
                var method = typeof(Entity).GetMethod("GetComponent")?.MakeGenericMethod(targetType);
                if (method != null)
                {
                    var component = method.Invoke(entity, null);
                    if (component != null)
                        return component;
                }
            }

            if (targetType.IsGenericType)
            {
                var genericTypeDef = targetType.GetGenericTypeDefinition();

                // Handle List<T>
                if (genericTypeDef == typeof(List<>))
                {
                    var elementType = targetType.GetGenericArguments()[0];
                    var listObj = Activator.CreateInstance(targetType);
                    if (listObj is not IList list)
                        throw new InvalidOperationException($"Cannot create instance of List<{elementType.Name}>.");
                    int idx = 0;
                    foreach (var pair in table.Values)
                    {
                        object valueToAdd = null;
                        if (pair.Type == DataType.Table && elementType.IsClass && elementType != typeof(string))
                        {
                            if (_tableLookup != null && _tableLookup.ContainsKey(pair.Table))
                            {
                                list.Add(null);
                                if (pendingReferences != null && parentInstance != null && parentField != null)
                                {
                                    pendingReferences.Add((list, idx, pair.Table, elementType));
                                }
                            }
                            else
                            {
                                valueToAdd = ConvertLuaTableToObject(pair.Table, elementType, pendingReferences);
                                list.Add(valueToAdd);
                            }
                        }
                        else
                        {
                            valueToAdd = pair.ToObject(elementType);
                            list.Add(valueToAdd);
                        }
                        idx++;
                    }
                    return list;
                }

                // Handle Dictionary<TKey, TValue>
                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    var keyType = targetType.GetGenericArguments()[0];
                    var valueType = targetType.GetGenericArguments()[1];
                    var dict = (IDictionary)Activator.CreateInstance(targetType);
                    foreach (var pair in table.Pairs)
                    {
                        object key;
                        if (keyType == typeof(Type) && pair.Key.Type == DataType.String)
                            key = ResolveTypeFromString(pair.Key.String);
                        else
                            key = pair.Key.ToObject(keyType);

                        object value;
                        if (valueType == typeof(Type) && pair.Value.Type == DataType.String)
                            value = ResolveTypeFromString(pair.Value.String);
                        else if (pair.Value.Type == DataType.Table && !IsSimpleType(valueType))
                            value = ConvertLuaTableToObject(pair.Value.Table, valueType);
                        else
                            value = pair.Value.ToObject(valueType);
                        dict.Add(key, value);
                    }
                    return dict;
                }
            }
            else if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                var array = Array.CreateInstance(elementType, table.Values.Count());
                int idx = 0;
                foreach (var pair in table.Values)
                {
                    object value = null;
                    if (pair.Type == DataType.Table && elementType.IsClass && elementType != typeof(string))
                    {
                        if (_tableLookup != null && _tableLookup.ContainsKey(pair.Table))
                        {
                            array.SetValue(null, idx);
                            if (pendingReferences != null && parentInstance != null && parentField != null)
                            {
                                pendingReferences.Add((array, idx, pair.Table, elementType));
                            }
                        }
                        else
                        {
                            value = ConvertLuaTableToObject(pair.Table, elementType, pendingReferences);
                            array.SetValue(value, idx);
                        }
                    }
                    else
                    {
                        value = pair.ToObject(elementType);
                        array.SetValue(value, idx);
                    }
                    idx++;
                }
                return array;
            }

            // Handle simple types and custom classes
            if (targetType.IsClass && targetType != typeof(string))
            {
                ValidateRequiredFields(table, targetType);
                ValidateAllFields(table, targetType);
                var obj = Activator.CreateInstance(targetType);

                // Set public fields
                foreach (var field in targetType.GetFields())
                {
                    if (table.Get(field.Name) is DynValue val && val.Type != DataType.Nil)
                    {
                        object fieldValue;
                        if (val.Type == DataType.Function && typeof(Delegate).IsAssignableFrom(field.FieldType))
                        {
                            // Handle delegate assignment from Lua function
                            fieldValue = BindLuaFunction(table, field.Name, field.FieldType, _script);
                        }
                        else if (val.Type == DataType.Table && (field.FieldType.IsClass || field.FieldType.IsGenericType))
                        {
                            fieldValue = ConvertLuaTableToObject(val.Table, field.FieldType);
                        }
                        else
                        {
                            fieldValue = val.ToObject(field.FieldType);
                        }

                        field.SetValue(obj, fieldValue);
                    }
                }

                // Set public properties
                foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    if (!prop.CanWrite) continue;
                    if (table.Get(prop.Name) is DynValue val && val.Type != DataType.Nil)
                    {
                        object propValue;
                        if (val.Type == DataType.Function && typeof(Delegate).IsAssignableFrom(prop.PropertyType))
                        {
                            // Handle delegate assignment from Lua function
                            propValue = BindLuaFunction(table, prop.Name, prop.PropertyType, _script);
                        }
                        else if (val.Type == DataType.Table && (prop.PropertyType.IsClass || prop.PropertyType.IsGenericType))
                        {
                            propValue = ConvertLuaTableToObject(val.Table, prop.PropertyType);
                        }
                        else
                        {
                            propValue = val.ToObject(prop.PropertyType);
                        }

                        prop.SetValue(obj, propValue);
                    }
                }

                return obj;
            }

            // Handle primitives and enums
            if (IsSimpleType(targetType) || targetType.IsEnum)
            {
                var first = table.Values.FirstOrDefault();
                if (first != null)
                    return first.ToObject(targetType);
                throw new InvalidOperationException($"Cannot convert Lua table to simple type {targetType.Name}.");
            }

            throw new InvalidOperationException($"Cannot convert Lua table to type {targetType.Name}.");
        }

        private static bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal);
        }

        private static Type ResolveTypeFromString(string typeName)
        {
            Type? type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName)
                ?? throw new Exception($"Type '{typeName}' not found in any loaded assemblies.");
            return type;
        }

        /// <summary>
        /// Converts a Lua function in a Table to a C# delegate of the specified type.
        /// </summary>
        public static Delegate? BindLuaFunction(Table table, string fieldName, Type delegateType, Script script)
        {
            var dynValue = table.Get(fieldName);
            if (dynValue.Type != DataType.Function)
                return null;

            var closure = dynValue.Function;
            var invokeMethod = delegateType.GetMethod("Invoke");
            var parameters = invokeMethod.GetParameters();

            // Build a lambda that matches the delegate signature
            var paramExprs = parameters.Select(p => System.Linq.Expressions.Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var selfExpr = paramExprs[0];
            var argsArrayExpr = System.Linq.Expressions.Expression.NewArrayInit(
                typeof(object),
                paramExprs.Skip(1).Select(p =>
                    p.Type.IsValueType
                        ? System.Linq.Expressions.Expression.Convert(p, typeof(object))
                        : (System.Linq.Expressions.Expression)p
                )
            );

            var callExpr = System.Linq.Expressions.Expression.Call(
                typeof(LuaEntityWrapper).GetMethod(nameof(InvokeLuaFunction), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static),
                System.Linq.Expressions.Expression.Constant(script),
                System.Linq.Expressions.Expression.Constant(closure),
                selfExpr,
                argsArrayExpr
            );

            System.Linq.Expressions.Expression body;
            if (invokeMethod.ReturnType == typeof(void))
            {
                // Discard the result if the delegate returns void
                body = System.Linq.Expressions.Expression.Block(callExpr);
            }
            else
            {
                // Convert the result to the expected return type
                body = System.Linq.Expressions.Expression.Convert(callExpr, invokeMethod.ReturnType);
            }

            var lambda = System.Linq.Expressions.Expression.Lambda(delegateType, body, paramExprs);
            return lambda.Compile();
        }

        // Helper method to call Lua function
        private static object InvokeLuaFunction(Script script, Closure closure, object self, object[] args)
        {
            var luaArgs = new DynValue[args.Length + 1];
            luaArgs[0] = DynValue.FromObject(script, self);
            for (int i = 0; i < args.Length; i++)
                luaArgs[i + 1] = DynValue.FromObject(script, args[i]);
            var result = script.Call(closure, luaArgs);
            return result.ToObject(typeof(object));
        }
    }
}
