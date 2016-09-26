using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using YZ.JsonRpc;

namespace ServiceHost
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {

            JsonRpcRegister.LoadFromConfig();
            YZ.JsonRpc.Config.SetErrorHandler(OnJsonRpcException);
            Config.SetPreProcessHandler(new PreProcessHandler(PreProcess));
            Config.SetCompletedProcessHandler(new CompletedProcessHandler(CompletedProcess));
            DataCommand.OnDataCommandInit += DataCommand_OnDataCommandInit;
            DataCommand.OnDataCommandExecuteAfter += DataCommand_OnDataCommandExecuteAfter;
            InjectServiceToClient();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //ViewEngines.Engines.Clear();
            //ViewEngines.Engines.Add(new ThemeableRazorViewEngine());
        }

        #region DataCommand ForceWriteDB Handlers

        void DataCommand_OnDataCommandInit(DataCommand cmd)
        {
            if (cmd.ForceWriteDB == null)
            {
                if (string.Equals(DataContext.GetContextItemString("ForceWriteDB"), "true", StringComparison.InvariantCultureIgnoreCase))
                {
                    cmd.ForceWriteDB = true;
                    return;
                }

                if (ConfigurationManager.AppSettings["ForceWriteDB-AutoSwtich-Enabled"] != "true")
                    return;

                SQL sqlNode = cmd.SQLNode;
                var cmdDbConn = DBConfigHelper.ConfigSetting.DBConnectionList.Find(db => db.Key == sqlNode.ConnectionKey);
                bool isRead = !string.Equals(cmdDbConn.IsWrite, "true", StringComparison.InvariantCultureIgnoreCase);
                if (isRead)
                {
                    if (IsJustWritedDb(sqlNode))
                    {
                        cmd.ForceWriteDB = true;
                    }
                }
            }
        }

        void DataCommand_OnDataCommandExecuteAfter(DataCommand cmd)
        {
            if (ConfigurationManager.AppSettings["ForceWriteDB-AutoSwtich-Enabled"] != "true")
                return;

            SQL sqlNode = cmd.SQLNode;
            var cmdDbConn = DBConfigHelper.ConfigSetting.DBConnectionList.Find(db => db.Key == sqlNode.ConnectionKey);
            bool isWrite = string.Equals(cmdDbConn.IsWrite, "true", StringComparison.InvariantCultureIgnoreCase);
            if (isWrite)
            {
                SetWritedDbFlag(sqlNode);
            }
        }

        private string GetWritedDbFlagCacheKey(SQL sqlNode)
        {
            if (DataContext.GetContextItem("UserSysNo") == null)
                return null;

            string prefix = "WritedDb";
            string connKey = new DbHelper().GetForceWriteDBConnKey(sqlNode.ConnectionKey); // read/write连接都要统一以write连接key做为判断标识
            string visitorId = string.Concat(DataContext.GetContextItem("UserSysNo"),
                "-" + DataContext.GetContextItem("ApplicationKey"),
                "-" + DataContext.GetContextItem("LoginTime"));
            if (visitorId == "--")
                return null;
            string cacheKey = CacheManager.GenerateKey(prefix, connKey, visitorId);
            return cacheKey;
        }

        private void SetWritedDbFlag(SQL sqlNode)
        {
            string cacheKey = GetWritedDbFlagCacheKey(sqlNode);
            if (cacheKey == null)
                return;

            CacheManager.Remove(cacheKey);
            CacheManager.GetWithCache(cacheKey, () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), 300);
        }

        private bool IsJustWritedDb(SQL sqlNode)
        {
            string cacheKey = GetWritedDbFlagCacheKey(sqlNode);

            if (cacheKey == null)
                return false;

            string cacheTime = CacheManager.GetWithCache(cacheKey, () => string.Empty, 300);

            if (string.IsNullOrWhiteSpace(cacheTime))
                return false;
            double defaultJustSeconds = 5;
            double justSeconds = defaultJustSeconds;
            string appcfgSeconds = ConfigurationManager.AppSettings["ForceWriteDB-AutoSwtich-Seconds"];
            if (!string.IsNullOrWhiteSpace(appcfgSeconds))
            {
                if (!double.TryParse(appcfgSeconds, out justSeconds))
                    justSeconds = defaultJustSeconds;
            }

            return (DateTime.Now - DateTime.Parse(cacheTime)).TotalSeconds <= justSeconds;
        }

        #endregion

        #region JsonRpc Config Handlers

        private YZ.JsonRpc.JsonRpcException OnJsonRpcException(YZ.JsonRpc.JsonRequest rpc, YZ.JsonRpc.JsonRpcException ex)
        {
            // Serivce Call Service 业务异常透传
            if (ex.data is YZ.JsonRpc.Client.JsonRpcException && ((YZ.JsonRpc.Client.JsonRpcException)ex.data).code == 32000)
            {
                ex.code = 32000;
                ex.message = ((YZ.JsonRpc.Client.JsonRpcException)ex.data).message;
                ex.data = null;
                return ex;
            }

            // Service内部产生的异常是BusinessException时，需要抛给调用端，并且指定RPC错误编码为：32000，没有堆栈。
            // 自定义代码，抛出业务异常
            if (ex.data is YZ.Utility.BusinessException)
            {
                ex.code = 32000;
                ex.message = ((BusinessException)ex.data).Message;
                ex.data = null;
            }
            else
            {

                if (ex.data != null)
                    ex.data = ex.data.ToString();
                object j = JsonRpcContext.Current().Value;

                HttpContextBase httpContext = null;
                if (j != null)
                {
                    if (j.GetType() == typeof(HttpRequest))
                    {
                        httpContext = ((HttpRequest)JsonRpcContext.Current().Value).RequestContext.HttpContext;

                    }
                }

                try
                {
                    //增加前缀Mall区分是商城的日志
                    var categoryName = string.Format("Mall_{0}", rpc.Method.Split('.')[0]);
                    new SystemLogService().WriteSystemLog(string.Format("{0} - {1}", ex.Message, ex.data), "microService", categoryName,
                        null, string.Format("url:{0} method:{1}", httpContext == null ? string.Empty : httpContext.Request.Url.ToString(), rpc.Method));
                }
                catch (Exception writeLogEx)
                {
                    Logger.WriteLog(writeLogEx.ToString());
                }
            }

            return ex;
        }

        private JsonRpcException PreProcess(JsonRequest rpc, object context)
        {
            NameValueCollection contextNameValueCollection = null;

            if (context != null)
            {
                if (context.GetType() == typeof(NameValueCollection))
                {
                    return null;
                    //contextNameValueCollection = (NameValueCollection)context;
                }
                else if (context.GetType() == typeof(HttpRequest))
                {
                    contextNameValueCollection = ((HttpRequest)context).RequestContext.HttpContext.Request.Headers;
                }
            }


            DataContext.SetContextItem("X-Start-Time", DateTime.Now);

            DataContext.SetContextItem("UserSysNo", HttpUtility.UrlDecode(contextNameValueCollection["x-user-UserSysNo"]));
            DataContext.SetContextItem("UserID", HttpUtility.UrlDecode(contextNameValueCollection["x-user-UserID"]));
            DataContext.SetContextItem("UserDisplayName", HttpUtility.UrlDecode(contextNameValueCollection["x-user-UserDisplayName"]));
            DataContext.SetContextItem("ApplicationKey", HttpUtility.UrlDecode(contextNameValueCollection["x-user-ApplicationKey"]));
            //DataContext.SetContextItem("DataPermission"        , HttpUtility.UrlDecode(contextNameValueCollection["x-user-DataPermission"]));
            DataContext.SetContextItem("LoginTime", HttpUtility.UrlDecode(contextNameValueCollection["x-user-LoginTime"]));
            DataContext.SetContextItem("SupplierSysNo", HttpUtility.UrlDecode(contextNameValueCollection["x-user-SupplierSysNo"]));
            DataContext.SetContextItem("SupplierUserType", HttpUtility.UrlDecode(contextNameValueCollection["x-user-SupplierUserType"]));
            DataContext.SetContextItem("ParentSupplierSysNo", HttpUtility.UrlDecode(contextNameValueCollection["x-user-ParentSupplierSysNo"]));
            DataContext.SetContextItem("SystemUserType", HttpUtility.UrlDecode(contextNameValueCollection["x-user-SystemUserType"]));

            if (contextNameValueCollection["x-rpc-forcewritedb"] != null
                && string.Equals(contextNameValueCollection["x-rpc-forcewritedb"], "true", StringComparison.CurrentCultureIgnoreCase))
            {
                DataContext.SetContextItem("ForceWriteDB", contextNameValueCollection["x-rpc-forcewritedb"]);
            }

            if (contextNameValueCollection["x-real-ip"] != null)
            {
                DataContext.SetContextItem("RealIP", contextNameValueCollection["x-real-ip"]);
            }

            return null;
        }

        private void CompletedProcess(JsonRequest jsonRequest, JsonResponse jsonResponse, object context)
        {
            try
            {
                DateTime endTime = DateTime.Now;
                //DataContext.SetContextItem("X-End-Time", endTime);

                // 业务日志，记录服务具体的业务行为。
                OptLogger.Log(jsonResponse, jsonRequest);

                // 服务跟踪日志，记录时间、性能、异常。
                if (ConfigurationManager.AppSettings["ServiceTrace-Enabled"] == "true")
                {
                    DateTime startTime = (DateTime)DataContext.GetContextItem("X-Start-Time");
                    ServiceTraceLogger.LogAsync(jsonResponse, jsonRequest, startTime, endTime);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (context.GetType() == typeof(HttpRequest))
                {
                    DataContext.RemoveContext();
                }
            }
        }


        #endregion

        #region JsonRpc Client Write Context

        public static void InjectServiceToClient()
        {
            Rpc.AddGlobalContextSetHandler(
                 (NameValueCollection nameValueCollection) =>
                 {
                     nameValueCollection["x-user-UserSysNo"] = DataContextToClientValue("UserSysNo");
                     nameValueCollection["x-user-UserID"] = DataContextToClientValue("UserID");
                     nameValueCollection["x-user-UserDisplayName"] = DataContextToClientValue("UserDisplayName");
                     nameValueCollection["x-user-ApplicationKey"] = DataContextToClientValue("ApplicationKey");
                     //nameValueCollection["x-user-DataPermission"]                 = DataContextToClientValue("DataPermission");
                     nameValueCollection["x-user-LoginTime"] = DataContextToClientValue("LoginTime");
                     nameValueCollection["x-user-SupplierSysNo"] = DataContextToClientValue("SupplierSysNo");
                     nameValueCollection["x-user-SupplierUserType"] = DataContextToClientValue("SupplierUserType");
                     nameValueCollection["x-user-ParentSupplierSysNo"] = DataContextToClientValue("ParentSupplierSysNo");
                     nameValueCollection["x-user-SystemUserType"] = DataContextToClientValue("SystemUserType");

                     if (DataContext.GetContextItem("RealIP") != null)
                     {
                         nameValueCollection["x-real-ip"] = DataContext.GetContextItem("RealIP").ToString();
                     }
                 });
        }

        public static string DataContextToClientValue(string key)
        {
            return EncodeValue(DataContext.GetContextItem(key));
        }

        public static string EncodeValue(object val)
        {
            if (val == null)
                return null;
            return HttpUtility.UrlEncode(val.ToString());
        }

        #endregion

    }
}  