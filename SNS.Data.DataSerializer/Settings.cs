using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Linq;
namespace SNS.Data.DataSerializer
{
    public static class Settings
    {
        static SortedList<string, IDataConnector> connectors = new SortedList<string, IDataConnector>();
        static IDataConnector defaultConnector;

        public static IDataConnector GetConnector(string ConnectorName)
        {
            if (ConnectorName == "")
            {
                return defaultConnector;
            }
            if (connectors.ContainsKey(ConnectorName))
                return connectors[ConnectorName];
            else
                throw new ApplicationException("Connector, " + ConnectorName + ", not registered.");
        }
        public static bool HasConnectorRegistered(string ConnectorName)
        {
            if (connectors.ContainsKey(ConnectorName))
                return true;
            else
                return false;
        }
        public static IDbConnection GetOpenConnection()
        {
            return GetOpenConnection("");
        }
        public static IDataConnector[] GetConnectors()
        {
            return connectors.Values.ToArray();
        }
        public static IDbConnection GetOpenConnection(string ConnectorName)
        {
            if (ConnectorName == "")
            {
                if (defaultConnector != null)
                    return defaultConnector.OpenConnection();
                else
                    throw new ApplicationException("No Default Connector Registered. Use Settings.RegisterConnector() To Specify a Connection");
            }
            if (connectors.ContainsKey(ConnectorName))
                return connectors[ConnectorName].OpenConnection();
            else
                throw new ApplicationException("Connector, " + ConnectorName + ", not registered.");
        }
        public static void RegisterConnector<T>(string Name, string ConnectionString, ConnectorSettings Settings, bool DefaultConnection, int ConnectionRetries = 0, int ConnectionRetrySeconds = 5, int? DefaultCommandTimeout = null) where T : DbConnection, new()
        {
            DataConnector<T> connector = new DataConnector<T>(Name, ConnectionString, Settings);
            connector.Retries = ConnectionRetries;
            connector.RetrySeconds = ConnectionRetrySeconds;
            connector.DefaultCommandTimeout = DefaultCommandTimeout;
            connectors.Add(Name, connector);
            if (DefaultConnection)
                defaultConnector = connector;
        }
        public static void UnregisterConnector(string Name)
        {
            connectors.Remove(Name);
        }
        public static void ClearConnectors()
        {
            connectors.Clear();
        }
       
    }
}
