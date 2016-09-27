using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using YZ.JsonRpc.Client;
using YZ.Utility;
using JsonRequest = YZ.JsonRpc.JsonRequest;
using JsonResponse = YZ.JsonRpc.JsonResponse;

namespace YZ.Mall.ServiceHost
{
    public class JsonRpcInject
    {
        public void PreProcess(JsonRequest rpc, object context)
        {
        }

        public void CompletedProcess(JsonRequest jsonRequest, JsonResponse jsonResponse)
        {
            var profiles = FindInjectProfiles(jsonRequest.Method);
            if (profiles != null && profiles.Count > 0)
            {
                foreach (InjectProfile profile in profiles)
                {
                    Task.Factory.StartNew(
                        CallHandleService, new
                        {
                            JsonRequest = jsonRequest,
                            JsonResponse = jsonResponse,
                            Profile = profile
                        }
                        );
                }
            }
        }

        private void CallHandleService(dynamic state)
        {
            try
            {
                JsonRequest jsonRequest = state.JsonRequest;
                JsonResponse jsonResponse = state.JsonResponse;
                InjectProfile profile = state.Profile;

                var serviceParams = new List<object>();
                if (profile.ParamExprs != null)
                {
                    foreach (var param in profile.ParamExprs)
                        serviceParams.Add(JsonRpcExpr.ConvertExprToJToken(param.Expr, jsonRequest, jsonResponse));
                }
                Rpc.Call<dynamic>(profile.HandleMethod, serviceParams.ToArray());
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex.ToString());
            }
        }

        private List<InjectProfile> FindInjectProfiles(string method)
        {
            List<InjectProfile> profiles = new List<InjectProfile>();

            if (InjectConfig.Current != null && InjectConfig.Current.Groups != null)
            {
                foreach (InjectGroup group in InjectConfig.Current.Groups)
                {
                    profiles.AddRange(
                        group.Profiles.FindAll(
                            m => string.Equals(m.InjectMethod, method, StringComparison.InvariantCultureIgnoreCase)));
                }

            }

            return profiles;
        }
    }
}