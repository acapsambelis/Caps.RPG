using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SNS.Data.DataSerializer
{
    //Used for only XML Serialization, there are cases where we dont want a sub object to be included in a data set but
    //we need them to be included in XML output
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.ReturnValue), Serializable]
    public class XmlSubDataObject : Attribute
    {
        string fieldsPrefix;
        bool useSubKeys;

        public bool UseSubKeys
        {
            get { return useSubKeys; }
            set { useSubKeys = value; }
        }
        public string FieldPrefix
        {
            get { return fieldsPrefix; }
            set { fieldsPrefix = value; }
        }
        public XmlSubDataObject(string FieldPrefix)
        {
            fieldsPrefix = FieldPrefix;
            useSubKeys = true;
        }
        public XmlSubDataObject(string FieldPrefix, bool UseSubKeys)
        {
            fieldsPrefix = FieldPrefix;
            useSubKeys = UseSubKeys;
        }
        public XmlSubDataObject(bool UseSubKeys)
        {
            fieldsPrefix = "";
            useSubKeys = UseSubKeys;
        }
        public XmlSubDataObject()
        {
            fieldsPrefix = "";
            useSubKeys = true;
        }
    }
}
