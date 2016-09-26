using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace YZ.Utility.DataAccess.DbProvider
{
    public interface IDbFactory
    {
        DbCommand CreateCommand();
        DbConnection CreateConnection();
        DbConnection CreateConnection(string connectionString);
        DbDataAdapter CreateDataAdapter();
        DbParameter CreateParameter();

        string SetSafeParameter(string parameterValue);

        string EscapeSpecialChars(string parameterValue);
    }
}
