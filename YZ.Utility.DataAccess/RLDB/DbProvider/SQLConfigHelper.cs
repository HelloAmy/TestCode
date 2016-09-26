using YZ.Utility.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YZ.Utility.DataAccess.DbProvider
{
    public class SQLConfigHelper
    {
        
        public static List<SQL> GetSQLList()
        {
            return CacheManager.GetWithCache("YZ_DataAccess_SQLConfig", _LoadConfig, CacheTime.Longest);
        }

        private static object _obj = new object();
        private static object _obj2 = new object(); 

        private static List<SQL> _LoadConfig()
        {
            List<SQL> list = new List<SQL>();
            Regex regex = new Regex(@"@\w*", RegexOptions.IgnoreCase);

            DBConfig dbConfig = DBConfigHelper.ConfigSetting;
            if (dbConfig != null && dbConfig.SQLFileList != null)
            {
                lock (_obj)
                {
                    foreach (string file in dbConfig.SQLFileList)
                    {
                        lock (_obj2)
                        {
                            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration\Data", file);
                            if (File.Exists(filePath))
                            {
                                SQLConfig sqlConfig = SerializeHelper.LoadFromXml<SQLConfig>(filePath);
                                if (sqlConfig.SQLList != null)
                                {
                                    foreach (SQL sql in sqlConfig.SQLList)
                                    {
                                        sql.ParameterNameList = new List<string>();

                                        MatchCollection matches = regex.Matches(sql.Text.Trim());
                                        if (matches != null && matches.Count > 0)
                                        {
                                            foreach (Match match in matches)
                                            {
                                                if (!sql.ParameterNameList.Exists(f => f.Trim().ToLower() == match.Value.Trim().ToLower()))
                                                {
                                                    sql.ParameterNameList.Add(match.Value);
                                                }
                                            }
                                        }

                                        if (sql.TimeOut == 0)
                                        {
                                            DBConnection conn = DBConfigHelper.ConfigSetting.DBConnectionList.Find(f => f.Key.ToLower().Trim() == sql.ConnectionKey.ToLower().Trim());
                                            if (conn != null)
                                            {
                                                sql.TimeOut = conn.TimeOut;
                                            }
                                            else
                                            {
                                                sql.TimeOut = 180;
                                            }

                                        }
                                    }
                                    list.AddRange(sqlConfig.SQLList);
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("Not found sql file {0}", filePath));
                            }
                        }
                    }
                }
            }

            return list;

        }
    }
}
