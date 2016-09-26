using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Linq;

using YZ.Utility.DataAccess.DbProvider;
using YZ.Utility.DataAccess.Entity;
using YZ.Utility;

namespace YZ.Utility.DataAccess
{
    public class DbHelper
    {
        internal void GetConnectionInfo(string connectionKey, out string connectionString, out IDbFactory factory)
        {
            bool excludeTransaction;
            GetConnectionInfo(connectionKey, out connectionString, out factory, out excludeTransaction);
        }

        internal void GetConnectionInfo(string connectionKey, out string connectionString, out IDbFactory factory, out bool excludeTransaction)
        {

            DBConnection conn = DBConfigHelper.ConfigSetting.DBConnectionList.Find(f => f.Key.ToUpper().Trim() == connectionKey.ToUpper().Trim());
            if (conn == null)
            {
                throw new Exception(string.Format("Don't found DBConnection Key", connectionKey));
            }
            connectionString = conn.ConnectionString;
            factory = DbFactoryManager.GetFactory(conn.DBProviderType);
            excludeTransaction = string.Equals(conn.ExcludeTransaction, "true", StringComparison.InvariantCultureIgnoreCase);
        }

        private ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, IDbFactory factory)
        {
            return GetOpenConnection(connectionString, factory, true);
        }

        private ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, IDbFactory factory,
            bool disposeInnerConnection)
        {
            return TransactionScopeConnections.GetOpenConnection(connectionString, () => factory.CreateConnection(), disposeInnerConnection);
        }

        public int ExecuteNonQuery(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
        }

        public DbDataReader ExecuteReader(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();

            CommandBehavior cmdBehavior;
            if (Transaction.Current != null)
            {
                cmdBehavior = CommandBehavior.Default;
            }
            else
            {
                cmdBehavior = CommandBehavior.CloseConnection;
            }

            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataReader rdr = cmd.ExecuteReader(cmdBehavior);
                // cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
        }

        public object ExecuteScalar(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);
            DbCommand cmd = dbFactory.CreateCommand();

            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
        }

        public DataSet ExecuteDataSet(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);

            DbCommand cmd = dbFactory.CreateCommand();
            DataSet ds = new DataSet();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataAdapter sda = dbFactory.CreateDataAdapter();
                sda.SelectCommand = cmd;
                sda.Fill(ds);
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
            return ds;
        }

        public DataTable ExecuteDataTable(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);

            DbCommand cmd = dbFactory.CreateCommand();
            DataTable table = new DataTable();
            ConnectionWrapper<DbConnection> wrapper = null;
            try
            {
                wrapper = GetOpenConnection(connectionString, dbFactory);
                PrepareCommand(cmd, wrapper.Connection, null, cmdType, cmdText, timeout, commandParameters);
                DbDataAdapter sda = dbFactory.CreateDataAdapter();
                sda.SelectCommand = cmd;
                sda.Fill(table);
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, connectionString, cmdText, commandParameters);
            }
            finally
            {
                if (wrapper != null)
                {
                    wrapper.Dispose();
                }
            }
            return table;
        }

        public DataRow ExecuteDataRow(string connKey, CommandType cmdType, string cmdText, int timeout, params DbParameter[] commandParameters)
        {
            DataTable table = ExecuteDataTable(connKey, cmdType, cmdText, timeout, commandParameters);
            if (table.Rows.Count == 0)
            {
                return null;
            }
            return table.Rows[0];
        }

        private void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction trans, CommandType cmdType,
            string cmdText, int timeout, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandTimeout = timeout;

            if (trans != null)
            {
                cmd.Transaction = trans;
            }

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (DbParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }

        public string SetSafeParameter(string connKey, string parameterValue)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);
            return dbFactory.SetSafeParameter(parameterValue);

        }

        public string EscapeSpecialChars(string connKey, string parameterValue)
        {
            IDbFactory dbFactory;
            string connectionString;
            GetConnectionInfo(connKey, out connectionString, out dbFactory);
            return dbFactory.EscapeSpecialChars(parameterValue);

        }

        public string GetForceWriteDBConnKey(string connKey)
        {
            DBConnection conn = DBConfigHelper.ConfigSetting.DBConnectionList.Find(f => f.Key.ToUpper().Trim() == connKey.ToUpper().Trim());

            if (!string.IsNullOrWhiteSpace(conn.GroupID) &&
                string.Equals(conn.IsWrite, "false", StringComparison.InvariantCultureIgnoreCase))
            {
                var writeConn =
                    DBConfigHelper.ConfigSetting.DBConnectionList.FirstOrDefault(
                        f => f.GroupID == conn.GroupID && string.Equals(f.IsWrite, "true", StringComparison.InvariantCultureIgnoreCase));
                if (writeConn != null)
                    return writeConn.Key;
            }

            return connKey;
        }
    }
}
