using YZ.Utility.DataAccess.Entity; 
using System;
using System.Collections.Generic;
using System.Text;

namespace YZ.Utility.DataAccess.DbProvider
{
    internal static class DbFactoryManager
    {
        public static IDbFactory GetFactory(ProviderType providerType )
        { 
            switch (providerType)
            {
                case ProviderType.SqlServer:
                    return SqlServerFactory.Instance;       
                case ProviderType.MySql:
                    return MySqlFactory.Instance; 
                default:
                    return OleDbFactory.Instance;
            }
        }
    }

    
}
