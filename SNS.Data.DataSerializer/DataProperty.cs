using System;
using System.Collections.Generic;
using System.Text;

namespace SNS.Data.DataSerializer
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.ReturnValue), Serializable]
    public class DataProperty : Attribute
    {
        int dbTypeCode;
        bool key;

        bool insert;
        bool update;
        bool select;

        string getColumn;
        string setColumn;

        public int? RequiresDatabaseVersion { get; set; }

        public int DbTypeCode
        {
            get { return dbTypeCode; }
            set { dbTypeCode = value; }
        }
        public bool Key
        {
            get { return key; }
            set { key = value; }
        }
        public string GetColumn
        {
            get
            {
                return getColumn;
            }
            set
            {
                getColumn = value;
            }
        }
        public string SetColumn
        {
            get
            {
                return setColumn;
            }
            set
            {
                setColumn = value;
            }
        }

        public bool Update
        {
            get { return update; }
            set { update = value; }
        }

        public bool Insert
        {
            get { return insert; }
            set { insert = value; }
        }

        public bool Select
        {
            get { return select; }
            set { select = value; }
        }
        public bool MeetsDatabaseVersion(int? DatabaseVersion)
        {
            if (RequiresDatabaseVersion == null)
                return true;
            if (DatabaseVersion == null)
                return true;
            if (DatabaseVersion.Value >= RequiresDatabaseVersion.Value)
                return true;

            return false;
        }

        public DataProperty(string GetColumn, string SetColumn)
            : this(GetColumn, SetColumn, true, true, true)
        { }

        public DataProperty(string GetColumn, string SetColumn, bool Update)
            : this(GetColumn, SetColumn, Update, true)
        { }

        public DataProperty(string GetColumn, string SetColumn, bool Update, bool Insert)
            : this(GetColumn, SetColumn, Update, Insert, true)
        {

        }
        public DataProperty(string GetColumn, string SetColumn, bool Update, bool Insert, bool Select)
        {
            setColumn = SetColumn;
            getColumn = GetColumn;
            update = Update;
            insert = Insert;
            select = Select;
            key = false;
        }
        public DataProperty(string ColumnName, bool Update, bool Insert, bool Select, bool Key)
            : this(ColumnName, ColumnName, Update, Insert, Select)
        {
            key = Key;
        }
        public DataProperty(string ColumnName, bool Update, bool Insert, bool Key) :
            this(ColumnName, ColumnName, Update, Insert)
        {
            key = Key;
        }
        public DataProperty(string ColumnName, bool Update, bool Key) :
            this(ColumnName, ColumnName, Update)
        {
            key = Key;
        }

        public DataProperty(string ColumnName, int RequiresVersion) :
          this(ColumnName, ColumnName)
        {
            this.RequiresDatabaseVersion = RequiresVersion;
        }
        public DataProperty(string ColumnName) :
            this(ColumnName, ColumnName)
        {
        }
    }
}
