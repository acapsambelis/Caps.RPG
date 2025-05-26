using System;
using System.Collections.Generic;
using System.Text;

namespace SNS.Data.DataSerializer
{

    [AttributeUsage(AttributeTargets.Class), Serializable]
    public class DataClass : Attribute
    {
        string connectorName = "";
        string schema = "";
        string tableName = "";
        public string Schema
        {
            get { return schema; }
            set { schema = value; }
        }
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }
        public string ConnectorName
        {
            get { return connectorName; }
            set { connectorName = value; }
        }
        public DataClass()
         : this("")
        {
        }
        public DataClass(string TableName)
            : this("", "", TableName)
        {
        }
        public DataClass(string ConnectorName, string TableName): this(ConnectorName,"",TableName)
        {
        }
       
        public DataClass(string ConnectorName, string Schema, string TableName)
        {
            schema = Schema;
            tableName = TableName;
        }
    }
}
