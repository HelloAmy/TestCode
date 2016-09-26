using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace YZ.Utility.DataAccess.DbProvider
{
    internal class SqlServerFactory : IDbFactory
    {

        public static SqlServerFactory Instance
        {
            get
            {
                return new SqlServerFactory();
            }
        }

        private SqlServerFactory()
        {

        }

        #region IDbFactory Members

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new SqlConnection();
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        #endregion


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
            return parameterValue.Replace("[", "[[]").Replace("_", "[_]").Replace("%", "[%]");
        }
    }
}
