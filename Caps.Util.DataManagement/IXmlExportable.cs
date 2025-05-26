namespace Caps.Util.DataManagement
{
    public interface IXmlExportable
    {
    }

    public static class DataObjectExtensions
    {
        public static string ToXml(this IXmlExportable Object)
        {
            return XmlHelper.ToXml(Object).ToString();
        }

        public static T FromXml<T>(this T Subject, string xml) where T : IXmlExportable
        {
            return XmlHelper.FromXml<T>(xml);
        }
    }
}
