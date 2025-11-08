using MoonSharp.Interpreter;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Caps.Util.Lua
{
    public class LuaEntityLoader
    {
        private Script _script;
        private List<Entity> _entities = new();
        private Dictionary<string, Entity> _symbolLookup = new();
        private Dictionary<string, Entity> _nameLookup = new();
        private Dictionary<Table, Entity> _tableLookup = new();
        private List<(object target, object fieldOrIndex, Table table, Type type)> _pendingReferences = new();

        public LuaEntityLoader(string baseFolder)
        {
            Debug.WriteLine($"[LuaEntityLoader] Initializing loader for folder: {baseFolder}");
            _script = new Script();

            _script.Globals["CreateEntity"] = (Func<DynValue, DynValue>)(data =>
            {
                if (!Directory.Exists(baseFolder))
                    throw new DirectoryNotFoundException($"Lua entity folder not found: {baseFolder}");

                var entity = new Entity();
                var wrapper = new LuaEntityWrapper(entity, _tableLookup, _script);

                if (data.Type == DataType.Table)
                {
                    foreach (var pair in data.Table.Pairs)
                    {
                        string componentName = pair.Key.String;
                        Table componentData = pair.Value.Table;
                        wrapper.AddComponent(componentName, componentData);
                    }
                }

                return UserData.Create(wrapper);
            });

            _script.Globals["print"] = (Func<CallbackArguments, DynValue>)(args =>
            {
                string message = string.Join(" ", args.GetArray().Select(a => a.ToPrintString()));
                Debug.WriteLine($"[Lua print] {message}");
                return DynValue.Nil;
            });

            UserData.RegisterType<LuaEntityWrapper>();
            RegisterAllEnums();

            LoadDataFromFolder(baseFolder);
        }

        private void RegisterAllEnums()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsEnum && type.IsPublic)
                    {
                        // Register with MoonSharp
                        UserData.RegisterType(type);

                        // Expose to Lua by name (e.g., Test)
                        _script.Globals[type.Name] = type;
                    }
                }
            }
        }

        /// <summary>
        /// Loads all Lua entity scripts from the specified folder, recursively following _load_order.lua if present.
        /// </summary>
        public void LoadDataFromFolder(string baseFolder)
        {
            Debug.WriteLine($"[LuaEntityLoader] Loading data from folder: {baseFolder}");
            if (!Directory.Exists(baseFolder))
                throw new DirectoryNotFoundException($"Lua entity folder not found: {baseFolder}");

            var luaFiles = GetLuaFilesInOrder(baseFolder);
            Debug.WriteLine($"[LuaEntityLoader] Lua files to load: {string.Join(", ", luaFiles)}");

            var allScripts = new StringBuilder();
            foreach (var script in luaFiles)
            {
                Debug.WriteLine($"[LuaEntityLoader] Reading Lua file: {script}");
                allScripts.AppendLine(File.ReadAllText(script));
            }
            LoadScript(allScripts.ToString(), baseFolder);
        }

        // Recursively loads Lua files in the order specified by _load_order.lua
        private static List<string> GetLuaFilesInOrder(string folder)
        {
            var result = new List<string>();
            string loadOrderPath = Path.Combine(folder, "_load_order.lua");

            if (File.Exists(loadOrderPath))
            {
                foreach (var line in File.ReadAllLines(loadOrderPath)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("--")))
                {
                    if (line.EndsWith('/') || line.EndsWith('\\'))
                    {
                        // Recurse into subfolder
                        string subfolder = Path.Combine(folder, line.TrimEnd(['/', '\\']));
                        if (Directory.Exists(subfolder))
                        {
                            result.AddRange(GetLuaFilesInOrder(subfolder));
                        }
                    }
                    else
                    {
                        string filePath = Path.Combine(folder, line);
                        if (File.Exists(filePath))
                            result.Add(filePath);
                        else
                            throw new FileNotFoundException(filePath + " not found by LuaEntityLoader.");
                    }
                }
            }
            else
            {
                // No _load_order.lua, just load all .lua files except _load_order.lua
                result.AddRange(Directory.GetFiles(folder, "*.lua", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith("_load_order.lua", StringComparison.OrdinalIgnoreCase)));
            }

            return result;
        }

        public void LoadScript(string luaScript, string baseFolder)
        {
            var debugPath = Path.Combine(Path.GetTempPath(), $"LuaEntityLoader_{baseFolder}.txt");
            File.WriteAllText(debugPath, luaScript);
            try
            {
                Debug.WriteLine("[LuaEntityLoader] Executing Lua script.");
                _script.DoString(luaScript);
            }
            catch (SyntaxErrorException ex)
            {
                Debug.WriteLine($"[LuaEntityLoader] Lua syntax error: {ex.DecoratedMessage}");
                throw new Exception($"Lua syntax error: {ex.DecoratedMessage}", ex);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine($"[LuaEntityLoader] Lua runtime error: {ex.DecoratedMessage}");
                throw new Exception($"Lua runtime error: {ex.DecoratedMessage}", ex);
            }
        }

        public List<Entity> LoadEntitiesFromCategory(string categoryName)
        {
            var result = new List<Entity>();

            void LoadCategoryRecursive(string catName)
            {
                DynValue globalTable = _script.Globals.Get(catName);
                if (globalTable.Type != DataType.Table)
                    throw new Exception($"'{catName}' is not a valid table in Lua.");

                foreach (var pair in globalTable.Table.Pairs)
                {
                    string symbol = pair.Key.String;
                    string qualifiedSymbol = $"{catName}.{symbol}";
                    if (_symbolLookup.ContainsKey(qualifiedSymbol))
                        continue; // Already loaded

                    var entry = pair.Value.Table;
                    var entity = new Entity();
                    var wrapper = new LuaEntityWrapper(entity, _tableLookup, _script);

                    foreach (var comp in entry.Pairs)
                    {
                        string componentName = comp.Key.String;
                        Table componentData = comp.Value.Table;
                        wrapper.AddComponent(componentName, componentData, _pendingReferences);

                        _tableLookup[componentData] = entity;
                    }

                    _entities.Add(entity);
                    _symbolLookup[qualifiedSymbol] = entity;
                    _tableLookup[entry] = entity;

                    foreach (var comp in entity.GetAllComponents())
                    {
                        var nameField = comp.GetType().GetField("Name");
                        if (nameField != null && nameField.FieldType == typeof(string))
                        {
                            var name = (string)nameField.GetValue(comp);
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                string qualifiedName = $"{catName}.{name}";
                                _nameLookup[qualifiedName] = entity;
                            }
                        }
                    }

                    result.Add(entity);
                }
            }

            // Initial load
            LoadCategoryRecursive(categoryName);

            // Recursively load referenced entities not yet loaded
            bool added;
            do
            {
                added = false;
                foreach (var pending in _pendingReferences.ToList())
                {
                    if (pending.table != null && !_tableLookup.ContainsKey(pending.table))
                    {
                        // Try to find which category this table belongs to
                        foreach (var globalKey in _script.Globals.Keys)
                        {
                            string global = globalKey.String;
                            if (string.IsNullOrEmpty(global)) continue;
                            var globalTable = _script.Globals.Get(global).Table;
                            if (globalTable != null)
                            {
                                foreach (var pair in globalTable.Pairs)
                                {
                                    if (pair.Value.Table == pending.table)
                                    {
                                        LoadCategoryRecursive(global);
                                        added = true;
                                        break;
                                    }
                                }
                            }
                            if (added) break;
                        }
                    }
                }
            } while (added);

            // Now resolve all references
            ResolveEntityReferences();

            return result;
        }

        public List<T> LoadComponentsFromCategory<T>(string categoryName)
        {
            var entities = LoadEntitiesFromCategory(categoryName);
            var components = new List<T>();
            foreach (var entity in entities)
            {
                var component = entity.GetComponent<T>();
                if (component != null)
                    components.Add(component);
            }
            return components;
        }

        private void ResolveEntityReferences()
        {
            foreach (var pending in _pendingReferences)
            {
                if (pending.target is IList list && pending.fieldOrIndex is int idx)
                {
                    // List item reference
                    if (_tableLookup.TryGetValue(pending.table, out var resolvedEntity))
                    {
                        var method = typeof(Entity).GetMethod("GetComponent")?.MakeGenericMethod(pending.type);
                        var resolvedComponent = method.Invoke(resolvedEntity, null);
                        list[idx] = resolvedComponent;
                    }
                    else
                    {
                        throw new Exception($"Could not resolve entity reference for list item at index {idx}");
                    }
                }
                else if (pending.target != null && pending.fieldOrIndex is FieldInfo field)
                {
                    // Single field reference (existing logic)
                    if (_tableLookup.TryGetValue(pending.table, out var resolvedEntity))
                    {
                        var fieldType = field.FieldType;

                        if (fieldType == typeof(Entity))
                        {
                            field.SetValue(pending.target, resolvedEntity);
                        }
                        else
                        {
                            var method = typeof(Entity).GetMethod("GetComponent")?.MakeGenericMethod(fieldType);
                            if (method == null)
                                throw new Exception($"GetComponent<{fieldType.Name}> method not found.");

                            var resolvedComponent = method.Invoke(resolvedEntity, null);
                            if (resolvedComponent == null)
                                throw new Exception($"Entity does not contain component of type {fieldType.Name} for field '{field.Name}'");

                            field.SetValue(pending.target, resolvedComponent);
                        }
                    }
                    else
                    {
                        throw new Exception($"Could not resolve entity reference for field '{field.Name}'");
                    }
                }
            }
        }

        public static Type ResolveComponentType(string componentName)
        {
            // First try global
            var type = Type.GetType(componentName);
            if (type != null)
                return type;

            // Then try all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetTypes().FirstOrDefault(t => t.Name == componentName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}
