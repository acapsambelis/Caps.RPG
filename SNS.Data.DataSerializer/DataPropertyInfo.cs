using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SNS.Data.DataSerializer
{
    public class DataPropertyInfo
    {
        PropertyInfo propertyInfo;
        DataProperty propertyAttribute;
        bool isSubObject;
        bool useSubKeys;
        string fieldPrefixPath = "";
        string subObjectPath = "";
        string[] subObjectPathSplit;
        Type valueType;

        public Type ValueType
        {
            get { return valueType; }
            set { valueType = value; }
        }
        
        public bool UseSubKeys
        {
            get { return useSubKeys; }
            set { useSubKeys = value; }
        }
        public string SubObjectPath
        {
            get { return subObjectPath; }
            set { subObjectPath = value; }
        }
        internal string[] SubObjectPathSplit
        {
            get
            {
                return subObjectPathSplit;
            }
        }
        public string FieldPrefixPath
        {
            get { return fieldPrefixPath; }
            set { fieldPrefixPath = value; }
        }
        public bool IsSubObject
        {
            get { return isSubObject; }
            set { isSubObject = value; }
        }
       
        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
            set { propertyInfo = value; }
        }
        public DataProperty PropertyAttribute
        {
            get { return propertyAttribute; }
            set { propertyAttribute = value; }
        }
        public DataPropertyInfo(PropertyInfo Property, DataProperty PropertyAttribute):this(Property,PropertyAttribute,false,"","", false)
        { }
        public DataPropertyInfo(PropertyInfo Property, DataProperty PropertyAttribute, bool IsSubObject, string FieldPrefixPath, string SubObjectPath, bool UseSubKeys)
        {
            propertyInfo = Property;
            propertyAttribute = PropertyAttribute;
            isSubObject = IsSubObject;
            fieldPrefixPath = FieldPrefixPath;
            subObjectPath = SubObjectPath;
            subObjectPathSplit = SubObjectPath.Split("|".ToCharArray());
            if (PropertyInfo != null)
            {
                if (PropertyInfo.PropertyType.IsGenericType && PropertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    valueType = Nullable.GetUnderlyingType(PropertyInfo.PropertyType);
                }
                else
                {
                    valueType = PropertyInfo.PropertyType;
                }
            }
        }
    }
}
