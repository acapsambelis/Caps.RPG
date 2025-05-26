using SNS.Data.DataSerializer.DataExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SNS.Data.DataSerializer.XmlExtensions
{
    public static class Xml<T> where T : IGenericDataObject<T>, new()
    {
        public static string ToXml(T Object)
        {
            return Object.ToXml();
        }
        public static string ToXml(IEnumerable<T> Objects)
        {
            return Objects.ToXml();
        }
        public static string ToXml(IEnumerable<T> Objects, bool UseBase64Arrays, IDataRelation[] Relationships)
        {
            int count = Objects.Count();
            if (count == 0)
                return "";
            string rootNodeName = ReflectionCache<T>.Table;
            StringBuilder sb = new StringBuilder("<" + rootNodeName + " Count=\"" + count + "\">\r\n");
            foreach (T obj in Objects)
            {
                sb.Append(
                obj.ToXml(UseBase64Arrays,Relationships));
            }
            sb.Append("</" + rootNodeName + ">\r\n");

            return sb.ToString();
        }
        public static T LoadOneFromXml(string XmlString)
        {
            T[] results = LoadFromXml(XmlString, false);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T[] LoadFromXml(string XmlString)
        {
            return LoadFromXml(XmlString, false);
        }
        internal static T[] LoadFromXml(string XmlString, bool LoadingOne)
        {
            try
            {
                T returnObj = default(T);
                List<T> objs = new List<T>();
                if (XmlString == "")
                    return objs.ToArray();
                using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XmlString)))
                {
                    using (XmlTextReader reader = new XmlTextReader(ms))
                    {
                        Type thisType = typeof(T);
                        string typeName = thisType.Name;
                        bool inType = false;
                        bool readContent = false;

                        while (readContent || reader.Read())
                        {
                            readContent = false;
                            if (!inType && reader.Name == typeName && reader.IsStartElement() && reader.GetAttribute("Count") == null)
                            {
                                returnObj = new T();
                                objs.Add(returnObj);
                                inType = true;
                            }
                            else if (reader.Name == typeName && !reader.IsStartElement())
                            {
                                inType = false;
                                if (LoadingOne)
                                    break;
                            }
                            else if (inType)
                            {
                                //if (reader.HasValue)
                                //{
                                if (reader.Name != "" && reader.IsStartElement())
                                {
                                    if (reader["Count"] == null)
                                    {
                                        if (reader["ItemCount"] == null)
                                        {
                                            object setOnObj = returnObj;
                                            string propName = reader.Name;
                                            if (propName.Contains("."))
                                            {
                                                string objectPath = "";
                                                string[] classPath = propName.Split(".".ToCharArray());
                                                for (int i = 0; i < classPath.Length - 1; i++)
                                                {
                                                    objectPath += classPath[i] + "|";
                                                }
                                                objectPath = objectPath.Remove(objectPath.Length - 1, 1);
                                                propName = classPath[classPath.Length - 1];
                                                setOnObj = ReflectionUtility.GetSubObjectValue(returnObj.GetType(), returnObj, objectPath.Split("|".ToCharArray()), 0, true);
                                                if (setOnObj == null) //No longer exists
                                                    continue; 
                                            }
                                            CultureInfo invC = new CultureInfo("");
                                            object value = reader.ReadElementContentAsObject();
                                            readContent = true;
                                            DataPropertyInfo matchPropInfo = ReflectionUtility.GetDataProperties(typeof(T)).Where(p => p.PropertyAttribute != null && (p.PropertyInfo.DeclaringType == setOnObj.GetType() || setOnObj.GetType().IsSubclassOf(p.PropertyInfo.DeclaringType)) && p.PropertyAttribute.GetColumn.ToLower() == propName.ToLower()).FirstOrDefault();
                                            if (matchPropInfo == null)
                                                continue;
                                            PropertyInfo info = matchPropInfo.PropertyInfo;//matchPropInfo.PropertyInfo;//setOnObj.GetType().GetProperty(propName);
                                            Type changeToType;
                                            if (info != null)
                                            {
                                                //TODO: Consolidate these blocks of code to a function
                                                if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                                                {
                                                    changeToType = Nullable.GetUnderlyingType(info.PropertyType);
                                                    value = Convert.ChangeType(value, changeToType, invC);

                                                }
                                                else if (info.PropertyType.IsEnum)
                                                {
                                                    value = Enum.Parse(info.PropertyType, value.ToString());
                                                }
                                                else
                                                {
                                                    changeToType = info.PropertyType;
                                                    value = Convert.ChangeType(value, changeToType, invC);
                                                }
                                                info.SetValue(setOnObj, value, null);
                                            }
                                        }
                                        else
                                        {
                                            ReadXmlList(reader, returnObj);
                                        }
                                    }
                                    else
                                    {
                                        //Relationships ignore them and call load again with that type.
                                        string nodeName = reader.Name;
                                        while (reader.Read())
                                        {

                                            if (reader.Name == nodeName && !reader.IsStartElement())
                                                break;
                                        }
                                    }
                                }
                                //}
                            }
                        }
                    }
                }
                return objs.ToArray();
            }
            catch(Exception err)
            {
                Console.WriteLine(err.Message + " " + err.StackTrace);
            }
            return null;
        }
        private static object ReadXmlList(XmlTextReader reader, T returnObj, bool IsSubList = false, string SubNodeName = "", Type SubListType = null)
        {

            int countEle = Convert.ToInt32(reader["ItemCount"]);
            string nodeName = SubNodeName == "" ? reader.Name : SubNodeName;
            object setOnObj = returnObj;
            PropertyInfo info = null;
            if (!IsSubList)
            {
                string propName = reader.Name;
                if (propName.Contains("."))
                {
                    string objectPath = "";
                    string[] classPath = propName.Split(".".ToCharArray());
                    for (int i = 0; i < classPath.Length - 1; i++)
                    {
                        objectPath += classPath[i] + "|";
                    }
                    objectPath = objectPath.Remove(objectPath.Length - 1, 1);
                    propName = classPath[classPath.Length - 1];
                    setOnObj = ReflectionUtility.GetSubObjectValue(returnObj.GetType(), returnObj, objectPath.Split("|".ToCharArray()), 0, true);

                }
                DataPropertyInfo matchPropInfo = ReflectionUtility.GetDataProperties(typeof(T)).Where(p => p.PropertyAttribute != null && p.PropertyAttribute.GetColumn.ToLower() == propName.ToLower()).FirstOrDefault();
                if (matchPropInfo == null) //Property Removed...? just skip it and 
                {
                    while (reader.Read())
                    {
                        if (reader.Name == nodeName && !reader.IsStartElement())
                            break;
                    }
                    return null;
                }
                info = matchPropInfo.PropertyInfo;//matchPropInfo.PropertyInfo;//setOnObj.GetType().GetProperty(propName);
            }
            Array setArray = null;
            IList setList = null;
            IDictionary setDictionary = null;
            Type itemType = null;
            Type keyType = null;
            if (!IsSubList)
            {
                if (info.PropertyType.IsArray)
                {
                    itemType = info.PropertyType.GetElementType();
                    setArray = Array.CreateInstance(itemType, countEle);
                    //info.SetValue(setOnObj, setArray, null);
                }
                else if (info.PropertyType.GetInterfaces().Any(t => t == typeof(IDictionary)))
                {
                    keyType = info.PropertyType.GetGenericArguments()[0];
                    itemType = info.PropertyType.GetGenericArguments()[1];
                    setDictionary = (IDictionary)info.PropertyType.InvokeMember("", BindingFlags.CreateInstance, null, null, null);

                }
                else
                {
                    itemType = info.PropertyType.GetGenericArguments()[0];
                    setList = (IList)info.PropertyType.InvokeMember("", BindingFlags.CreateInstance, null, null, null);
                    //info.SetValue(setOnObj, setList, null);
                }
            }
            else
            {
                if (SubListType.IsArray)
                {
                    itemType = SubListType.GetElementType();
                    setArray = Array.CreateInstance(itemType, countEle);
                    //info.SetValue(setOnObj, setArray, null);
                }
                else if (SubListType.GetInterfaces().Any(t => t == typeof(IDictionary)))
                {
                    keyType = SubListType.GetGenericArguments()[0];
                    itemType = SubListType.GetGenericArguments()[1];
                    setDictionary = (IDictionary)SubListType.InvokeMember("", BindingFlags.CreateInstance, null, null, null);

                }
                else
                {
                    itemType = SubListType.GetGenericArguments()[0];
                    setList = (IList)SubListType.InvokeMember("", BindingFlags.CreateInstance, null, null, null);
                    //info.SetValue(setOnObj, setList, null);
                }
            }
            int j = 0;
            object currentKey = null;
            while (reader.Read())
            {

                if (reader.Name == nodeName && !reader.IsStartElement())
                    break;
                else if (reader.Name == "Key" && setDictionary != null)
                {
                    if (keyType.GetInterfaces().Any(t => t == typeof(IDataObject)))
                    {
                        string innerXml = reader.ReadInnerXml();
                        Type openGeneric = typeof(Xml<>);
                        // Make a type for a specific value of T
                        Type closedGeneric = openGeneric.MakeGenericType(itemType);
                        MethodInfo method = closedGeneric.GetMethod("LoadOneFromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
                        // Invoke the static method
                        currentKey = method.Invoke(null, new object[] { innerXml });
                    }
                    else
                    {
                        if (keyType.IsEnum)
                        {
                            currentKey = Enum.Parse(keyType, (string)reader.ReadElementContentAsObject(), false);
                        }
                        else
                        {
                            currentKey = Convert.ChangeType(reader.ReadElementContentAsObject(), keyType);
                        }
                    }
                }
                else if (reader.Name == "ItemByteArray" && reader.IsStartElement())
                {
                    string b64ByteArray = reader.ReadString();
                    byte[] bytes = Convert.FromBase64String(b64ByteArray);
                    if (setArray != null && setArray is byte[])
                    {
                        setArray = bytes;
                    }
                    else if (setList != null && setList is IList<byte>)
                    {
                        foreach (byte b in bytes)
                            ((IList<byte>)setList).Add(b);
                    }
                }
                else if (reader.Name == "Item" && reader.IsStartElement())
                {
                    if (itemType.GetInterfaces().Any(t => t == typeof(IDataObject)))
                    {
                        string innerXml = reader.ReadInnerXml();
                        Type openGeneric = typeof(Xml<>);
                        // Make a type for a specific value of T
                        Type closedGeneric = openGeneric.MakeGenericType(itemType);
                        MethodInfo method = closedGeneric.GetMethod("LoadOneFromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
                        // Invoke the static method
                        object value = method.Invoke(null, new object[] { innerXml });
                        if (setArray != null)
                        {
                            setArray.SetValue(Convert.ChangeType(value, itemType), j);
                        }
                        else if (setDictionary != null)
                        {
                            setDictionary.Add(currentKey, Convert.ChangeType(value, itemType));
                        }
                        else
                        {
                            ((IList)setList).Add(Convert.ChangeType(value, itemType));
                        }
                    }
                    else if (itemType.IsArray || itemType.GetInterfaces().Any(t => t == typeof(IList)))
                    {
                        
                        reader.Read();
                        while (reader.Name == "")
                            reader.Read();
                        object result = ReadXmlList(reader, returnObj, true, reader.Name, itemType);
                        if (result != null)
                            setDictionary.Add(currentKey, result);
                    }
                    else
                    {
                        object value = reader.ReadElementContentAsObject();
                        if (setArray != null)
                        {
                            setArray.SetValue(Convert.ChangeType(value, itemType), j);
                        }
                        else if (setDictionary != null)
                        {
                            setDictionary.Add(currentKey, Convert.ChangeType(value, itemType));
                        }
                        else
                        {
                            ((IList)setList).Add(Convert.ChangeType(value, itemType));
                        }
                    }
                    j++;
                }


            }
            if (!IsSubList)
            {
                if (setArray != null)
                {
                    info.SetValue(setOnObj, setArray, null);
                    return setArray;
                }
                else if (setDictionary != null)
                {
                    info.SetValue(setOnObj, setDictionary, null);
                    return setDictionary;
                }
                else
                {
                    info.SetValue(setOnObj, setList, null);
                    return setList;
                }
            }
            else
            {
                if (setArray != null)
                {
                    return setArray;
                }
                else if (setDictionary != null)
                {
                    return setDictionary;
                }
                else
                {
                    return setList;
                }
            }
            

        }
    }
}
