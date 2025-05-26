using System;

namespace Caps.Util.DataManagement
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class XmlIncludeAttribute : Attribute
    {
        public string ElementName { get; }

        public XmlIncludeAttribute(string elementName = null)
        {
            ElementName = elementName;
        }
    }
}
