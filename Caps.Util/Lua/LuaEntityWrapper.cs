using MoonSharp.Interpreter;
using System.Collections;
using System.Reflection;

namespace Caps.Util.Lua
{
    public class LuaEntityWrapper
    {
        private readonly Entity _entity;
        private readonly Dictionary<MoonSharp.Interpreter.Table, Entity> _tableLookup;
        public Entity Entity => _entity;

        public LuaEntityWrapper(Entity entity, Dictionary<MoonSharp.Interpreter.Table, Entity> tableLookup)
        {
            _entity = entity;
            _tableLookup = tableLookup;
        }

        public void AddComponent(string componentName, Table data, List<(object target, object fieldOrIndex, Table table, Type type)> pendingReferences = null)
        {
            // Use new type resolver that searches all loaded assemblies
            Type componentType = LuaEntityLoader.ResolveComponentType(componentName);
            if (componentType == null)
                throw new Exception($"Component type '{componentName}' not found in any loaded assembly.");

            var instance = Activator.CreateInstance(componentType);

            foreach (var pair in data.Pairs)
            {
                string fieldName = pair.Key.String;
                DynValue value = pair.Value;

                FieldInfo field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                    continue;

                try
                {
                    // For each field in the component:
                    if (value.Type != DataType.Nil)
                    {
                        if (value.Type == DataType.Table && (field.FieldType.IsClass || field.FieldType.IsGenericType))
                            field.SetValue(instance, ConvertLuaTableToObject(value.Table, field.FieldType, pendingReferences, instance, field));
                        else
                            field.SetValue(instance, value.ToObject(field.FieldType));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error setting field '{fieldName}' on component '{componentName}': {ex.Message}", ex);
                }
            }

            ValidateRequiredFields(data, componentType);

            _entity.AddComponent(instance);
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

        private object ConvertLuaTableToObject(
            Table table,
            Type targetType,
            List<(object target, object fieldOrIndex, Table table, Type type)> pendingReferences = null,
            object parentInstance = null,
            FieldInfo parentField = null)
        {
            // Try to get the component of the correct type
            // Replace this block inside ConvertLuaTableToObject:
            if (_tableLookup != null && _tableLookup.TryGetValue(table, out var entity))
            {
                // Try to get the component of the correct type
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
                        if (pair.Type == DataType.Table && elementType.IsClass && elementType != typeof(string))
                        {
                            // Try to resolve as a reference first
                            if (_tableLookup != null && _tableLookup.ContainsKey(pair.Table))
                            {
                                // Reference: defer resolution
                                list.Add(null);
                                if (pendingReferences != null && parentInstance != null && parentField != null)
                                {
                                    pendingReferences.Add((list, idx, pair.Table, elementType));
                                }
                            }
                            else
                            {
                                // Value: instantiate directly from the table
                                var valueObj = ConvertLuaTableToObject(pair.Table, elementType, pendingReferences);
                                list.Add(valueObj);
                            }
                        }
                        else
                        {
                            list.Add(pair.ToObject(elementType));
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
                        object key = pair.Key.ToObject(keyType);
                        object value;
                        if (pair.Value.Type == DataType.Table && !IsSimpleType(valueType))
                            value = ConvertLuaTableToObject(pair.Value.Table, valueType);
                        else
                            value = pair.Value.ToObject(valueType);
                        dict.Add(key, value);
                    }
                    return dict;
                }
            }

            // Handle simple types and custom classes
            if (targetType.IsClass && targetType != typeof(string))
            {
                // Validate all required fields are present
                ValidateRequiredFields(table, targetType);

                var obj = Activator.CreateInstance(targetType);
                foreach (var field in targetType.GetFields())
                {
                    if (table.Get(field.Name) is DynValue val && val.Type != DataType.Nil)
                    {
                        if (val.Type == DataType.Table && (field.FieldType.IsClass || field.FieldType.IsGenericType))
                            field.SetValue(obj, ConvertLuaTableToObject(val.Table, field.FieldType));
                        else
                            field.SetValue(obj, val.ToObject(field.FieldType));
                    }
                }
                return obj;
            }

            // Handle primitives and enums
            if (IsSimpleType(targetType) || targetType.IsEnum)
            {
                // Try to get the first value in the table (for single-value tables)
                var first = table.Values.FirstOrDefault();
                if (first != null)
                    return first.ToObject(targetType);
                throw new InvalidOperationException($"Cannot convert Lua table to simple type {targetType.Name}.");
            }

            // If we reach here, we don't know how to convert
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
    }
}
