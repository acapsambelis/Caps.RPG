using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using System.IO;

namespace SNS.Data.DataSerializer.DataExtensions
{
    public abstract class Data<T> where T : IGenericDataObject<T>, new()
    {
        internal static long CountBy(IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            long count = 0;
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildSelectCountCommand<T>(Connection, Connector, ""))
                    {
                        count = Convert.ToInt64(cmd.ExecuteScalar());
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildSelectCountCommand<T>(Connection, Connector,""))
                {
                    count = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
            return count;
        }
        internal static long CountBy(Expression<Func<T, bool>> Expression, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            long count = 0;
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildSelectCountCommandFromExpression<T>(Connection, Connector, Expression))
                    {
                        count = Convert.ToInt64(cmd.ExecuteScalar());
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildSelectCountCommandFromExpression<T>(Connection, Connector, Expression))
                {
                    count = Convert.ToInt64(cmd.ExecuteScalar());
                }
            }
            return count;
        }
        internal static T[] LoadBy(IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    DataReader<T> reader = new DataReader<T>();
                    using (IDbCommand cmd = CommandUtility.BuildSelectCommand<T>(Connection, Connector, Limit, OrderByExpressions))
                    {
                        reader.Populate(results, cmd, -1);
                        Connection.Close();
                    }
                }
            }
            else
            {
                DataReader<T> reader = new DataReader<T>();
                using (IDbCommand cmd = CommandUtility.BuildSelectCommand<T>(Connection, Connector, Limit, OrderByExpressions))
                {
                    reader.Populate(results, cmd, -1);
                }
            }
            return results.ToArray();
        }
        internal static T[] LoadBy(Expression Expression, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    DataReader<T> reader = new DataReader<T>();
                    using (IDbCommand cmd = CommandUtility.BuildSelectCommandFromExpression<T>(Connection, Connector, Expression, Limit, OrderByExpressions))
                    {
                        reader.Populate(results, cmd, -1);
                        Connection.Close();
                    }
                }
            }
            else
            {
                DataReader<T> reader = new DataReader<T>();
                using (IDbCommand cmd = CommandUtility.BuildSelectCommandFromExpression<T>(Connection, Connector, Expression, Limit, OrderByExpressions))
                {
                    reader.Populate(results,cmd,-1);
                }
            }
            return results.ToArray();
        }
        internal static T[] LoadBy(string ProcedureName, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    DataReader<T> reader = new DataReader<T>();
                    using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames,ParamValues))
                    {
                        reader.Populate(results, cmd, -1);
                        Connection.Close();
                    }
                }
            }
            else
            {
                DataReader<T> reader = new DataReader<T>();
                using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                {
                    reader.Populate(results, cmd, -1);
                }
            }
            return results.ToArray();
        }
     
        internal static T[] LoadBySQL(string SQL, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, int Limit)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    DataReader<T> reader = new DataReader<T>();
                    using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                    {
                        reader.Populate(results, cmd, Limit == 0? -1: Limit);
                        Connection.Close();
                    }
                }
            }
            else
            {
                DataReader<T> reader = new DataReader<T>();
                using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                {
                    reader.Populate(results, cmd, Limit == 0 ? -1 : Limit);
                }
            }
            return results.ToArray();
        }
        internal static void CallBy(string ProcedureName, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                    {
                        cmd.ExecuteNonQuery();
                        Connection.Close();
                       
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static void CallBySQL(string SQL, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                    {
                        cmd.ExecuteNonQuery();
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static ReturnType CallBySQL<ReturnType>(string SQL, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            object o = null;
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                    {
                        o = cmd.ExecuteScalar();
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.GetTextCommand(Connection, Connector, SQL, ParamNames, ParamValues))
                {
                    o = cmd.ExecuteScalar();
                }
            }
            if (o == DBNull.Value)
                return default(ReturnType);

            return (ReturnType)Convert.ChangeType(o, typeof(ReturnType));
        }
        internal static ReturnType CallScalarBy<ReturnType>(string ProcedureName, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            object o = null;
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                    {
                        o = cmd.ExecuteScalar();
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                {
                    o = cmd.ExecuteScalar();
                }
            }
            if (o == DBNull.Value)
                return default(ReturnType);

            return (ReturnType)Convert.ChangeType(o, typeof(ReturnType));
        }
        internal static void DeleteBy(Expression<Func<T, bool>> Expression, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, IDbTransaction Transaction = null)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildDeleteCommandFromExpression<T>(Connection, Connector, Expression))
                    {
                        cmd.ExecuteNonQuery(); 
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildDeleteCommandFromExpression<T>(Connection, Connector, Expression))
                {
                    if (Transaction != null)
                        cmd.Transaction = Transaction;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        internal static void UpdateBy(Expression<Func<T, bool>> Expression, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection, params Expression<Func<T, object>>[] SetExpressions)
        {
            List<T> results = new List<T>();
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.BuildUpdateCommandFromExpression<T>(Connection, Connector, Expression, SetExpressions))
                    {
                        cmd.ExecuteNonQuery();
                        Connection.Close();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.BuildUpdateCommandFromExpression<T>(Connection, Connector, Expression, SetExpressions))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
      
        #region Delete
        public static void Delete(Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        public static void Delete(string ConnectorName, Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        public static void Delete(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false);
        }
        public static void Delete(string ConnectorName,  IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        public static void Delete(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        public static void Delete(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            DeleteBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, Transaction);
        }
        #endregion
        #region Update
        public static void Update(Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, SetExpressions);
        }
        public static void Update(string ConnectorName, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, SetExpressions);
        }
        public static void Update(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false, SetExpressions);
        }
        public static void Update(string ConnectorName, IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false, SetExpressions);
        }
        public static void Update(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, SetExpressions);
        }
        public static void Update(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] SetExpressions)
        {
            if (SetExpressions.Length == 0)
                throw new ApplicationException("No Set Expressions specified.");
            UpdateBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, SetExpressions);
        }
        #endregion
        #region LoadSingle
        public static T Load(Expression<Func<T, bool>> Expression)
        {
            T[] results = LoadBy(Expression,ReflectionCache<T>.InstanceReference.GetDefaultConnector(),ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(),true,1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T Load(string ConnectorName, Expression<Func<T, bool>> WhereExpression)
        {
            T[] results = LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, 1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T Load(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            T[] results = LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false, 1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T Load(string ConnectorName, IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            T[] results = LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false, 1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T Load(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            T[] results = LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, 1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        public static T Load(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            T[] results = LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, 1);
            if (results.Length > 0)
                return results[0];
            else
                return default(T);
        }
        #endregion
        #region LoadSingleFromSQL
        public static T LoadFromSQLFile(string SQLFile, params object[] ParameterNamesThenValues)
        {
            return LoadFromSQL(File.ReadAllText(SQLFile), ParameterNamesThenValues);
        }
        public static T LoadFromSQL(string SQL, params object[] ParameterNamesThenValues )
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadFromSQL(SQL, names, vals);
        }
        public static T LoadFromSQL(string ConnectorName, string SQL, string[] ParameterNames, object[] ParameterValues)
        {
            if (ParameterNames.Length != ParameterValues.Length)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            return LoadFromSQLWithConnector(ConnectorName, SQL, ParameterNames, ParameterValues);
        }
        public static T LoadFromSQL(string ConnectorName, string SQL, IDbConnection Connection, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadFromSQLWithConnector(ConnectorName, Connection, SQL, names, vals);
        }
        public static T LoadFromSQL(string ConnectorName, IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadFromSQLWithConnector(ConnectorName, Transaction, SQL, names, vals);
        }
        public static T LoadFromSQL(IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadFromSQLWithConnector(Connection, SQL, names, vals);
        }
        public static T LoadFromSQL(IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadFromSQLWithConnector(Transaction, SQL, names, vals);
        }
        public static T LoadFromSQL(string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        internal static T LoadFromSQLWithConnector(string ConnectorName, string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        internal static T LoadFromSQLWithConnector(string ConnectorName, IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        internal static T LoadFromSQLWithConnector(string ConnectorName, IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        internal static T LoadFromSQLWithConnector(IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        internal static T LoadFromSQLWithConnector(IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            T[] results = LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, 1);
            if (results.Length > 0)
            {
                return results[0];
            }
            return default(T);
        }
        #endregion
        #region LoadManyFromSQL
        public static T[] LoadManyFromSQLFile(string SQLFile, params object[] ParameterNamesThenValues)
        {
            return LoadManyFromSQL(File.ReadAllText(SQLFile), ParameterNamesThenValues);
        }
        public static T[] LoadManyFromSQL(string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromSQL(SQL, names, vals);
        }
        public static T[] LoadManyFromSQL(string ConnectorName, string SQL, string[] ParameterNames, object[] ParameterValues)
        {
            if(ParameterNames.Length != ParameterValues.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            return LoadManyFromSQLWithConnector(ConnectorName, SQL, ParameterNames, ParameterValues);
        }
        public static T[] LoadManyFromSQL(string ConnectorName, IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromSQLWithConnector(ConnectorName, Connection, SQL, names, vals);
        }
        public static T[] LoadManyFromSQL(string ConnectorName, IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromSQLWithConnector(ConnectorName, Transaction, SQL, names, vals);
        }
        public static T[] LoadManyFromSQL(IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromSQLWithConnector(Connection, SQL, names, vals);
        }
        public static T[] LoadManyFromSQL(IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromSQLWithConnector(Transaction, SQL, names, vals);
        }
        public static T[] LoadManyFromSQL(string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, 0);
        }
        internal static T[] LoadManyFromSQLWithConnector(string ConnectorName, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, 0);
        }
        internal static T[] LoadManyFromSQLWithConnector(string ConnectorName, IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false, 0);
        }
        internal static T[] LoadManyFromSQLWithConnector(string ConnectorName, IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false, 0);
        }
        internal static T[] LoadManyFromSQLWithConnector(IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, 0);
        }
        internal static T[] LoadManyFromSQLWithConnector(IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return LoadBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, 0);
        }

        #endregion
        #region LoadMany
        public static T[] LoadMany(Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, 0, OrderByExpressions);
        }
        public static T[] LoadMany(string ConnectorName, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, 0);
        }
        public static T[] LoadMany(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false, 0);
        }
        public static T[] LoadMany(string ConnectorName, IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false, 0);
        }
        public static T[] LoadMany(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, 0);
        }
        public static T[] LoadMany(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, 0);
        }
        public static T[] LoadMany(Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, Limit, OrderByExpressions);
        }
        public static T[] LoadMany(string ConnectorName, Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, Limit, OrderByExpressions);
        }
        public static T[] LoadMany(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadMany(string ConnectorName, IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadMany(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadMany(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, Limit, OrderByExpressions);
        }
        internal static T[] LoadMany(BinaryExpression WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, 0, OrderByExpressions);
        }
#endregion
        #region LoadManyFromProc
        public static T[] LoadManyFromProc(string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromProc(ProcedureName, names, vals);
        }
        public static T[] LoadManyFromProc(string ConnectorName, string ProcedureName)
        {
            return LoadManyFromProcWithConnector(ConnectorName, ProcedureName, new string[0], new object[0]);
        }
        public static T[] LoadManyFromProc(string ConnectorName, string ProcedureName, string[] ParameterNames, object[] ParameterValues)
        {
            if (ParameterNames.Length != ParameterValues.Length)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }

            return LoadManyFromProcWithConnector(ConnectorName, ProcedureName, ParameterNames, ParameterValues);
        }
        public static T[] LoadManyFromProc(string ConnectorName, IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromProcWithConnector(ConnectorName, Connection, ProcedureName, names, vals);
        }
        public static T[] LoadManyFromProc(string ConnectorName, IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromProcWithConnector(ConnectorName, Transaction, ProcedureName, names, vals);
        }
        public static T[] LoadManyFromProc(IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromProcWithConnector(Connection, ProcedureName, names, vals);
        }
        public static T[] LoadManyFromProc(IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadManyFromProcWithConnector(Transaction, ProcedureName, names, vals);
        }
        public static T[] LoadManyFromProc(string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static T[] LoadManyFromProcWithConnector(string ConnectorName, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName),Settings.GetOpenConnection(ConnectorName), true);
        }
        internal static T[] LoadManyFromProcWithConnector(string ConnectorName, IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false);
        }
        internal static T[] LoadManyFromProcWithConnector(string ConnectorName, IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        internal static T[] LoadManyFromProcWithConnector(IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        internal static T[] LoadManyFromProcWithConnector(IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);
        }
        #endregion
        #region Call
        public static void Call(string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            Call(ProcedureName, names, vals);
        }
        public static void Call(string ConnectorName, string ProcedureName, string[] ParameterNames, object[] ParameterValues)
        {
            if (ParameterValues.Length != ParameterNames.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");

            CallWithConnector(ConnectorName, ProcedureName, ParameterNames, ParameterValues);
        }
        public static void Call(string ConnectorName, IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallWithConnector(ConnectorName, Connection, ProcedureName, names, vals);
        }
        public static void Call(string ConnectorName, IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallWithConnector(ConnectorName, Transaction, ProcedureName, names, vals);
        }
        public static void Call(IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallWithConnector(Connection, ProcedureName, names, vals);
        }
        public static void Call(IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallWithConnector(Transaction, ProcedureName, names, vals);
        }
        internal static void Call(string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static void CallWithConnector(string ConnectorName, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        internal static void CallWithConnector(string ConnectorName, IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false);
        }
        internal static void CallWithConnector(string ConnectorName, IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        internal static void CallWithConnector(IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        internal static void CallWithConnector(IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            CallBy(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);
        }
        #endregion
        #region CallSQL
        public static void CallSQLFile(string SQLFile, params object[] ParameterNamesThenValues)
        {
            CallSQL(File.ReadAllText(SQLFile), ParameterNamesThenValues);
        }
        public static void CallSQL(string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallSQL(SQL, names, vals);
        }
        public static void CallSQL(string ConnectorName, string SQL, string[] ParameterNames, object[] ParameterValues)
        {
            if (ParameterNames.Length != ParameterValues.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");

            CallSQLWithConnector(ConnectorName, SQL, ParameterNames, ParameterValues);
        }
        public static void CallSQL(string ConnectorName, IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallSQLWithConnector(ConnectorName, Connection, SQL, names, vals);
        }
        public static void CallSQL(string ConnectorName, IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallSQLWithConnector(ConnectorName, Transaction, SQL, names, vals);
        }
        public static void CallSQL(IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallSQLWithConnector(Connection, SQL, names, vals);
        }
        public static void CallSQL(IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            CallSQLWithConnector(Transaction, SQL, names, vals);
        }
        internal static void CallSQL(string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static void CallSQLWithConnector(string ConnectorName, string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        internal static void CallSQLWithConnector(string ConnectorName, IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false);
        }
        internal static void CallSQLWithConnector(string ConnectorName, IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        internal static void CallSQLWithConnector(IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        internal static void CallSQLWithConnector(IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            CallBySQL(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);
        }
        #endregion
        #region Call<Scalar>
        public static ReturnType Call<ReturnType>(string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return Call<ReturnType>(ProcedureName, names, vals);
        }
        public static ReturnType Call<ReturnType>(string ConnectorName, string ProcedureName, string[] ParameterNames, object[] ParameterValues)
        {
           if (ParameterNames.Length != ParameterValues.Length)
               throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
           return CallWithConnector<ReturnType>(ConnectorName, ProcedureName, ParameterNames, ParameterValues);
        }
        public static ReturnType Call<ReturnType>(string ConnectorName, IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallWithConnector<ReturnType>(ConnectorName, Connection, ProcedureName, names, vals);
        }
        public static ReturnType Call<ReturnType>(string ConnectorName, IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallWithConnector<ReturnType>(ConnectorName, Transaction, ProcedureName, names, vals);
        }
        public static ReturnType Call<ReturnType>(IDbConnection Connection, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallWithConnector<ReturnType>(Connection, ProcedureName, names, vals);
        }
        public static ReturnType Call<ReturnType>(IDbTransaction Transaction, string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallWithConnector<ReturnType>(Transaction, ProcedureName, names, vals);
        }
        public static ReturnType Call<ReturnType>(string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static ReturnType CallWithConnector<ReturnType>(string ConnectorName, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        internal static ReturnType CallWithConnector<ReturnType>(string ConnectorName, IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false);
        }
        internal static ReturnType CallWithConnector<ReturnType>(string ConnectorName, IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        internal static ReturnType CallWithConnector<ReturnType>(IDbConnection Connection, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        internal static ReturnType CallWithConnector<ReturnType>(IDbTransaction Transaction, string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return CallScalarBy<ReturnType>(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);
        }
        #endregion
        #region CallSQL<Scalar>
        public static ReturnType CallSQLFile<ReturnType>(string SQLFile, params object[] ParameterNamesThenValues)
        {
            return CallSQL<ReturnType>(File.ReadAllText(SQLFile), ParameterNamesThenValues);
        }
        public static ReturnType CallSQL<ReturnType>(string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallSQL<ReturnType>(SQL, names, vals);
        }
        public static ReturnType CallSQL<ReturnType>(string ConnectorName, string SQL, string[] ParameterNames, object[] ParameterValues)
        {
            if (ParameterNames.Length != ParameterValues.Length)
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            
            return CallSQLWithConnector<ReturnType>(ConnectorName, SQL, ParameterNames, ParameterValues);
        }
        public static ReturnType CallSQL<ReturnType>(string ConnectorName, IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallSQLWithConnector<ReturnType>(ConnectorName, Connection, SQL, names, vals);
        }
        public static ReturnType CallSQL<ReturnType>(string ConnectorName, IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallSQLWithConnector<ReturnType>(ConnectorName, Transaction, SQL, names, vals);
        }
        public static ReturnType CallSQL<ReturnType>(IDbConnection Connection, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallSQLWithConnector<ReturnType>(Connection, SQL, names, vals);
        }
        public static ReturnType CallSQL<ReturnType>(IDbTransaction Transaction, string SQL, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return CallSQLWithConnector<ReturnType>(Transaction, SQL, names, vals);
        }
        public static ReturnType CallSQL<ReturnType>(string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static ReturnType CallSQLWithConnector<ReturnType>(string ConnectorName, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);
        }
        internal static ReturnType CallSQLWithConnector<ReturnType>(string ConnectorName, IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Connection, false);
        }
        internal static ReturnType CallSQLWithConnector<ReturnType>(string ConnectorName, IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, Settings.GetConnector(ConnectorName), Transaction.Connection, false);
        }
        internal static ReturnType CallSQLWithConnector<ReturnType>(IDbConnection Connection, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);
        }
        internal static ReturnType CallSQLWithConnector<ReturnType>(IDbTransaction Transaction, string SQL, string[] ParamNames, object[] ParamValues)
        {
            return CallBySQL<ReturnType>(SQL, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);
        }
        #endregion
        #region LoadAll
        public static T[] LoadAll(params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, 0, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, 0, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, IDbConnection Connection, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Connection, false, 0, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, IDbTransaction Transaction, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Transaction.Connection, false, 0, OrderByExpressions);
        }
        public static T[] LoadAll(IDbConnection Connection, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, 0, OrderByExpressions);
        }
        public static T[] LoadAll(IDbTransaction Transaction, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, 0, OrderByExpressions);
        }

        public static T[] LoadAll(int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true, Limit, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true, Limit, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, IDbConnection Connection, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadAll(string ConnectorName, IDbTransaction Transaction, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(Settings.GetConnector(ConnectorName), Transaction.Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadAll(IDbConnection Connection, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false, Limit, OrderByExpressions);
        }
        public static T[] LoadAll(IDbTransaction Transaction, int Limit, params Expression<Func<T, object>>[] OrderByExpressions)
        {
            return LoadBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false, Limit, OrderByExpressions);
        }
        #endregion
        #region Count
        public static long Count(Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);

        }
        public static long Count(string ConnectorName, Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);

        }
        public static long Count(string ConnectorName, IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, Settings.GetConnector(ConnectorName), Connection, false);

        }
        public static long Count(string ConnectorName, IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, Settings.GetConnector(ConnectorName), Transaction.Connection, false);

        }
        public static long Count(IDbConnection Connection, Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);

        }
        public static long Count(IDbTransaction Transaction, Expression<Func<T, bool>> WhereExpression)
        {
            return CountBy(WhereExpression, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);

        }
        public static long Count()
        {
            return CountBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);

        }
        public static long Count(string ConnectorName)
        {
            return CountBy(Settings.GetConnector(ConnectorName), Settings.GetOpenConnection(ConnectorName), true);

        }
        public static long Count(string ConnectorName, IDbConnection Connection)
        {
            return CountBy(Settings.GetConnector(ConnectorName), Connection, false);

        }
        public static long Count(string ConnectorName, IDbTransaction Transaction)
        {
            return CountBy(Settings.GetConnector(ConnectorName), Transaction.Connection, false);

        }
        public static long Count(IDbConnection Connection)
        {
            return CountBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Connection, false);

        }
        public static long Count(IDbTransaction Transaction)
        {
            return CountBy(ReflectionCache<T>.InstanceReference.GetDefaultConnector(), Transaction.Connection, false);

        }
        #endregion

        #region LoadScalarListFromProc<ScalarType>
        public static ReturnType[] LoadScalarListFromProc<ReturnType> (string ProcedureName, params object[] ParameterNamesThenValues)
        {
            if (ParameterNamesThenValues.Length % 2 != 0)
            {
                throw new ApplicationException("Number of Parameter Names are Different From Number of Parameter Values");
            }
            int nameValLength = ParameterNamesThenValues.Length / 2;
            string[] names = new string[nameValLength];
            object[] vals = new object[nameValLength];
            for (int i = 0; i < nameValLength; i++)
            {
                names[i] = (string)ParameterNamesThenValues[i];
                vals[i] = ParameterNamesThenValues[i + ParameterNamesThenValues.Length / 2];
            }
            return LoadScalarListFromProc<ReturnType>(ProcedureName, names, vals);
        }
        public static ReturnType[] LoadScalarListFromProc<ReturnType>(string ProcedureName, string[] ParamNames, object[] ParamValues)
        {
            return LoadScalarListBy<ReturnType>(ProcedureName, ParamNames, ParamValues, ReflectionCache<T>.InstanceReference.GetDefaultConnector(), ReflectionCache<T>.InstanceReference.GetDefaultConnectionInstance(), true);
        }
        internal static ReturnType[] LoadScalarListBy<ReturnType>(string ProcedureName, string[] ParamNames, object[] ParamValues, IDataConnector Connector, IDbConnection Connection, bool DisposeConnection)
        {
            if (DisposeConnection)
            {
                using (Connection)
                {
                    using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                    {
                        List<ReturnType> data = new List<ReturnType>();

                        using (IDataReader reader = cmd.ExecuteReader())
                        {
                            while(reader.Read())
                            {
                                object o = reader[0];
                                if (o == DBNull.Value)
                                    data.Add(default(ReturnType));
                                else
                                    data.Add((ReturnType)Convert.ChangeType(o, typeof(ReturnType)));
                            }
                            reader.Close();
                        }
                        Connection.Close();
                        return data.ToArray();
                    }
                }
            }
            else
            {
                using (IDbCommand cmd = CommandUtility.GetProcedureCommand(Connection, Connector, ProcedureName, ParamNames, ParamValues))
                {
                    List<ReturnType> data = new List<ReturnType>();

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object o = reader[0];
                            if (o == DBNull.Value)
                                data.Add(default(ReturnType));
                            else
                                data.Add((ReturnType)Convert.ChangeType(o, typeof(ReturnType)));
                        }
                        reader.Close();
                    }
                    Connection.Close();
                    return data.ToArray();
                }
            }
        }
        #endregion
        //public static DynamicDataObject[] LoadJoins(IJoin[] Joins, Expression<Func<T, bool>> WhereExpression, params Expression<Func<T, object>>[] OrderByExpressions)
        //{
        //}
    }
}
