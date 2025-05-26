using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SNS.Data.DataSerializer.DataExtensions;
using System.Linq.Expressions;

namespace SNS.Data.DataSerializer
{
    public class DynamicQuery
    {
        List<CommandUtility.ExpressionParam> parameters = new List<CommandUtility.ExpressionParam>();
        List<Type> types = new List<Type>();
        int paramIndex = 0;
        string fromTables = "";
        string joins = "";
        string wheres = "";
        string orderBys = "";
        internal IDataConnector Connector { get; set; }
        public DynamicQuery(string ConnectorName)
        {
            this.Connector = Settings.GetConnector(ConnectorName);
        }
        public DynamicQuery()
        {
            this.Connector = Settings.GetConnector("");
        }
        public DynamicQuery From<T>() where T : IGenericDataObject<T>, new()
        {
            if (ReflectionCache<T>.Schema != "")
                fromTables += ReflectionCache<T>.Schema + ".";
            fromTables += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T>.Table + Connector.Settings.TableSpecifierSuffix + ", ";
            if (!types.Contains(typeof(T)))
                types.Add(typeof(T));
            return this;
        }
        public DynamicQuery Join<T1,T2>(JoinType JoinType, Expression<Func<T1, T2, bool>> JoinExpression) where T1 : IGenericDataObject<T1>, new()  where T2 : IGenericDataObject<T2>, new()
        {
            CommandUtility.ExpressionParam[] param;
            int paramStart = paramIndex;
            string j = "";
            if (JoinType == DataSerializer.JoinType.INNER)
                j += "INNER JOIN ";
            else if (JoinType == DataSerializer.JoinType.LEFT)
                j += "LEFT JOIN ";
            else if (JoinType == DataSerializer.JoinType.RIGHT)
                j += "RIGHT JOIN ";
            j += Connector.Settings.TableSpecifierPrefix + ReflectionCache<T2>.Table + Connector.Settings.TableSpecifierSuffix + " ON ";
            j += CommandUtility.BuildJoinClauseFromExpression<T1, T2>(Connector, JoinExpression,out param,ref paramStart);
            joins += j + " ";
            paramIndex = paramStart;
            if (!types.Contains(typeof(T2)))
                types.Add(typeof(T2));

            return this;
        }
        public DynamicQuery Or()
        {
            wheres += "OR ";
            return this;
        }
        public DynamicQuery And()
        {
            wheres += "AND ";
            return this;
        }
        public DynamicQuery StartParen()
        {
            wheres += "(";
            return this;
        }
        public DynamicQuery EndParen()
        {
            wheres += ") ";
            return this;
        }
        public DynamicDataObject[] Execute()
        {
            string commandText = "SELECT ";

            //if (limitMode == DatalizerSettings.LimitMode.TopSyntax && Limit > 0)
            //    cmd.CommandText += " TOP " + Limit + " ";
            foreach (Type t in types)
            {

                string jtSchema = ReflectionUtility.GetSchema(t);
                string jtTable = ReflectionUtility.GetTable(t);
                DataPropertyInfo[] dpis = ReflectionUtility.GetDataProperties(t);
                foreach (DataPropertyInfo dpi in dpis)
                {
                    if (dpi.PropertyAttribute != null)
                    {
                        if (jtSchema != "")
                            commandText += jtSchema + ".";
                        commandText += Connector.Settings.TableSpecifierPrefix + jtTable + Connector.Settings.TableSpecifierSuffix;
                        commandText += "." + Connector.Settings.FieldSpecifierPrefix + dpi.PropertyAttribute.GetColumn + Connector.Settings.FieldSpecifierSuffix + " AS ";
                        commandText += t.Name + "_" + dpi.PropertyAttribute.GetColumn + " , ";
                    }
                }

            }
             
            return null;
        }
        public DynamicQuery Where<T>(Expression<Func<T, bool>> WhereExpression) where T : IGenericDataObject<T>, new()
        {
            CommandUtility.ExpressionParam[] param;
            int paramStart = paramIndex;
            wheres += CommandUtility.BuildWhereClauseFromExpression<T>(Connector, WhereExpression, out param, ref paramStart) + " ";
            paramIndex = paramStart;

            return this;
        }
        public DynamicQuery OrderBy<T>(Expression<Func<T, object>> OrderByExpression) where T : IGenericDataObject<T>, new()
        {
            CommandUtility.ExpressionParam[] param;
            int paramStart = paramIndex;
            orderBys += CommandUtility.BuildOrderByClauseFromExpression(Connector, OrderByExpression, out param);
            paramIndex = paramStart;

            return this;
        }

    }
}
