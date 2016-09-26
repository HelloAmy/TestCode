using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using YZ.Utility.DataAccess.DbProvider;
using YZ.Utility.DataAccess.Entity;

namespace YZ.Utility.DataAccess.DbProvider
{
    public static class DBConnectionRouteHelper
    {

        /// <summary>
        /// 获取路由连接Keys
        /// </summary>
        /// <param name="connKey">原始连接字符串Key</param>
        /// <param name="splitDbCodeString">如：01或01,02或*</param>
        /// <returns></returns>
        public static string[] GetRouteConnectionKeyList(string connKey, string splitDbCodeString)
        {
            string[] splitDbCodes;

            if (string.IsNullOrWhiteSpace(splitDbCodeString))
                return new[] { GetRouteConnectionKey(connKey) };

            if (splitDbCodeString == "*")
            {
                var routeConnection =
                    DBConfigHelper.ConfigSetting.DBConnectionRoute.RouteConnections.Find(
                        r => string.Equals(r.Key, connKey, StringComparison.InvariantCultureIgnoreCase));

                splitDbCodes = routeConnection.Nodes.GroupBy(n => n.SplitDbCode).Select(n => n.Key).ToArray();
            }
            else
            {
                splitDbCodes = splitDbCodeString.Split(',');
            }

            string[] resultKeys = new string[splitDbCodes.Length];

            for (int i = 0; i < resultKeys.Length; i++)
                resultKeys[i] = GetRouteConnectionKey(connKey, splitDbCodes[i]);

            return resultKeys;
        }

        public static string GetRouteConnectionKey(string connKey, string splitDbCode = null)
        {
            if (DBConfigHelper.ConfigSetting.DBConnectionRoute == null 
                || DBConfigHelper.ConfigSetting.DBConnectionRoute.RouteConnections == null)
                return connKey;

            var routeConnection =
                DBConfigHelper.ConfigSetting.DBConnectionRoute.RouteConnections.Find(
                    r => string.Equals(r.Key, connKey, StringComparison.InvariantCultureIgnoreCase));

            if (routeConnection == null)
                return connKey;

            List<DBConnectionRoute.Node> balanceNodes = null;
            if (string.IsNullOrWhiteSpace(splitDbCode))
                balanceNodes = routeConnection.Nodes.FindAll(n => string.IsNullOrWhiteSpace(n.SplitDbCode)).ToList();
            else
                balanceNodes = routeConnection.Nodes.FindAll(n => !string.IsNullOrWhiteSpace(n.SplitDbCode) && n.SplitDbCode == splitDbCode).ToList();

            // 通过权重计算随机分配
            Hashtable ht = new Hashtable();
            int totalWeight = 0;
            foreach (DBConnectionRoute.Node balanceNode in balanceNodes)
            {
                int weight = (int)(balanceNode.BalanceWeight * 100);

                ht[balanceNode] = new int[] { totalWeight + 1, totalWeight + weight };
                totalWeight = totalWeight + weight;
            }

            Random rd = new Random();
            var rdVal = rd.Next(1, totalWeight + 1);

            DBConnectionRoute.Node finalNode = (from DictionaryEntry entry in ht
                                                let range = entry.Value as int[]
                                                where range != null && (rdVal >= range[0] && rdVal <= range[1])
                                                select (DBConnectionRoute.Node)entry.Key).FirstOrDefault();

            ht.Clear();
            ht = null;

            //debug
            if (finalNode == null)
                throw new Exception(string.Format("Not find DB connection route for {0}, SplitDbCode:{1}", connKey, splitDbCode));

            if (!string.IsNullOrWhiteSpace(finalNode.Key))
                return finalNode.Key;
            else
                return connKey + "_ROUTE_" + routeConnection.Nodes.IndexOf(finalNode);
        }
    }
}