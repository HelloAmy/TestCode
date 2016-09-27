using System;
using System.Net;
using System.Threading.Tasks;
using YZ.JsonRpc.Client;
using YZ.Utility.EntityBasic;
using JsonRequest = YZ.JsonRpc.JsonRequest;
using JsonResponse = YZ.JsonRpc.JsonResponse;

namespace YZ.Mall.ServiceHost
{
    public class ServiceTraceLogger
    {
        public static void LogAsync(JsonResponse jsonResponse, JsonRequest jsonRequest, DateTime startTime, DateTime endTime)
        {
            Task.Factory.StartNew(() => Log(jsonResponse, jsonRequest, startTime, endTime), TaskCreationOptions.AttachedToParent);
        }

        const string ServiceTrace_MethodName = "CommonService.ServiceTraceService.InsertServiceTrace";

        public static void Log(JsonResponse jsonResponse, JsonRequest jsonRequest, DateTime startTime, DateTime endTime)
        {
            if (jsonRequest == null)
                return;

            if (string.Equals(jsonRequest.Method, ServiceTrace_MethodName, StringComparison.InvariantCultureIgnoreCase))
                return;

            string exception = null;
            if (jsonResponse.Error != null && jsonResponse.Error.code != 32000)
            {
                exception = Convert.ToString(jsonResponse.Error.data ?? jsonResponse.Error);

                if (exception.Length > 2000)
                    exception = exception.Substring(0, 2000);
            }

            dynamic entity = new
            {
                Method = jsonRequest.Method,
                ServerName = GetServerName(),
                ServerIP = GetServerIP(),
                StartTime = startTime,
                EndTime = endTime,
                DurationMs = Convert.ToInt32((endTime - startTime).TotalMilliseconds),
                Exception = exception
            };
            RpcOption rpcOption = new RpcOption();
            rpcOption.ServiceAddress = "local";
            Rpc.Call2<int>(ServiceTrace_MethodName, rpcOption, entity);
        }

        private static string serverName;
        private static string GetServerName()
        {
            if (string.IsNullOrEmpty(serverName))
            {
                serverName = Dns.GetHostName();
            }
            return serverName;
        }

        private static string serverIP;
        private static string GetServerIP()
        {
            if (string.IsNullOrEmpty(serverIP))
            {
                try
                {
                    IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    if (address != null)
                    {
                        foreach (IPAddress addr in address)
                        {
                            if (addr == null)
                            {
                                continue;
                            }
                            string tmp = addr.ToString().Trim();
                            //过滤IPv6的地址信息
                            if (tmp.Length <= 16 && tmp.Length > 5)
                            {
                                serverIP = tmp;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    //s_ServerIP = string.Empty;
                }
            }
            if (string.IsNullOrEmpty(serverIP))
            {
                return string.Empty;
            }
            return serverIP;
        }
    }
}