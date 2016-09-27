using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using YZ.JsonRpc;
using YZ.Utility.EntityBasic;

namespace YZ.Mall.ServiceHost
{
    public class OptLogger
    {
        //public static Task AsyncLog(JsonResponse jsonResponse)
        //{
            //Thread
            //return new Task((object jsonRequest) =>
            //{
            //    Log(jsonResponse, jsonRequest as JsonRequest);
            //}, DataContext.GetContextItem("JsonRequest"));
            //Task.Factory.
            //return Task.StartNew(() =>
            //{
            //    Thread.Sleep(1000*10);
            //    Log(jsonResponse);
            //    Thread.Sleep(1000 * 10);
            //});
        //}

        public static void Log(JsonResponse jsonResponse, JsonRequest jsonRequest)
        {
            // 操作日志
            //
            if (jsonRequest == null)
                return;
            
            if (!jsonRequest.Method.Equals("MallCommonService.OperationLogService.WriteLog", StringComparison.InvariantCultureIgnoreCase))
            {
                JsonRpcOptLogConfigMethod methodConfig = null;
                if (OptLogConfig.Current.Methods.ContainsKey(jsonRequest.Method))
                    methodConfig = OptLogConfig.Current.Methods[jsonRequest.Method];

                string bizObjectType = string.Empty;
                string bizOperationType = string.Empty;
                string bizObjectName = string.Empty;
                string bizOperationName = string.Empty;
                string bizObjectId = string.Empty;
                string operationDesc = string.Empty;

                if (methodConfig != null)
                {
                    bizObjectType = methodConfig.bizObjectType;
                    bizOperationType = methodConfig.bizOperationType;
                    operationDesc = methodConfig.operationDesc;

                    if (OptLogConfig.Current.DictBizObjectTypes.ContainsKey(bizObjectType))
                        bizObjectName = OptLogConfig.Current.DictBizObjectTypes[bizObjectType];

                    if (OptLogConfig.Current.DictBizOperationTypes.ContainsKey(bizOperationType))
                        bizOperationName = OptLogConfig.Current.DictBizOperationTypes[bizOperationType];

                    var macthes = Regex.Matches(operationDesc, "{(.+?)}");
                    foreach (Match m in macthes)
                    {
                        var replaceValue = ConvertExp(m.Groups[1].Value, jsonRequest, jsonResponse);
                        operationDesc = operationDesc.Replace(m.Groups[0].Value, replaceValue);
                    }

                    bizObjectId = ConvertExp(methodConfig.bizObjectIdExpression, jsonRequest, jsonResponse);

                    YZ.JsonRpc.Client.Rpc.Call<dynamic>("MallCommonService.OperationLogService.WriteLog",
                        bizObjectType,
                        bizOperationType,
                        bizObjectName,
                        bizOperationName,
                        bizObjectId,
                        operationDesc,
                        jsonRequest,
                        jsonResponse);
                }
            }
        }

        /// <summary>
        /// 转换表达式中的值,
        /// </summary>
        /// <param name="jsonExp"></param>
        /// <param name="jsonRequest"></param>
        /// <param name="jsonResponse"></param>
        /// <returns></returns>
        private static string ConvertExp(string jsonExp, JsonRequest jsonRequest, JsonResponse jsonResponse)
        {
            if (string.IsNullOrWhiteSpace(jsonExp))
                return string.Empty;
            bool isGetResult = false;
            var jsonexpResultSymbol = "$result"; // //json表达式字符串中表示取result的关键字
            if (jsonExp.StartsWith(jsonexpResultSymbol))
            {
                isGetResult = true;
                jsonExp = jsonExp.Remove(0, jsonexpResultSymbol.Length);
                jsonExp = jsonExp.TrimStart('.');
            }

            var replaceValue = string.Empty;
            if (isGetResult)
            {
                if (jsonResponse.Result == null)
                    return string.Empty;
                JToken jToken = null;
                if (jsonResponse.Result.GetType().IsValueType)
                {
                    jToken = JValue.FromObject(jsonResponse.Result);
                }
                else
                {
                    jToken = JObject.FromObject(jsonResponse.Result);
                }
                replaceValue = jToken.SelectToken(jsonExp).ToString();
            }
            else
            {
                var jArray = jsonRequest.Params as JArray;
                if (jArray != null)
                    replaceValue = jArray.SelectToken(jsonExp).ToString();
            }
            return replaceValue;
        }

    }
}