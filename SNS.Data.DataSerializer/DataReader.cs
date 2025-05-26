using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using SNS.Data.DataSerializer.DataExtensions;

namespace SNS.Data.DataSerializer
{
    class DataReader<GettingType> where GettingType : IGenericDataObject<GettingType>, new() 
    {
        public DataReader()
        {
        }
      
        internal static bool ContainsColumn(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).ToLower() == columnName.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        public int Populate(List<GettingType> Objects, IDbCommand command, int Max)
        {           
            int objI = 0;
            using (command)
            {
                command.CommandTimeout = 600;
                using (IDataReader reader = command.ExecuteReader())
                {
                    DataPropertyInfo[] classInfo = ReflectionCache<GettingType>.GetDataProperties();
                    List<DataPropertyInfo> containedColumnInf = new List<DataPropertyInfo>();
                    while (reader.Read() && (objI < Max || Max < 0))
                    {
                        if (objI == 0)
                        {
                            foreach (DataPropertyInfo inf in classInfo)
                            {
                                if (inf.PropertyAttribute != null)
                                {
                                    if (ContainsColumn(reader, inf.FieldPrefixPath + inf.PropertyAttribute.GetColumn))
                                    {
                                        containedColumnInf.Add(inf);
                                    }
                                }
                            }
                        }
                        GettingType newObject = new GettingType();
                        foreach (DataPropertyInfo inf in containedColumnInf)
                        {
                            object value = reader[inf.FieldPrefixPath + inf.PropertyAttribute.GetColumn];
                            if (value != DBNull.Value)
                            {
                                newObject.SetDataPropertyValue(inf, value);
                            }
                        }
                        newObject.WasLoaded = true;
                        Objects.Add(newObject);
                        objI++;
                    }
                    reader.Close();
                }
            }
            return objI;
        }
        public int Populate(GettingType Object, IDbCommand command)
        {
            Type CreateType = typeof(GettingType);
            int retVal = 0;
            using (command)
            {
                command.CommandTimeout = 600;
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DataPropertyInfo[] classInfo = ReflectionCache<GettingType>.GetDataProperties();
                        foreach (DataPropertyInfo inf in classInfo)
                        {
                            DataProperty prop = inf.PropertyAttribute;
                            if (ContainsColumn(reader, inf.FieldPrefixPath + prop.GetColumn))
                            {
                                object value = reader[inf.FieldPrefixPath + prop.GetColumn];

                                if (value != DBNull.Value)
                                {
                                    Object.SetDataPropertyValue(inf, value);
                                }
                            }

                        }
                        Object.WasLoaded = true;
                        retVal = 1;
                    }
                    reader.Close();
                }
            }
            return retVal;
        }
    }
}
