using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SNS.Data.DataSerializer.DataExtensions
{
    public static class ReflectionUtility
    {
        public static void CacheDataClass<T>() where T : IGenericDataObject<T>, new()
        {
            Type t = typeof(T);
            ReflectionCache<T>.DataClassAttribute = (DataClass)t.GetCustomAttributes(typeof(DataClass), true)[0];
        }

        public static void CacheDataProperties<T>() where T : IGenericDataObject<T>, new()
        {
            Type dataType = typeof(T);
            PropertyInfo[] members = dataType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);

            foreach (PropertyInfo m in members)
            {

                DataProperty[] props = (DataProperty[])System.Attribute.GetCustomAttributes(m, typeof(DataProperty), true);// GetCustomAttributes<DataProperty>(m).ToArray();//(DataProperty[]) System.Attribute.GetCustomAttributes(m,typeof(DataProperty), true);
                if (props.Length > 0)
                {
                    ReflectionCache<T>.CachedProperties.Add(m.Name, new DataPropertyInfo((PropertyInfo)m, props[0]));
                    if (props[0].Key)
                        ReflectionCache<T>.CachedKeys.Add(m.Name, new DataPropertyInfo((PropertyInfo)m, props[0]));
                }
                else
                {
                    CacheDataSubObjects<T>("", "", m);
                }

            }

        }
        public static DataPropertyInfo[] GetDataProperties(Type T)
        {
            return (DataPropertyInfo[])typeof(ReflectionCache<>).MakeGenericType(T).InvokeMember("GetDataProperties", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null);
        }
        public static object GetSubObjectValue(Type T, object o, string[] pathParts, int PathIndex, bool AutoCreate)
        {
            return typeof(ReflectionCache<>).MakeGenericType(T).InvokeMember("GetSubObjectValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, new object[] { o, pathParts, PathIndex, AutoCreate });
        }
        private static void CacheDataSubObjects<T>(string baseFieldPrefix, string baseName, PropertyInfo m) where T : IGenericDataObject<T>, new()
        {

            SubDataObject[] subObjects = (SubDataObject[])m.GetCustomAttributes(typeof(SubDataObject), true);
            if (subObjects.Length > 0)
            {
                string subName = baseName + m.Name;
                ReflectionCache<T>.CachedProperties.Add("|" + subName, new DataPropertyInfo((PropertyInfo)m, null, true, baseFieldPrefix + subObjects[0].FieldPrefix, subName, subObjects[0].UseSubKeys));
                PropertyInfo[] subMembers = ((PropertyInfo)m).PropertyType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);


                foreach (PropertyInfo sm in subMembers)
                {
                    string subFieldPrefix = baseFieldPrefix.Replace("|", "") + subObjects[0].FieldPrefix;
                    DataProperty[] props = (DataProperty[])sm.GetCustomAttributes(typeof(DataProperty), true);
                    if (props.Length > 0)
                    {
                        ReflectionCache<T>.CachedProperties.Add(subFieldPrefix + sm.Name, new DataPropertyInfo((PropertyInfo)sm, props[0], true, subFieldPrefix, subName, subObjects[0].UseSubKeys));
                        if (props[0].Key)
                            ReflectionCache<T>.CachedKeys.Add(m.Name, new DataPropertyInfo((PropertyInfo)m, props[0]));

                    }
                    else
                    {
                        CacheDataSubObjects<T>(subFieldPrefix + "|", subName + "|", sm);

                    }
                }
            }
        }
        public static string GetTable(Type T)
        {
            return (string)typeof(ReflectionCache<>).MakeGenericType(T).InvokeMember("Table", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Static, null, null, null);
        }
        public static string GetSchema(Type T)
        {
            return (string)typeof(ReflectionCache<>).MakeGenericType(T).InvokeMember("Schema", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Static, null, null, null);
        }

    }
}
