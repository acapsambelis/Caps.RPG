using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SNS.Data.DataSerializer
{
    public class ConnectorSettings
    {
        public enum LimitMode
        {
            Unsupported = 0,
            TopSyntax = 1,
            LimitSyntax = 2
        }
        DateTime minimumDateTimeSupported = new DateTime(1753, 1, 1);
        string variableSymbol = "@";
        string fieldSpecifierPrefix = "[";
        string fieldSpecifierSuffix = "]";
        string tableSpecifierPrefix = "[";
        string tableSpecifierSuffix = "]";
        string lastIdCommand = "SCOPE_IDENTITY()";
        LimitMode selectLimitMode = LimitMode.TopSyntax;

        public LimitMode SelectLimitMode
        {
            get { return selectLimitMode; }
            set { selectLimitMode = value; }
        }
        public DateTime MinimumDateTimeSupported
        {
            get { return minimumDateTimeSupported; }
            set { minimumDateTimeSupported = value; }
        }
        public string LastIDCommand
        {
            get { return lastIdCommand; }
            set { lastIdCommand = value; }
        }
        public string TableSpecifierSuffix
        {
            get { return tableSpecifierSuffix; }
            set { tableSpecifierSuffix = value; }
        }

        public string TableSpecifierPrefix
        {
            get { return tableSpecifierPrefix; }
            set { tableSpecifierPrefix = value; }
        }
        public string FieldSpecifierSuffix
        {
            get { return fieldSpecifierSuffix; }
            set { fieldSpecifierSuffix = value; }
        }
        public string FieldSpecifierPrefix
        {
            get { return fieldSpecifierPrefix; }
            set { fieldSpecifierPrefix = value; }
        }
        public string VariableSymbol
        {
            get { return variableSymbol; }
            set { variableSymbol = value; }
        }

        public static ConnectorSettings GetMsSqlSettings()
        {
            ConnectorSettings settings = new ConnectorSettings();

            settings.variableSymbol = "@";
            settings.fieldSpecifierPrefix = "[";
            settings.fieldSpecifierSuffix = "]";
            settings.tableSpecifierPrefix = "[";
            settings.tableSpecifierSuffix = "]";
            settings.lastIdCommand = "SCOPE_IDENTITY()";
            settings.selectLimitMode = LimitMode.TopSyntax;
            settings.minimumDateTimeSupported = new DateTime(1753, 1, 1);
            return settings;
        }
        public static ConnectorSettings GetMySqlSettings()
        {
            ConnectorSettings settings = new ConnectorSettings();
            settings.variableSymbol = "?_";
            settings.fieldSpecifierPrefix = "`";
            settings.fieldSpecifierSuffix = "`";
            settings.tableSpecifierPrefix = "`";
            settings.tableSpecifierSuffix = "`";
            settings.lastIdCommand = "LAST_INSERT_ID()";
            settings.selectLimitMode = LimitMode.LimitSyntax;
            settings.minimumDateTimeSupported = DateTime.MinValue;
            return settings;
        }
    }
}
