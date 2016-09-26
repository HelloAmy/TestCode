using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace YZ.Utility.DataAccess.DbProvider
{
    public class SQLiteFactory : IDbFactory
    {
        public static SQLiteFactory Instance
        {
            get
            {
                return new SQLiteFactory();
            }
        }

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new SQLiteCommand(); 
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new SQLiteConnection(); 
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString); 
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new SQLiteDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new SQLiteParameter(); 
        }

        public string SetSafeParameter(string parameterValue)
        {
            string v = parameterValue.Replace("'", "''");
            return v;
        }

        public string EscapeSpecialChars(string parameterValue)
        {
            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                return string.Empty;
            }
            return parameterValue.Replace("[", @"\[").Replace("_", @"\_").Replace("%", @"\%");
        }
    }
}
