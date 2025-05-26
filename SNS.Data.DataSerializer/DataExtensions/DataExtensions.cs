using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Linq.Expressions;
using System.Collections;
using System.Threading;

namespace SNS.Data.DataSerializer.DataExtensions
{
    public static class DataObjectExtensions
    {

        internal static IDataConnector GetDefaultConnector<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            return Settings.GetConnector(ReflectionCache<T>.ConnectorName);
        }
        public static object GetDataPropertyUnpreparedValue<T>(this IGenericDataObject<T> Object, DataPropertyInfo Info) where T : IGenericDataObject<T>, new()
        {
            object value = null;
            if (!Info.IsSubObject)
            {
                value = Info.PropertyInfo.GetValue(Object, null);
            }
            else
            {
                string[] pathParts = Info.SubObjectPathSplit;//Info.SubObjectPath.Split("|".ToCharArray());
                object o = ReflectionCache<T>.GetSubObjectValue(Object, pathParts, 0, false);
                if (o == null)
                    value = null;
                else
                    value = Info.PropertyInfo.GetValue(o, null);
            }
            
            return value;
        }
        public static object GetDataPropertyValue<T>(this IGenericDataObject<T> Object, IDataConnector Connector, DataPropertyInfo Info) where T : IGenericDataObject<T>, new()
        {
            object value = null;
            if (!Info.IsSubObject)
            {
                value = Info.PropertyInfo.GetValue(Object, null);
            }
            else
            {
                string[] pathParts = Info.SubObjectPathSplit;//Info.SubObjectPath.Split("|".ToCharArray());
                object o = ReflectionCache<T>.GetSubObjectValue(Object, pathParts, 0, false);
                if (o == null)
                    value = null;
                else
                    value = Info.PropertyInfo.GetValue(o, null);
            }
            if (value == null)
            {
                return DBNull.Value;
            }
            if (value is DateTime && ((DateTime)value) == DateTime.MinValue)
            {
                return Connector.Settings.MinimumDateTimeSupported;
            }

            return value;
        }
        public static void SetDataPropertyValue<T>(this IGenericDataObject<T> Object, DataPropertyInfo Info, object Value) where T : IGenericDataObject<T>, new()
        {
            //Type changeToType = null;

            if (!Info.IsSubObject)
            {
                if (!Info.ValueType.IsEnum)
                {
                    //changeToType = Info.PropertyInfo.PropertyType;
                    Value = Convert.ChangeType(Value, Info.ValueType);
                }
                else
                {
                    Value = Enum.Parse(Info.PropertyInfo.PropertyType, Value.ToString());
                }

                Info.PropertyInfo.SetValue(Object, Value, null);
            }
            else
            {
                string[] pathParts = Info.SubObjectPathSplit;//Info.SubObjectPath.Split("|".ToCharArray());
                object o = ReflectionCache<T>.GetSubObjectValue(Object, pathParts, 0, true);
                if (!Info.ValueType.IsEnum)
                {
                    //changeToType = Info.PropertyInfo.PropertyType;
                    Value = Convert.ChangeType(Value, Info.ValueType);
                }
                else
                {
                    Value = Enum.Parse(Info.PropertyInfo.PropertyType, Value.ToString());
                }


                //if (Info.PropertyInfo.PropertyType.IsGenericType && Info.PropertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                //{
                //    changeToType = Nullable.GetUnderlyingType(Info.PropertyInfo.PropertyType);
                //    Value = Convert.ChangeType(Value, changeToType);

                //}
                //else if (Info.PropertyInfo.PropertyType.IsEnum)
                //{
                //    Value = Enum.Parse(Info.PropertyInfo.PropertyType, Value.ToString());
                //}
                //else
                //{
                //    changeToType = Info.PropertyInfo.PropertyType;
                //    Value = Convert.ChangeType(Value, changeToType);

                //}

                Info.PropertyInfo.SetValue(o, Value, null);
            }

            //info.PropertyInfo.DeclaringType.InvokeMember(info.PropertyInfo.Name, BindingFlags.SetProperty, null, DataObject, new object[] { Value });
        }
        internal static long Create<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, bool RetrieveIdentity) where T : IGenericDataObject<T>, new()
        {
            long result = -1;

            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildInsertCommand<T>(Connection, Connector, Object, RetrieveIdentity))
                    {
                        if (RetrieveIdentity)
                            result = Convert.ToInt64(cmd.ExecuteScalar());
                        else
                            cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildInsertCommand<T>(Connection, Connector, Object, RetrieveIdentity))
                {
                    if (RetrieveIdentity)
                        result = Convert.ToInt64(cmd.ExecuteScalar());
                    else
                        cmd.ExecuteNonQuery();
                }
            }
            Object.WasLoaded = true;
            return result;
        }

        internal static long Create<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbTransaction Transaction, bool RetrieveIdentity) where T : IGenericDataObject<T>, new()
        {
            long result = -1;
            
            using (IDbCommand cmd = CommandUtility.BuildInsertCommand<T>(Transaction.Connection, Connector, Object, RetrieveIdentity))
            {
                cmd.Transaction = Transaction;
                if (RetrieveIdentity)
                    result = Convert.ToInt64(cmd.ExecuteScalar());
                else
                    cmd.ExecuteNonQuery();
            }

            Object.WasLoaded = true;
            return result;
        }

        internal static bool Load<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection) where T : IGenericDataObject<T>, new()
        {
            int loaded = 0;
            if (DisposeConnection)
            {
                using (Connection)
                {
                    DataReader<T> reader = new DataReader<T>();
                    using (IDbCommand cmd = CommandUtility.BuildSelectCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector),"" , 1))
                    {
                        loaded = reader.Populate((T)Object, cmd);
                        Connection.Close();
                    }
                }
            }
            else
            {
                DataReader<T> reader = new DataReader<T>();
                using (IDbCommand cmd = CommandUtility.BuildSelectCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector), null, 1))
                {
                    loaded = reader.Populate((T)Object, cmd);
                }
            }
            return loaded == 1;
        }
        internal static void Create<T>(this IEnumerable<T> Objects, IDataConnector Connector, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            using (Connection)
            {
                foreach (IGenericDataObject<T> obj in Objects)
                {
                    bool retrieveIdentity = obj is IIDDataObject || obj is ILongIDDataObject;
                    using (IDbCommand cmd = CommandUtility.BuildInsertCommand<T>(Connection, Connector, obj, retrieveIdentity))
                    {
                        if (retrieveIdentity)
                        {
                            if (obj is IIDDataObject)
                            {
                                IIDDataObject idObj = (IIDDataObject)obj;
                                idObj.ID = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                            else
                            {
                                ILongIDDataObject idObj = (ILongIDDataObject)obj;
                                idObj.ID = Convert.ToInt64(cmd.ExecuteScalar());
                            }
                        }
                        else
                            cmd.ExecuteNonQuery();

                        obj.WasLoaded = true;
                    }
                }
            }
        }
        internal static void Update<T>(this IEnumerable<T> Objects, IDataConnector Connector, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            using (Connection)
            {
                foreach (IGenericDataObject<T> obj in Objects)
                {
                    using (IDbCommand cmd = CommandUtility.BuildUpdateCommand<T>(Connection, Connector, obj, CommandUtility.BuildWhereClauseFromKeys<T>(obj, Connector)))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            
        }
      
        internal static void Update<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection) where T : IGenericDataObject<T>, new()
        {
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildUpdateCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildUpdateCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void Update<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            using (IDbCommand cmd = CommandUtility.BuildUpdateCommand<T>(Transaction.Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
            {
                cmd.Transaction = Transaction;
                cmd.ExecuteNonQuery();
            }
        }
        internal static void Delete<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection) where T : IGenericDataObject<T>, new()
        {
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildDeleteCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildDeleteCommand<T>(Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static void Delete<T>(this IGenericDataObject<T> Object, IDataConnector Connector, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {

            using (IDbCommand cmd = CommandUtility.BuildDeleteCommand<T>(Transaction.Connection, Connector, Object, CommandUtility.BuildWhereClauseFromKeys<T>(Object, Connector)))
            {
                cmd.Transaction = Transaction;
                cmd.ExecuteNonQuery();
            }

        }
        internal static string Table<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            return ReflectionCache<T>.Table;
        }
        internal static string Schema<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            return ReflectionCache<T>.Schema;
        }
        internal static string Table(this IDataObject Object)
        {
            return (string)typeof(ReflectionCache<>).MakeGenericType(Object.GetType()).GetMethod("Table").Invoke(null, new object[] { Object });
        }
        internal static string Schema(this IDataObject Object)
        {
            return (string)typeof(ReflectionCache<>).MakeGenericType(Object.GetType()).GetMethod("Schema").Invoke(null, new object[] { Object });
        }

        public static SaveMode DetermineSaveMode<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            if (Object.WasLoaded)
                return SaveMode.Update;
            else
                return SaveMode.Create;
        }
        public static SaveMode DetermineSaveMode<T>(this IIDDataObject<T> Object) where T : IIDDataObject<T>, new()
        {
            if (Object.ID == 0)
                return SaveMode.Create;
            else
                return SaveMode.Update;

        }
        public static SaveMode DetermineSaveMode<T>(this ILongIDDataObject<T> Object) where T : ILongIDDataObject<T>, new()
        {
            if (Object.ID == 0)
                return SaveMode.Create;
            else
                return SaveMode.Update;

        }

        public static IDbConnection GetDefaultConnectionInstance<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            return Settings.GetOpenConnection(ReflectionCache<T>.ConnectorName);
        }

        public static void Create<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, false);
        }
        public static void Create<T>(this IGenericDataObject<T> Object, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, false);
        }
        public static void Create<T>(this IGenericDataObject<T> Object, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Object.GetDefaultConnector(), Connection, false, false);
        }
        public static void Create<T>(this IGenericDataObject<T> Object, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Object.GetDefaultConnector(), Transaction, false);
        }
        public static void Create<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Settings.GetConnector(ConnectorName), Connection, false, false);
        }
        public static void Create<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Create(Settings.GetConnector(ConnectorName), Transaction, false);
        }

        public static int Create<T>(this IIDDataObject<T> Object) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, true);
            return Object.ID;
        }
        public static int Create<T>(this IIDDataObject<T> Object, string ConnectorName) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, true);
            return Object.ID;
        }
        public static int Create<T>(this IIDDataObject<T> Object, IDbConnection Connection) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Connection, false, true);
            return Object.ID;
        }
        public static int Create<T>(this IIDDataObject<T> Object, IDbTransaction Transaction) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Transaction, true);
            return Object.ID;
        }
        public static int Create<T>(this IIDDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Connection, false, true);
            return Object.ID;
        }
        public static int Create<T>(this IIDDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IIDDataObject<T>, new()
        {
            Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Transaction, true);
            return Object.ID;
        }

        public static long Create<T>(this ILongIDDataObject<T> Object) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, true);
            return Object.ID;
        }
        public static long Create<T>(this ILongIDDataObject<T> Object, string ConnectorName) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, true);
            return Object.ID;
        }
        public static long Create<T>(this ILongIDDataObject<T> Object, IDbConnection Connection) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Object.GetDefaultConnector(), Connection, false, true);
            return Object.ID;
        }
        public static long Create<T>(this ILongIDDataObject<T> Object, IDbTransaction Transaction) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Object.GetDefaultConnector(), Transaction, true);
            return Object.ID;
        }
        public static long Create<T>(this ILongIDDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Connection, false, true);
            return Object.ID;
        }
        public static long Create<T>(this ILongIDDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : ILongIDDataObject<T>, new()
        {
            Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Transaction, true);
            return Object.ID;
        }
        
        public static bool Load<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static bool Load<T>(this IGenericDataObject<T> Object, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        public static bool Load<T>(this IGenericDataObject<T> Object, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Object.GetDefaultConnector(), Connection, false);
        }
        public static bool Load<T>(this IGenericDataObject<T> Object, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Object.GetDefaultConnector(), Transaction.Connection, false);
        }
        public static bool Load<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static bool Load<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            return Object.Load(Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        
        
        public static void Save<T>(this IEnumerable<IGenericDataObject<T>> Objects) where T : IGenericDataObject<T>, new()
        {
            if (Objects.Count() > 0)
            {
                using (IDbConnection connection = Objects.First().GetDefaultConnectionInstance())
                {
                    foreach (IGenericDataObject<T> obj in Objects)
                    {
                        obj.Save(connection);
                    }
                }
            }
        }
        public static void Save<T>(this IEnumerable<IIDDataObject<T>> Objects) where T : IIDDataObject<T>, new()
        {
            if (Objects.Count() > 0)
            {
                using (IDbConnection connection = Objects.First().GetDefaultConnectionInstance())
                {
                    foreach (IIDDataObject<T> obj in Objects)
                    {
                        obj.Save(connection);
                    }
                }
            }
        }
        public static void Update<T>(this IEnumerable<T> Objects) where T : IGenericDataObject<T>, new()
        {
            
            if (Objects.Count() > 0)
                Objects.Update(Objects.First().GetDefaultConnector(), Objects.First().GetDefaultConnectionInstance());
        }
        public static void Create<T>(this IEnumerable<T> Objects) where T : IGenericDataObject<T>, new()
        {
            if (Objects.Count() > 0)
                Objects.Create(Objects.First().GetDefaultConnector(), Objects.First().GetDefaultConnectionInstance());
        }
        public static void Save<T>(this IEnumerable<IGenericDataObject<T>> Objects, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            
            if (Objects.Count() > 0)
            {
                using (IDbConnection connection = Settings.GetOpenConnection(ConnectorName))
                {
                    using (var trans = connection.BeginTransaction())
                    {
                        foreach (IGenericDataObject<T> obj in Objects)
                        {
                            obj.Save(ConnectorName, trans);
                        }
                        trans.Commit();
                    }
                }
            }
        }
        public static void Save<T>(this IEnumerable<IIDDataObject<T>> Objects, string ConnectorName, int ThreadedConnections = 1) where T : IIDDataObject<T>, new()
        {
            if (Objects.Count() > 0)
            {
                //if (ThreadedConnections <= 1)
                //{
                using (IDbConnection connection = Settings.GetOpenConnection(ConnectorName))
                {
                    using (var trans = connection.BeginTransaction())
                    {
                        foreach (IIDDataObject<T> obj in Objects)
                        {
                            obj.Save(ConnectorName, trans);
                        }
                        trans.Commit();
                    }
                }
               
                //}
                //else
                //{
                //    IIDDataObject<T>[] objArray = Objects.ToArray();
                //    int processNum = Objects.Count() / ThreadedConnections;
                //    ManualResetEvent[] mres = new ManualResetEvent[ThreadedConnections];
                //    for (int i = 0; i < ThreadedConnections; i++)
                //    {
                //        mres[i] = new ManualResetEvent(false);

                //        ThreadPool.QueueUserWorkItem((object obj) =>
                //        {
                //            int thrNum = (int)obj;
                //            bool isLast = thrNum == (ThreadedConnections - 1);
                //            int startNum = processNum * thrNum;
                //            int endNum = isLast? objArray.Length: processNum * (thrNum + 1);

                //            using (IDbConnection connection = Settings.GetOpenConnection(ConnectorName))
                //            {
                //                for (int j = startNum; j < endNum; j++)
                //                {
                //                    objArray[j].Save(ConnectorName,connection);
                //                }
                //            }
                //            mres[thrNum].Set();
                //        }, i);
                //    }
                //    for(int i = 0; i < mres.Length; i++)
                //    {
                //        mres[i].WaitOne();
                //    }

                //}
            }

        }
        public static void Update<T>(this IEnumerable<T> Objects, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            if (Objects.Count() > 0)
                Objects.Update(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName));
        }
        public static void Create<T>(this IEnumerable<T> Objects, string ConnectorName) where T : IIDDataObject<T>, new()
        {
            if (Objects.Count() > 0)
                Objects.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName));
        }
     
        public static void Update<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static void Update<T>(this IGenericDataObject<T> Object, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        public static void Update<T>(this IGenericDataObject<T> Object, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Object.GetDefaultConnector(), Connection, false);
        }
        public static void Update<T>(this IGenericDataObject<T> Object, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Object.GetDefaultConnector(), Transaction);
        }
        public static void Update<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Update<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Update(Settings.GetConnector(ConnectorName), Transaction);
        }

        public static void Delete<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static void Delete<T>(this IGenericDataObject<T> Object, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        public static void Delete<T>(this IGenericDataObject<T> Object, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Object.GetDefaultConnector(), Connection, false);
        }
        public static void Delete<T>(this IGenericDataObject<T> Object, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Object.GetDefaultConnector(), Transaction);
        }
        public static void Delete<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Delete<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            Object.Delete(Settings.GetConnector(ConnectorName), Transaction);
        }

        public static void Save<T>(this IGenericDataObject<T> Object) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, false);
            else
                Object.Update(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static void Save<T>(this IGenericDataObject<T> Object, string ConnectorName) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, false);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);

        }
        public static void Save<T>(this IGenericDataObject<T> Object, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Object.GetDefaultConnector(), Connection, false, false);
            else
                Object.Update(Object.GetDefaultConnector(), Connection, false);
        }
        public static void Save<T>(this IGenericDataObject<T> Object, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Object.GetDefaultConnector(), Transaction, false);
            else
                Object.Update(Object.GetDefaultConnector(), Transaction);
        }
        public static void Save<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Settings.GetConnector(ConnectorName), Connection, false, false);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Save<T>(this IGenericDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IGenericDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.Create(Settings.GetConnector(ConnectorName), Transaction, false);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Transaction);

        }

        public static void Save<T>(this IIDDataObject<T> Object) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, true);
            else
                Object.Update(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static void Save<T>(this IIDDataObject<T> Object, string ConnectorName) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);

        }
        public static void Save<T>(this IIDDataObject<T> Object, IDbConnection Connection) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Connection, false, true);
            else
                Object.Update(Object.GetDefaultConnector(), Connection, false);
        }
        public static void Save<T>(this IIDDataObject<T> Object, IDbTransaction Transaction) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Object.GetDefaultConnector(), Transaction, true);
            else
                Object.Update(Object.GetDefaultConnector(), Transaction);
        }
        public static void Save<T>(this IIDDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Connection, false, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Save<T>(this IIDDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : IIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = (int)Object.Create(Settings.GetConnector(ConnectorName), Transaction, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Transaction);

        }

        public static void Save<T>(this ILongIDDataObject<T> Object) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true, true);
            else
                Object.Update(Object.GetDefaultConnector(), Object.GetDefaultConnectionInstance(), true);
        }
        public static void Save<T>(this ILongIDDataObject<T> Object, string ConnectorName) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);

        }
        public static void Save<T>(this ILongIDDataObject<T> Object, IDbConnection Connection) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Object.GetDefaultConnector(), Connection, false, true);
            else
                Object.Update(Object.GetDefaultConnector(), Connection, false);
        }
        public static void Save<T>(this ILongIDDataObject<T> Object, IDbTransaction Transaction) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Object.GetDefaultConnector(), Transaction, true);
            else
                Object.Update(Object.GetDefaultConnector(), Transaction);
        }
        public static void Save<T>(this ILongIDDataObject<T> Object, string ConnectorName, IDbConnection Connection) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Connection, false, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Save<T>(this ILongIDDataObject<T> Object, string ConnectorName, IDbTransaction Transaction) where T : ILongIDDataObject<T>, new()
        {
            if (Object.DetermineSaveMode() == SaveMode.Create)
                Object.ID = Object.Create(Settings.GetConnector(ConnectorName), Transaction, true);
            else
                Object.Update(Settings.GetConnector(ConnectorName), Transaction);

        }
        public static R[] Related<R>(this IDataObject Object, Expression<Func<R, bool>> Exp, params Expression<Func<R, object>>[] OrderByExpressions)
          where R : IGenericDataObject<R>, new()
        {
            return Data<R>.LoadMany(Exp, OrderByExpressions);
        }
        public static R[] Related<R>(this IIDDataObject Object, Expression<Func<R, int>> Exp, params Expression<Func<R, object>>[] OrderByExpressions)
            where R : IGenericDataObject<R>, new()
        {
            MemberExpression mEx2 = (MemberExpression)Exp.Body;
            MemberExpression mEx1 = MemberExpression.MakeMemberAccess(Expression.Constant(Object), Object.GetType().GetProperty("ID"));
            BinaryExpression bExp = Expression.Equal(mEx1, mEx2);
            return Data<R>.LoadMany(bExp, OrderByExpressions);
        }
        public static R[] Related<R>(this ILongIDDataObject Object, Expression<Func<R, long>> Exp, params Expression<Func<R, object>>[] OrderByExpressions)
            where R : IGenericDataObject<R>, new()
        {
            MemberExpression mEx2 = (MemberExpression)Exp.Body;
            MemberExpression mEx1 = MemberExpression.MakeMemberAccess(Expression.Constant(Object), Object.GetType().GetProperty("ID"));
            BinaryExpression bExp = Expression.Equal(mEx1, mEx2 );
            return Data<R>.LoadMany(bExp, OrderByExpressions);
        }



        public static object DBSet(this object Object, object Value)
        {
            throw new ApplicationException("This function is only intended to be referenced in an Data.Update lambda expression.");
           //Virtual Set used in db functions      
            //return Value;
        }
        //public static bool DBIn<T>(this T Object, params T[] InSet )
        //{
        //    throw new ApplicationException("This function is only intended to be referenced in a Where lambda expression.");
        //    //Virtual Set used in db functions      
        //    //return Value;
        //}
        public static bool DBIn<T>(this T Object, IEnumerable InSet, bool UseParameters = true)
        {
            throw new ApplicationException("This function is only intended to be referenced in a Where lambda expression.");
            //Virtual Set used in db functions      
            //return Value;
        }
        public static bool DBLike(this string Object, string LikePhrase)
        {
            throw new ApplicationException("This function is only intended to be referenced in a Where lambda expression.");
            //Virtual Set used in db functions      
            //return Value;
        }
        public static bool DBAsc(this object Object)
        {
            throw new ApplicationException("This function is only intended to be referenced in a Where lambda expression.");
            //Virtual Set used in db functions      
            //return Value;
        }
        public static bool DBDesc(this object Object)
        {
            throw new ApplicationException("This function is only intended to be referenced in a Where lambda expression.");
            //Virtual Set used in db functions      
            //return Value;
        }
    }
   
}
