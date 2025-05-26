using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SNS.Data.DataSerializer.DataExtensions;
using System.Web;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Xml;

namespace SNS.Data.DataSerializer.XmlExtensions
{
    public static class DataObjectExtensions
    {

        //public static string ToXml<T>(this IEnumerable<T> Objects) where T : IGenericDataObject<T>, new()
        //{
        //    return ToXml(Objects, new IDataRelation[0]);
        //}
        public static string ToXml<T>(this IEnumerable<T> Objects, bool UseBase64Arrays = true) where T : IGenericDataObject<T>, new()
        {
            int count = Objects.Count();
            string rootNodeName = ReflectionCache<T>.Table;
            StringBuilder sb = new StringBuilder("<" + rootNodeName + " Count=\"" + count + "\">\r\n");
            foreach (T O in Objects)
            {
                sb.Append(O.ToXml(UseBase64Arrays));
            }
            sb.Append("</" + rootNodeName + ">\r\n");
            return sb.ToString();
        }

        public static string ToXml(this IDataObject Object, bool UseBase64Arrays = true)
        {
            return ToXml(Object, UseBase64Arrays, new IDataRelation[0]);
        }
     
        public static string ToXml(this IDataObject Object, bool UseBase64Arrays, params IDataRelation[] WithRelationships)
        {
            CultureInfo InvC = new CultureInfo("");
            Type objectType = Object.GetType();
            string rootNodeName = objectType.Name;
            StringBuilder sb = new StringBuilder("<" + rootNodeName + ">\r\n");
            DataPropertyInfo[] props = ReflectionUtility.GetDataProperties(objectType);
            foreach (DataPropertyInfo prop in props)
            {
                if (prop.PropertyAttribute != null)
                {
                    string useName = prop.PropertyAttribute.GetColumn; //prop.PropertyInfo.Name;

                    object sourceO = Object;
                    if (prop.IsSubObject)
                    {
                        sourceO = ReflectionUtility.GetSubObjectValue(objectType, Object, prop.SubObjectPath.Split("|".ToCharArray()), 0, false);
                    }
                    object value = null;
                    if (sourceO != null)
                        value = prop.PropertyInfo.GetValue(sourceO, null);
                    if (value != null)
                    {
                        if (prop.IsSubObject)
                        {
                            if (value is IList || value is Array)
                            {
                                int count = value is Array ? ((Array)value).Length : ((IList)value).Count;
                                sb.Append("<" + prop.SubObjectPath.Replace("|", ".") + "." + useName + " ItemCount=\"" + count.ToString() + "\"> ");
                                if (UseBase64Arrays && value is byte[])
                                {
                                    sb.Append("<ItemByteArray>");
                                    sb.Append(HttpUtility.HtmlEncode(Convert.ToBase64String(value as byte[])));
                                    sb.Append("</ItemByteArray>");
                                }
                                else if (UseBase64Arrays && value is IList<byte>)
                                {
                                    sb.Append("<ItemByteArray>");
                                    sb.Append(HttpUtility.HtmlEncode(Convert.ToBase64String(((IList<byte>)value).ToArray())));
                                    sb.Append("</ItemByteArray>");
                                }
                                else
                                {
                                    foreach (object v in (IEnumerable)value)
                                    {
                                        sb.Append("<Item>");
                                        if (v is IFormattable)
                                            sb.Append(HttpUtility.HtmlEncode(((IFormattable)v).ToString("", InvC)));
                                        else if (v is IDataObject)
                                        {
                                            sb.Append(ToXml(v as IDataObject));
                                        }
                                        else
                                            sb.Append(HttpUtility.HtmlEncode(v.ToString()));
                                        sb.Append("</Item>\r\n");
                                    }
                                }
                                sb.Append("</" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">\r\n");
                            }
                            else if (value is IDictionary)
                            {
                                int count = ((IDictionary)value).Count;
                                sb.Append("<" + prop.SubObjectPath.Replace("|", ".") + "." + useName + " ItemCount=\"" + count.ToString() + "\"" + " Dictionary=\"True\"> ");
                                foreach (object key in ((IDictionary)value).Keys)
                                {
                                    object v = ((IDictionary)value)[key];

                                    sb.Append("<Key>");
                                    if (key is IFormattable)
                                        sb.Append(HttpUtility.HtmlEncode(((IFormattable)key).ToString("", InvC)));
                                    else if (key is IDataObject)
                                    {
                                        sb.Append(ToXml(key as IDataObject));
                                    }
                                    else
                                        sb.Append(HttpUtility.HtmlEncode(key.ToString()));
                                    sb.Append("</Key>\r\n");


                                    sb.Append("<Item>");
                                    if (v is IFormattable)
                                        sb.Append(HttpUtility.HtmlEncode(((IFormattable)v).ToString("", InvC)));
                                    else if (v is IList || v is Array)
                                    {
                                        int subcount = v is Array ? ((Array)v).Length : ((IList)v).Count;
                                        sb.Append("<List ItemCount=\"" + subcount.ToString() + "\">\r\n");
                                        foreach (object sv in (IEnumerable)v)
                                        {
                                            sb.Append("<Item>");
                                            if (sv is IFormattable)
                                                sb.Append(HttpUtility.HtmlEncode(((IFormattable)sv).ToString("", InvC)));
                                            else if (sv is IDataObject)
                                            {
                                                sb.Append(ToXml(sv as IDataObject));
                                            }
                                            else
                                                sb.Append(HttpUtility.HtmlEncode(sv.ToString()));
                                            sb.Append("</Item>\r\n");
                                        }
                                        sb.Append("</List>\r\n");
                                    }
                                    else if (v is IDataObject)
                                    {
                                        sb.Append(ToXml(v as IDataObject));
                                    }
                                    else
                                        sb.Append(HttpUtility.HtmlEncode(v.ToString()));
                                    sb.Append("</Item>\r\n");
                                }
                                sb.Append("</" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">\r\n");

                            }
                            else if (value is IFormattable)
                                sb.Append("<" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">" + HttpUtility.HtmlEncode(((IFormattable)value).ToString("", InvC)) + "</" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">\r\n");
                            else
                                sb.Append("<" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">" + HttpUtility.HtmlEncode(value.ToString()) + "</" + prop.SubObjectPath.Replace("|", ".") + "." + useName + ">\r\n");
                        }
                        else
                        {
                            if (value is IList || value is Array)
                            {
                                int count = value is Array? ((Array)value).Length: ((IList)value).Count;
                                sb.Append("<" + useName + " ItemCount=\"" + count.ToString() + "\">\r\n");
                                if (UseBase64Arrays && value is byte[])
                                {
                                    sb.Append("<ItemByteArray>");
                                    sb.Append(HttpUtility.HtmlEncode(Convert.ToBase64String(value as byte[])));
                                    sb.Append("</ItemByteArray>");
                                }
                                else if (UseBase64Arrays && value is IList<byte>)
                                {
                                    sb.Append("<ItemByteArray>");
                                    sb.Append(HttpUtility.HtmlEncode(Convert.ToBase64String(((IList<byte>)value).ToArray())));
                                    sb.Append("</ItemByteArray>");
                                }
                                else
                                {
                                    foreach (object v in (IEnumerable)value)
                                    {
                                        sb.Append("<Item>");
                                        if (v is IFormattable)
                                            sb.Append(HttpUtility.HtmlEncode(((IFormattable)v).ToString("", InvC)));
                                        else if (v is IDataObject)
                                        {
                                            sb.Append(ToXml(v as IDataObject));
                                        }
                                        else
                                            sb.Append(HttpUtility.HtmlEncode(v.ToString()));
                                        sb.Append("</Item>\r\n");
                                    }
                                }
                                sb.Append("</" + useName + ">\r\n");
                            }
                            else if (value is IDictionary)
                            {
                                int count = ((IDictionary)value).Count;
                                sb.Append("<" + useName + " ItemCount=\"" + count.ToString() + "\" Dictionary=\"True\" > ");
                                foreach (object key in ((IDictionary)value).Keys)
                                {
                                    object v = ((IDictionary)value)[key];

                                    sb.Append("<Key>");
                                    if (key is IFormattable)
                                        sb.Append(HttpUtility.HtmlEncode(((IFormattable)key).ToString("", InvC)));
                                    else if (key is IDataObject)
                                    {
                                        sb.Append(ToXml(key as IDataObject));
                                    }
                                    else
                                        sb.Append(HttpUtility.HtmlEncode(key.ToString()));
                                    sb.Append("</Key>\r\n");


                                    sb.Append("<Item>");
                                    if (v is IFormattable)
                                        sb.Append(HttpUtility.HtmlEncode(((IFormattable)v).ToString("", InvC)));
                                    else if (v is IList || v is Array)
                                    {
                                        int subcount = v is Array ? ((Array)v).Length : ((IList)v).Count;
                                        sb.Append("<List ItemCount=\"" + subcount.ToString() + "\">\r\n");
                                        foreach (object sv in (IEnumerable)v)
                                        {
                                            sb.Append("<Item>");
                                            if (sv is IFormattable)
                                                sb.Append(HttpUtility.HtmlEncode(((IFormattable)sv).ToString("", InvC)));
                                            else if (sv is IDataObject)
                                            {
                                                sb.Append(ToXml(sv as IDataObject));
                                            }
                                            else
                                                sb.Append(HttpUtility.HtmlEncode(sv.ToString()));
                                            sb.Append("</Item>\r\n");
                                        }
                                        sb.Append("</List>\r\n");
                                    }
                                    else if (v is IDataObject)
                                    {
                                        sb.Append(ToXml(v as IDataObject));
                                    }
                                    else
                                        sb.Append(HttpUtility.HtmlEncode(v.ToString()));
                                    sb.Append("</Item>\r\n");
                                }
                                sb.Append("</" + useName + ">\r\n");

                            }
                            else if (value is IFormattable)
                                sb.Append("<" + useName + ">" + HttpUtility.HtmlEncode(((IFormattable)value).ToString("", InvC)) + "</" + useName + ">\r\n");
                            else
                                sb.Append("<" + useName + ">" + HttpUtility.HtmlEncode(value.ToString()) + "</" + useName + ">\r\n");
                        }
                    }
                }

            }
            foreach (IDataRelation relationship in WithRelationships)
            {
                if (relationship.Type1 == Object.GetType())
                    sb.Append(relationship.ToXml(Object, UseBase64Arrays, WithRelationships));
            }
            sb.Append("</" + rootNodeName + ">\r\n");
            return sb.ToString();
        }
    }
}
