using YZ.Utility.DataAccess.DbProvider;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility.DataAccess.DbProvider
{
    internal class MySqlFactory : IDbFactory
    { 

        public static MySqlFactory Instance
        {
            get
            {
                return new MySqlFactory(); 
            }
        }


        public System.Data.Common.DbCommand CreateCommand()
        {
            return new MySqlCommand(); 
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new MySqlConnection(); 
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString); 
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new MySqlDataAdapter(); 
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new MySqlParameter();
        }


        public string SetSafeParameter(string parameterValue)
        {
            string v = parameterValue.Replace("'", "\'").Replace(";",@"\;");
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
