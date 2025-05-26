using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Threading;

namespace SNS.Data.DataSerializer
{
    internal class DataConnector<T> : IDataConnector where T : DbConnection, new()
    {
        string connectionString = "";
        string name = "";
        ConnectorSettings settings;

        public ConnectorSettings Settings
        {
            get { return settings; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        public int? DatabaseVersion { get; set; } = null;
        public int Retries { get; set; } = 0;
        public int RetrySeconds { get; set; } = 5;
        public int? DefaultCommandTimeout { get; set; } = null;

        public Type ADOConnectionType { get { return typeof(T); } }



        public DataConnector(string Name, string ConnectionString, ConnectorSettings Settings)
        {
            settings = Settings;
            name = Name;
            connectionString = ConnectionString;
        }
        public IDbConnection OpenConnection()
        {
            T connection = new T();
            bool connected = false;
            int retries = 0;
            Exception err = null;
            while (!connected && (retries < Retries || retries == 0))
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    connected = true;
                }
                catch (Exception ex)
                {
                    if (retries < Retries)
                        Thread.Sleep(RetrySeconds * 1000);
                    err = ex;
                }
                retries++;
            }
            if (!connected)
            {
                try
                {
                    connection.Dispose();
                    connection = null;
                }
                catch
                {

                }
                throw err;
            }
            return connection;

        }

        public IDataConnector Clone()
        {
            return (IDataConnector)this.MemberwiseClone();
        }
    }
}
