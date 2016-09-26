using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace YZ.Utility.DataAccess.DbProvider
{
    internal class OleDbFactory : IDbFactory
    { 

        public static OleDbFactory Instance
        {
            get
            {
                return new OleDbFactory(); 
            }
        }

        private OleDbFactory()
        {

        }

        #region IDbFactory Members

        public System.Data.Common.DbCommand CreateCommand()
        {
            return new OleDbCommand();
        }

        public System.Data.Common.DbConnection CreateConnection()
        {
            return new OleDbConnection();
        }

        public System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        public System.Data.Common.DbDataAdapter CreateDataAdapter()
        {
            return new OleDbDataAdapter();
        }

        public System.Data.Common.DbParameter CreateParameter()
        {
            return new OleDbParameter();
        }

        #endregion


        public string SetSafeParameter(string parameterValue)
        {
            string v = parameterValue;
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
