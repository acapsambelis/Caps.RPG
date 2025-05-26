using System.Collections;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Caps.Util.DataManagement
{
    public static class XmlHelper
    {
        public static string ToXml(this object obj, string elementNameOverride = null, int indentLevel = 0)
        {
            if (obj == null)
                return Indent("<null />", indentLevel);

            Type type = obj.GetType();
            string elementName = elementNameOverride ?? type.Name;
            var sb = new StringBuilder();

            sb.AppendLine(Indent($"<{elementName}>", indentLevel));

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var member in members)
            {
                var attr = member.GetCustomAttribute<XmlIncludeAttribute>();
                if (attr == null) continue;

                string childName = attr.ElementName ?? member.Name;
                object value = null;

                switch (member)
                {
                    case PropertyInfo prop when prop.CanRead:
                        value = prop.GetValue(obj);
                        break;
                    case FieldInfo field:
                        value = field.GetValue(obj);
                        break;
                }

                if (value == null)
                {
                    sb.AppendLine(Indent($"  <{childName} />", indentLevel));
                }
                else if (value is string || value.GetType().IsPrimitive)
                {
                    sb.AppendLine(Indent($"  <{childName}>{value}</{childName}>", indentLevel));
                }
                else if (value is IEnumerable enumerable)
                {
                    sb.AppendLine(Indent($"  <{childName}>", indentLevel));
                    foreach (var item in enumerable)
                    {
                        sb.AppendLine(item.ToXml("Item", indentLevel + 2));
                    }
                    sb.AppendLine(Indent($"  </{childName}>", indentLevel));
                }
                else
                {
                    sb.AppendLine(value.ToXml(childName, indentLevel + 1));
                }
            }

            sb.AppendLine(Indent($"</{elementName}>", indentLevel));
            return sb.ToString();
        }

        private static string Indent(string text, int level)
        {
            return new string(' ', level * 2) + text;
        }

        public static T FromXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using StringReader reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }
    }
}
