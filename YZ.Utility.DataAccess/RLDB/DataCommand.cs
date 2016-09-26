using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Linq;
using YZ.Utility.DataAccess.Entity;
using YZ.Utility.DataAccess.DbProvider;
using YZ.Utility.DataAccess;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using System.Transactions;


namespace YZ.Utility.DataAccess
{
    public delegate void CutomDataCommandHandler(DataCommand cmd);

    public partial class DataCommand
    {
        public const string INIT_QUERYCONDITION_STRING = " WHERE 1=1 ";

        private SQL m_SQLNode;
        private string m_ConnString;
        private IDbFactory m_DBFactory;
        private string m_CommandText = string.Empty;
        private List<DbParameter> m_DbParameterList;
        private bool? m_ForceWriteDB;
        private bool m_ExcludeTransaction;

        public static event CutomDataCommandHandler OnDataCommandInit;
        public static event CutomDataCommandHandler OnDataCommandExecuteAfter;

        public SQL SQLNode { get { return m_SQLNode; } }

        /// <summary>
        /// 创建一个DataCommand
        /// </summary>
        /// <param name="sqlKey">SQL 语句配置文件中的节点SQLKey，如SQLKey="LoadProduct"</param>
        public DataCommand(string sqlKey)
        {
            if (string.IsNullOrWhiteSpace(sqlKey))
            {
                throw new Exception(string.Format("Can not input an empty sqlKey!", sqlKey));
            }
            m_SQLNode = SQLConfigHelper.GetSQLList().Find(f => f.SQLKey.Trim().ToUpper() == sqlKey.Trim().ToUpper());
            if (m_SQLNode == null)
            {
                throw new Exception(string.Format("Don't found SQLKey:{0} configuration!", sqlKey));
            }
            new DbHelper().GetConnectionInfo(m_SQLNode.ConnectionKey, out m_ConnString, out m_DBFactory, out m_ExcludeTransaction);

            m_DbParameterList = new List<DbParameter>();
            m_CommandText = m_SQLNode.Text;
            if (!string.IsNullOrWhiteSpace(m_SQLNode.ForceWriteDB))
            {
                m_ForceWriteDB = m_SQLNode.ForceWriteDB.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }

            var cmd = this;
            if (OnDataCommandInit != null)
                OnDataCommandInit(cmd);
        }


        /// <summary>
        /// 获取或者设置DataCommand的SQL语句
        /// </summary>
        public string CommandText
        {
            get
            {
                return m_CommandText;
            }
            set
            {
                m_CommandText = value;
            }
        }


        /// <summary>
        /// 强制使用WriteDB。
        /// 设置为true时，JsonRpc服务端执行时将在读取数据时覆盖原先的ReadDB数据库连接， 改使用对应的WriteDB数据库连接。
        /// 设置为false时，将不做额外的处理，服务端配置使用ReadDB还是WriteDB自定。
        /// </summary>
        public bool? ForceWriteDB
        {
            get { return m_ForceWriteDB; }
            set { m_ForceWriteDB = value; }
        }

        /// <summary>
        /// 切片数据库编号，将决定在哪些数据库执行语句。
        /// 如01，将决定只在01数据库执行，
        /// 如01,02将决定在01和02数据库同时执行。
        /// 如为*，将在所有配置的数据库上执行。
        /// </summary>
        public string SplitDbCode { get; set; }

        #region 设置参数
        /// <summary>
        /// 手动设置参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        public void SetParameter(string paramName, DbType dbType, object value)
        {
            SetParameter(paramName, dbType, value, 0, ParameterDirection.Input);
        }

        /// <summary>
        /// 手动设置参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        public void SetParameter(string paramName, DbType dbType, object value, int size)
        {
            SetParameter(paramName, dbType, value, size, ParameterDirection.Input);
        }

        /// <summary>
        /// 手动设置参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        public void SetParameter(string paramName, DbType dbType, object value, ParameterDirection direction)
        {
            SetParameter(paramName, dbType, value, 0, direction);
        }

        /// <summary>
        /// 手动设置参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        public void SetParameter(string paramName, DbType dbType, object value, int size, ParameterDirection direction)
        {
            if (!paramName.StartsWith("@"))
            {
                paramName = "@" + paramName;
            }
            DbParameter p = m_DbParameterList.Find(f => f.ParameterName.ToLower() == paramName.ToLower().Trim());
            if (p == null)
            {
                p = m_DBFactory.CreateParameter();
                p.ParameterName = paramName;
                p.DbType = dbType;
                m_DbParameterList.Add(p);
            }
            p.Value = ConvertDbValue(value);
            p.Size = size;
            p.Direction = direction;
        }

        /// <summary>
        /// 直接设置实体的参数，系统将根据类的属性名以及属性数据类型，自动Mapping到SQL中的参数上去
        /// 如果有手动设置参数，则手动设置参数的优先，后面自动匹配的则会跳过已手动设置的参数
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="manuSetParameter">使用手动设置参数:SetParameter，手动设置参数的优先，后面自动匹配的则会跳过已手动设置的参数</param>
        public void SetParameter<T>(T entity, Action<DataCommand, T> manuSetParameter = null) where T : class
        {
            //手动设置参数的优先，后面自动匹配的则会跳过已手动设置的参数
            if (manuSetParameter != null)
            {
                manuSetParameter(this, entity);
            }

            PropertyInfo[] propArray = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (string propName in m_SQLNode.ParameterNameList)
            {
                if (m_DbParameterList.Exists(f => f.ParameterName.ToLower().Trim() == propName.ToLower().Trim()))
                {
                    continue;
                }

                foreach (PropertyInfo prop in propArray)
                {
                    string tempName = "@" + prop.Name.ToLower();
                    if (tempName == propName.ToLower())
                    {
                        DbParameter p = m_DBFactory.CreateParameter();
                        p.ParameterName = propName;
                        p.DbType = ConvertDbType(prop.PropertyType);
                        p.Direction = ParameterDirection.Input;
                        p.Value = ConvertDbValue(Invoker.PropertyGet(entity, prop.Name));
                        m_DbParameterList.Add(p);
                    }
                }

                //TODO:对于对象型属性，可进行多层处理——待定，对性能会有比较大的问题，所以建议多层对象还是手动赋值
                //if(propName.Contains('.'))   {  }
            }
        }


        #endregion 设置参数End

        #region 值类型及赋值转换
        private object ConvertDbValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return DBNull.Value;
            }
            //枚举参数值转换为Int
            Type type = value.GetType();
            if (type.IsEnum ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1
                    && type.GetGenericArguments()[0].IsEnum))
            {
                return Convert.ChangeType(value, type.BaseType);
            }
            return value;
        }

        private DbType ConvertDbType(Type type)
        {
            Type baseType = type;
            //process enum
            if (type.IsEnum ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1
                    && type.GetGenericArguments()[0].IsEnum))
            {
                Type enumBaseType = null;
                if (type.IsEnum)
                {
                    enumBaseType = type.GetEnumUnderlyingType();
                }
                else
                {
                    enumBaseType = type.GetGenericArguments()[0];
                }

                if (enumBaseType == typeof(byte))
                {
                    return DbType.Byte;
                }
                else
                {
                    return DbType.Int32;
                }
            }
            //process generic type
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                baseType = type.GetGenericArguments()[0];
            }

            //TODO:转换所有的
            switch (baseType.Name.ToLower())
            {
                case "int32":
                    return DbType.Int32;
                case "int16":
                    return DbType.Int16;
                case "int64":
                    return DbType.Int64;
                case "decimal":
                    return DbType.Decimal;
                case "single":
                    return DbType.Decimal;
                case "double":
                    return DbType.Double;
                case "datetime":
                    return DbType.DateTime;
                case "guid":
                    return DbType.Guid;
                case "boolean":
                    return DbType.Int32;
                default:
                    return DbType.String;
            }

        }
        #endregion

        #region DataCommand的CURD方法
        /// <summary>
        /// 对于普通CURD的DataCommand方法，提供动态构建条件的方法，如果使用的SetDynamicParameter，请在执行时使用含有dynamicParameters参数的Exeute***方法
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="condOperation"></param>
        /// <param name="parameterDbType"></param>
        /// <param name="objValue"></param>
        public void SetDynamicParameter(string fieldName, ConditionOperation condOperation, DbType parameterDbType, object objValue)
        {
            QuerySetCondition(fieldName, condOperation, parameterDbType, objValue);
        }

        private void ParseDynamicParametersForExecute(string dynamicParameters)
        {
            if (m_QueryConditionString.Contains(INIT_QUERYCONDITION_STRING))
            {
                m_QueryConditionString = m_QueryConditionString.Replace(INIT_QUERYCONDITION_STRING, "");
            }
            m_CommandText = m_CommandText.Replace(dynamicParameters, m_QueryConditionString);
        }

        /// <summary>
        /// 标准方法：执行返回DataSet
        /// </summary>
        /// <returns></returns>
        public DataSet ExecuteDataSet()
        {
            Func<string, DataSet> func = connKey =>
               new DbHelper().ExecuteDataSet(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());

            var datasets = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
            return MergeDataSet(datasets);
        }
        /// <summary>
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  DataSet ds = cmd.ExecuteDataSet("#DynamicParameters#");
        /// </summary>
        /// <param name="dynamicParameters">动态条件的占位符，建议用#DynamicParameters#，可以自行定义，只要保持与SQL脚本中占位符一致即可</param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteDataSet();
        }

        /// <summary>
        /// 标准方法：执行返回DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteDataTable()
        {
            Func<string, DataTable> func = connKey =>
                new DbHelper().ExecuteDataTable(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut,
                    m_DbParameterList.ToArray());

            var tables = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
            return MergeDataTable(tables);
        }

        /// <summary>
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  DataTable dt = cmd.ExecuteDataTable("#DynamicParameters#");
        /// </summary>
        /// <param name="dynamicParameters">动态条件的占位符，建议用#DynamicParameters#，可以自行定义，只要保持与SQL脚本中占位符一致即可</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteDataTable();
        }

        /// <summary>
        /// 执行返回对象列表
        /// </summary>
        /// <typeparam name="T">实体类型，必须是可无参实例化的class</typeparam>
        /// <param name="manualMapper">可以手动mapping</param>
        /// <returns></returns>
        public List<T> ExecuteEntityList<T>(Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            DataTable dt = ExecuteDataTable();

            List<T> list = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
            {
                list = DataMapper.GetEntityList<T, List<T>>(dt.Rows, true, true, manualMapper);
            }
            return list;
        }

        /// <summary>
        /// 执行返回对象列表; 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteEntityList<T>("#DynamicParameters#");
        /// </summary> 
        /// <typeparam name="T">实体类型，必须是可无参实例化的class</typeparam>
        /// <param name="dynamicParameters"></param>
        /// <param name="manualMapper"></param>
        /// <returns></returns>
        public List<T> ExecuteEntityList<T>(string dynamicParameters, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteEntityList<T>(manualMapper);
        }

        /// <summary>
        /// 标准方法：执行返回第一行的DataRow
        /// </summary>
        /// <returns></returns>
        public DataRow ExecuteDataRow()
        {
            Func<string, DataRow> func = connKey =>
               new DbHelper().ExecuteDataRow(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());
            var rows = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
            return MergeDataRow(rows);
        }

        /// <summary>
        /// 执行返回第一行的DataRow;
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteDataRow("#DynamicParameters#");
        /// </summary>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public DataRow ExecuteDataRow(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteDataRow();
        }

        /// <summary>
        /// 执行返回一个实体对象，将第一行DataRow转换为实体
        /// </summary>
        /// <typeparam name="T">实体类型，必须是可无参实例化的class</typeparam>
        /// <param name="manualMapper">可以手动mapping</param>
        /// <returns></returns>
        public T ExecuteEntity<T>(Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            DataRow dr = ExecuteDataRow();
            if (dr != null)
            {
                T t = DataMapper.GetEntity<T>(dr, true, true, manualMapper);
                return t;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 执行返回一个实体对象，将第一行DataRow转换为实体;
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteEntity<T>("#DynamicParameters#");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicParameters"></param>
        /// <param name="manualMapper"></param>
        /// <returns></returns>
        public T ExecuteEntity<T>(string dynamicParameters, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteEntity<T>(manualMapper);
        }

        /// <summary>
        /// 标准方法：执行返回第一行第一列的值
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar()
        {
            Func<string, object> func = connKey =>
               new DbHelper().ExecuteScalar(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());

            var objects = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);

            return MergeScalar(objects);
        }

        /// <summary>
        /// 执行返回第一行第一列的值,
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteScalar("#DynamicParameters#");
        /// </summary>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteScalar();
        }

        /// <summary>
        /// 执行返回第一行第一列的值，并自动转换为对应的类型，如果是泛型值且为空则会返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ExecuteScalar<T>()
        {
            object v = DataMapper.ConvertIfEnum(ExecuteScalar(), typeof(T));
            return DataConvertor.GetValue<T>(v, null, null);
        }

        /// <summary>
        /// 执行返回第一行第一列的值，并自动转换为对应的类型，如果是泛型值且为空则会返回null
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteScalar<T>("#DynamicParameters#");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteScalar<T>();
        }

        /// <summary>
        /// 标准方法：执行无返回
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            Func<string, int> func = connKey =>
               new DbHelper().ExecuteNonQuery(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());

            var nums = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
            return MergeNonQuery(nums);
        }

        public int ExecuteNonQuery(bool usStoredProcedure)
        {
            if (usStoredProcedure)
            {
                Func<string, int> func = connKey =>
                   new DbHelper().ExecuteNonQuery(connKey, CommandType.StoredProcedure, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());

                var nums = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
                return MergeNonQuery(nums);
            }
            else
            {
                return ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 标准方法：执行无返回;
        /// 含有动态条件的执行，如sql：WHERE CategorySysNo=@CategorySysNo #DynamicParameters#，那么DataAccess层中的代码应该如下：
        ///  cmd.SetParameter("@CategorySysNo", DbType.Int32, categorySysNo);
        ///  cmd.SetDynamicParameter("ProductStatus", ConditionOperation.In, DbType.Int32, pstatusList);
        ///  cmd.ExecuteNonQuery("#DynamicParameters#");
        /// </summary>
        /// <param name="dynamicParameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string dynamicParameters)
        {
            ParseDynamicParametersForExecute(dynamicParameters);
            return ExecuteNonQuery();
        }


        private T[] ExecuteMultipleConnections<T>(string sourceConnKey, Func<string, T> func)
        {
            if (ForceWriteDB == true)
                sourceConnKey = new DbHelper().GetForceWriteDBConnKey(sourceConnKey);

            string[] connKeys = DBConnectionRouteHelper.GetRouteConnectionKeyList(sourceConnKey, SplitDbCode);

            if (connKeys.Length == 1)
            {
                T[] objectList = null;
                InvokeSupportExcludeTransaction(() =>
                {
                    var result = func(connKeys[0]);
                    if (result != null)
                    {
                        objectList = new T[] { result };
                    }
                }, m_ExcludeTransaction);

                if (OnDataCommandExecuteAfter != null)
                    OnDataCommandExecuteAfter(this);

                return objectList;
            }
            else
            {
                ArrayList objectList = ArrayList.Synchronized(new ArrayList());
                Task[] tasks = new Task[connKeys.Length];
                for (int i = 0; i < connKeys.Length; i++)
                {
                    string connKey = connKeys[i];
                    tasks[i] = Task.Factory.StartNew(() =>
                    {
                        InvokeSupportExcludeTransaction(() =>
                        {
                            var result = func(connKey);
                            if (result != null)
                            {
                                objectList.Add(result);
                            }
                        }, m_ExcludeTransaction);
                    });
                }
                try
                {
                    Task.WaitAll(tasks);

                    if (OnDataCommandExecuteAfter != null)
                        OnDataCommandExecuteAfter(this);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    else
                        throw;
                }
                return (T[])objectList.ToArray(typeof(T));
            }
        }

        private object MergeScalar(object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return null;

            return objects[0];
        }


        private DataRow MergeDataRow(DataRow[] rows)
        {
            if (rows == null || rows.Length == 0)
                return null;

            return rows.FirstOrDefault(dataRow => dataRow != null);
        }


        private DataSet MergeDataSet(DataSet[] datasets)
        {
            if (datasets == null || datasets.Length == 0)
                return null;

            if (datasets.Length == 1)
                return datasets[0];

            var mainDatasSet = datasets[0];

            for (int i = 1; i < datasets.Length; i++)
            {
                for (int j = 0; j < mainDatasSet.Tables.Count; j++)
                {
                    var mainDataTable = mainDatasSet.Tables[j];

                    foreach (DataRow row in datasets[i].Tables[j].Rows)
                        mainDataTable.ImportRow(row);
                }
            }

            return mainDatasSet;
        }


        private int MergeNonQuery(int[] nums)
        {
            if (nums == null || nums.Length == 0)
                return 0;

            return nums.Aggregate(0, (current, num) => current + num);
        }


        private DataTable MergeDataTable(DataTable[] tables)
        {
            if (tables == null || tables.Length == 0)
                return null;

            if (tables.Length == 1)
                return tables[0];

            var resultTable = tables[0];
            for (int i = 1; i < tables.Length; i++)
            {
                foreach (DataRow row in tables[i].Rows)
                {
                    resultTable.ImportRow(row);
                }
            }

            return resultTable;
        }


        private static void InvokeSupportExcludeTransaction(Action f, bool excludeTransaction)
        {
            if (Transaction.Current != null && excludeTransaction)
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    f.Invoke();
                    ts.Complete();
                }
            }
            else
            {
                f.Invoke();
            }
        }
        #endregion

        #region 查询专用
        /// <summary>
        /// 查询，返回条件页的DataTable；
        /// 查询的SQL语句有两个，第一个返回总的数量，第二个返回数据结果集；
        /// 查询条件可以使用参数，也可以自己定义拼装，注意拼装时，对参数值都使用SetSafeParameter(string parameterValue)处理一下。
        /// </summary>
        /// <param name="filter">继承QueryFilter的查询条件对象</param>
        /// <returns></returns>
        public QueryResult Query(QueryFilter filter, string defaultSortBy)
        {
            DataSet ds = _queryExecute(filter, defaultSortBy);
            int count = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            QueryResult result = new QueryResult();
            result.data = ds.Tables[1];
            result.PageIndex = filter.PageIndex;
            result.PageSize = filter.PageSize;
            result.SortFields = filter.SortFields;
            result.recordsTotal = count;
            return result;
        }

        /// <summary>
        /// 查询，返回条件页的实体列表
        /// 查询的SQL语句有两个，第一个返回总的数量，第二个返回数据结果集
        /// 查询条件可以使用参数，也可以自己定义拼装，注意拼装时，对参数值都使用SetSafeParameter(string parameterValue)处理一下。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public QueryResult<T> Query<T>(QueryFilter filter, string defaultSortBy, Action<DataRow, T> manualMapper = null) where T : class, new()
        {
            DataSet ds = _queryExecute(filter, defaultSortBy);
            int count = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            QueryResult<T> result = new QueryResult<T>();
            result.PageIndex = filter.PageIndex;
            result.PageSize = filter.PageSize;
            result.SortFields = filter.SortFields;
            result.draw = filter.draw;
            result.recordsTotal = count;

            if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
            {
                result.data = DataMapper.GetEntityList<T, List<T>>(ds.Tables[1].Rows, true, true, manualMapper);
            }
            else
            {
                result.data = new List<T>();
            }

            if (ds.Tables.Count >= 3)
            {
                result.Summary = ds.Tables[2].Rows[0][0] != null ? ds.Tables[2].Rows[0][0].ToString().Trim() : "";
                result.AllPageTotalInfo = DataMapper.GetEntity<T>(ds.Tables[2].Rows[0]);
            }

            return result;
        }

        private DataSet _queryExecute(QueryFilter filter, string defaultSortBy)
        {
            if (!string.IsNullOrWhiteSpace(SplitDbCode) && (SplitDbCode.Contains(",") || SplitDbCode == "*"))
                throw new NotSupportedException("Common query method not support multiple splitDbCode: " + SplitDbCode);

            if (string.IsNullOrWhiteSpace(defaultSortBy))
            {
                throw new Exception("Default SortBy can not be empty!");
            }
            if (string.IsNullOrWhiteSpace(filter.SortFields))
            {
                filter.SortFields = defaultSortBy;
            }
            m_CommandText = m_CommandText.Replace("@SortFields", filter.SortFields);
            m_CommandText = m_CommandText.Replace("@PageSize", filter.PageSize.ToString());
            m_CommandText = m_CommandText.Replace("@PageIndex", filter.PageIndex.ToString());

            m_CommandText = m_CommandText.Replace("#STRWHERE#", m_QueryConditionString);

            Func<string, DataSet> func = connKey =>
                new DbHelper().ExecuteDataSet(connKey, CommandType.Text, m_CommandText, m_SQLNode.TimeOut, m_DbParameterList.ToArray());
            DataSet[] datasets = ExecuteMultipleConnections(m_SQLNode.ConnectionKey, func);
            var ds = MergeDataSet(datasets);

            if (ds.Tables.Count < 2)
            {
                throw new Exception("Query SQL Script is Error, it should return 2 tables, 1st table is record count, 2rd table is dataresult.");
            }
            int count = 0;
            if (ds.Tables[0].Rows[0][0] == null || !int.TryParse(ds.Tables[0].Rows[0][0].ToString(), out count))
            {
                throw new Exception("Query SQL Script is Error, 1st table is record count, it must be integer.");
            }
            if (datasets.Length > 1)
            {
                count = ds.Tables[0].Rows.Cast<DataRow>().Aggregate(0, (current, row) => current + Convert.ToInt32(row[0]));
                ds.Tables[0].Rows[0][0] = count;
            }

            return ds;


        }


        private string m_QueryConditionString = INIT_QUERYCONDITION_STRING;
        public string QueryConditionString
        {
            get
            {
                return m_QueryConditionString;
            }
            set
            {
                m_QueryConditionString = value;
            }
        }

        /// <summary>
        /// 设置查询参数，都是AND模式，如果值为空，将自动不作为条件
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="condOperation"></param>
        /// <param name="parameterDbType"></param>
        /// <param name="objValue"></param>
        public void QuerySetCondition(string fieldName, ConditionOperation condOperation, DbType parameterDbType, object objValue)
        {
            if (objValue == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(objValue.ToString()))
            {
                return;
            }

            string strCond = "";
            switch (condOperation)
            {
                case ConditionOperation.Equal:
                    strCond = string.Format(" AND {0}={1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.NotEqual:
                    strCond = string.Format(" AND {0}<>{1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.LessThan:
                    strCond = string.Format(" AND {0}<{1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.LessThanEqual:
                    strCond = string.Format(" AND {0}<={1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.MoreThan:
                    strCond = string.Format(" AND {0}>{1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.MoreThanEqual:
                    strCond = string.Format(" AND {0}>={1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
                case ConditionOperation.Like:
                    strCond = string.Format(" AND {0} LIKE {1}", fieldName, "'%" + BuildLikeParameter(objValue) + "%'");
                    break;
                case ConditionOperation.LikeLeft:
                    strCond = string.Format(" AND {0} LIKE {1}", fieldName, "'" + BuildLikeParameter(objValue) + "%'");
                    break;
                case ConditionOperation.LikeRight:
                    strCond = string.Format(" AND {0} LIKE {1}", fieldName, "'%" + BuildLikeParameter(objValue) + "'");
                    break;
                case ConditionOperation.In:
                    if (typeof(IList).IsAssignableFrom(objValue.GetType()))
                    {
                        if (objValue is List<int> || objValue is List<Int16> || objValue is List<Int32> || objValue is List<Int64>
                            || objValue is List<uint> || objValue is List<UInt16> || objValue is List<UInt32> || objValue is List<UInt64>
                            || objValue is List<decimal> || objValue is List<float> || objValue is List<long> || objValue is List<double>)
                        {
                            string lstStr = string.Empty;
                            foreach (var x in (IEnumerable)objValue)
                            {
                                lstStr += x.ToString() + ",";
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                lstStr = lstStr.TrimEnd(',');
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                strCond = string.Format(" AND {0} IN ({1})", fieldName, lstStr);
                            }
                        }
                        else
                        {
                            string lstStr = string.Empty;
                            foreach (var x in (IEnumerable)objValue)
                            {
                                lstStr += "'" + SetSafeParameter(x.ToString()) + "',";
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                lstStr = lstStr.TrimEnd(',');
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                strCond = string.Format(" AND {0} IN ({1})", fieldName, lstStr);
                            }
                        }
                    }
                    else
                    {
                        strCond = string.Format(" AND {0} IN ({1})", fieldName, objValue.ToString());
                    }
                    break;
                case ConditionOperation.NotIn:
                    if (typeof(IList).IsAssignableFrom(objValue.GetType()))
                    {
                        if (objValue is List<int> || objValue is List<Int16> || objValue is List<Int32> || objValue is List<Int64>
                            || objValue is List<uint> || objValue is List<UInt16> || objValue is List<UInt32> || objValue is List<UInt64>
                            || objValue is List<decimal> || objValue is List<float> || objValue is List<long> || objValue is List<double>)
                        {
                            string lstStr = string.Empty;
                            foreach (var x in (IEnumerable)objValue)
                            {
                                lstStr += x.ToString() + ",";
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                lstStr = lstStr.TrimEnd(',');
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                strCond = string.Format(" AND {0} NOT IN ({1})", fieldName, lstStr);
                            }
                        }
                        else
                        {
                            string lstStr = string.Empty;
                            foreach (var x in (IEnumerable)objValue)
                            {
                                lstStr += "'" + SetSafeParameter(x.ToString()) + "',";
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                lstStr = lstStr.TrimEnd(',');
                            }
                            if (!string.IsNullOrEmpty(lstStr))
                            {
                                strCond = string.Format(" AND {0} NOT IN ({1})", fieldName, lstStr);
                            }
                        }
                    }
                    else
                    {
                        strCond = string.Format(" AND {0} NOT IN ({1})", fieldName, objValue.ToString());
                    }

                    break;
                default:
                    strCond = string.Format(" AND {0}={1}", fieldName, _buildConditionValue(parameterDbType, objValue));
                    break;
            }
            m_QueryConditionString += strCond;


        }



        /// <summary>
        /// 设置自定义查询参数，模式由查询条件指定
        /// </summary>
        /// <param name="customerQueryCondition"></param>
        public void QuerySetCondition(string customerQueryCondition)
        {
            if (string.IsNullOrWhiteSpace(customerQueryCondition))
            {
                return;
            }
            m_QueryConditionString += " " + customerQueryCondition;
        }

        private string _buildConditionValue(DbType parameterDbType, object objValue)
        {
            if (objValue.GetType().IsEnum)
            {
                if (objValue.GetType().GetEnumUnderlyingType() == typeof(byte))
                {
                    objValue = (byte)objValue;
                }
                else
                {
                    objValue = (int)objValue;
                }
            }
            switch (parameterDbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Single:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.DateTimeOffset:
                case DbType.SByte:
                case DbType.Byte:
                case DbType.Time:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.VarNumeric:
                    return SetSafeParameter(objValue.ToString());
                case DbType.DateTime:
                    return "'" + SetSafeParameter(Convert.ToDateTime(objValue).ToString("yyyy-MM-dd HH:mm:ss.fff")) + "'";
                default:
                    return "'" + SetSafeParameter(objValue.ToString()) + "'";
            }

        }

        /// <summary>
        /// 设置安全参数值，防注入攻击；尤其是在拼装SQL 查询语句时需要
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public string SetSafeParameter(string parameterValue)
        {
            string v = new DbHelper().SetSafeParameter(m_SQLNode.ConnectionKey, parameterValue);
            return v;
        }

        /// <summary>
        /// 将特殊字符(_,%,[l等)进行转义
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public string EscapeSpecialChars(string parameterValue)
        {
            string v = new DbHelper().EscapeSpecialChars(m_SQLNode.ConnectionKey, parameterValue);
            return v;
        }

        /// <summary>
        /// 构造Like查询参数，会将其中的特殊字符(_,%,[l等)进行转义
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public string BuildLikeParameter(object objValue)
        {
            string likeValue = SetSafeParameter(objValue.ToString());
            likeValue = EscapeSpecialChars(likeValue);
            return likeValue;
        }

        #endregion

    }

    public enum ConditionOperation
    {
        Equal,
        NotEqual,
        MoreThan,
        MoreThanEqual,
        LessThan,
        LessThanEqual,
        Like,
        LikeRight,
        LikeLeft,
        In,
        NotIn
    }

}
