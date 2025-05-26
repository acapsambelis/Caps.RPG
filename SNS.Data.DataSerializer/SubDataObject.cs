using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SNS.Data.DataSerializer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.ReturnValue), Serializable]
    public class SubDataObject : Attribute
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
        public SubDataObject(string FieldPrefix)
        {
            fieldsPrefix = FieldPrefix;
            useSubKeys = true;
        }
        public SubDataObject(string FieldPrefix, bool UseSubKeys)
        {
            fieldsPrefix = FieldPrefix;
            useSubKeys = UseSubKeys;
        }
        public SubDataObject(bool UseSubKeys)
        {
            fieldsPrefix = "";
            useSubKeys = UseSubKeys;
        }
        public SubDataObject()
        {
            fieldsPrefix = "";
            useSubKeys = true;
        }
    }
}
