using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace SNS.Data.DataSerializer.DataExtensions
{
    internal abstract class ReflectionCache<T> where T : IGenericDataObject<T>, new()
    {
        static T instRef = new T();
        static DataClass dataClassAttribute;
        static SortedList<string, DataPropertyInfo> cachedProperties;
        static SortedList<string, DataPropertyInfo> cachedKeys;

       
        static void CheckDataClassCache()
        {
            if (dataClassAttribute == null)
            {
                try
                {
                    ReflectionUtility.CacheDataClass<T>();
                }
                catch
                {
                    throw new ApplicationException("No Data Class Attribute Specified For: " + typeof(T).ToString());
                }
            }
        }
        public static DataClass DataClassAttribute
        {
            get { return ReflectionCache<T>.dataClassAttribute; }
            set { ReflectionCache<T>.dataClassAttribute = value; }
        }
        public static SortedList<string, DataPropertyInfo> CachedProperties
        {
            get { return ReflectionCache<T>.cachedProperties; }
            set { ReflectionCache<T>.cachedProperties = value; }
        }
        public static SortedList<string, DataPropertyInfo> CachedKeys
        {
            get { return ReflectionCache<T>.cachedKeys; }
            set { ReflectionCache<T>.cachedKeys = value; }
        }
        public static T InstanceReference
        {
            get { return ReflectionCache<T>.instRef; }
            set { ReflectionCache<T>.instRef = value; }
        }
        public static string ConnectorName
        {
            get
            {
                CheckDataClassCache();
                return DataClassAttribute.ConnectorName;
            }
        }
        public static string Table
        {
            get
            {
                CheckDataClassCache();
                if (DataClassAttribute.TableName == "")
                    throw new ApplicationException("Create, Read, Update, and Delete operations cannot be performed on this object, it has no table specified in the data class attribute (" + typeof(T).ToString() + "). This type may still have stored procedures and SQL utilized to load it.");
                return DataClassAttribute.TableName;

                
            }
        }
        public static string Schema
        {
            get
            {
                CheckDataClassCache();
                return DataClassAttribute.Schema;
            }
        }
        public static void CheckCache()
        {
            lock (ReflectionCache<T>.InstanceReference)
            {
                if (ReflectionCache<T>.CachedProperties == null)
                {
                    ReflectionCache<T>.CachedKeys = new SortedList<string, DataPropertyInfo>();
                    ReflectionCache<T>.CachedProperties = new SortedList<string, DataPropertyInfo>();
                    ReflectionUtility.CacheDataProperties<T>();
                }
            }
        }
        public static DataPropertyInfo[] GetKeys()
        {
            lock (ReflectionCache<T>.InstanceReference)
            {
                ReflectionCache<T>.CheckCache();
                //DataPropertyInfo[] Info = new DataPropertyInfo[ReflectionCache<T>.CachedProperties.Count];
                return ReflectionCache<T>.CachedKeys.Values.ToArray();//CopyTo(Info, 0);
            }
        }
        public static DataPropertyInfo[] GetDataProperties()
        {
            lock (ReflectionCache<T>.InstanceReference)
            {
                ReflectionCache<T>.CheckCache();
                //DataPropertyInfo[] Info = new DataPropertyInfo[ReflectionCache<T>.CachedProperties.Count];
                return ReflectionCache<T>.CachedProperties.Values.ToArray();//CopyTo(Info, 0);
            }
        }
        public static DataPropertyInfo GetDataProperty(string Name) 
        {
            lock (ReflectionCache<T>.InstanceReference)
            {
                ReflectionCache<T>.CheckCache();
                if (ReflectionCache<T>.CachedProperties.ContainsKey(Name))
                    return ReflectionCache<T>.CachedProperties[Name];
                else
                    return null;
            }
        }
        public static object GetSubObjectValue(object o, string[] pathParts, int PathIndex, bool AutoCreate)
        {
            if (PathIndex < pathParts.Length)
            {
                string propInfoStr = "";
                for (int pi = 0; pi <= PathIndex; pi++)
                {
                    propInfoStr += "|" + pathParts[pi];
                }
                DataPropertyInfo info = GetDataProperty(propInfoStr);
                if (info == null)
                    return null;
                object so = info.PropertyInfo.GetValue(o, null);
                if (so == null)
                {
                    if (AutoCreate)
                    {
                        info.PropertyInfo.SetValue(o, info.PropertyInfo.PropertyType.InvokeMember("", BindingFlags.CreateInstance, null, null, null), null);
                        so = info.PropertyInfo.GetValue(o, null);
                    }
                    else
                    {
                        return null;
                    }
                }
                return GetSubObjectValue(so, pathParts, PathIndex + 1, AutoCreate);
            }
            else
            {
                return o;
            }
        }
    }
}
