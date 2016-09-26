using YZ.Utility.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YZ.Utility.DataAccess.DbProvider
{
    public class DBConfigHelper
    {
        public static DBConfig ConfigSetting
        {
            get
            {
                return CacheManager.GetWithCache("YZ_DataAccess_DBConfig", _LoadConfig, CacheTime.Longest);
                //return _LoadConfig();
            }
        }

        private static object _obj = new object();
        private static DBConfig _LoadConfig()
        {
            lock (_obj)
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configuration\Data\DB.config");
                if (File.Exists(filePath))
                {
                    DBConfig config = SerializeHelper.LoadFromXml<DBConfig>(filePath);
                    foreach(DBConnection con in  config.DBConnectionList)
                    {
                        if(!string.IsNullOrWhiteSpace(con.IsEncrypt)
                            && (con.IsEncrypt.Trim().ToUpper()=="Y" ||con.IsEncrypt.Trim().ToUpper()=="YES"))
                        {
                            con.ConnectionString = CryptoManager.Decrypt(con.ConnectionString);
                           
                        }
                    }
                    return config;
                }
                else
                {
                    throw new Exception(string.Format("Not found sql file {0}", filePath));
                }
            }
        }
    }
}
