using YZ.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace YZ.Utility
{
    /// <summary>
    /// 带有分页信息的查询结果Entity,所有带分页的查询Service返回类型都必须为此类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryResult<T> : QueryFilter
    {
        public int recordsTotal { get; set; }

        public int recordsFiltered { get { return recordsTotal; } }

        public List<T> data { get; set; }

        public string Summary { get; set; }

        public T CurrentPageTotalInfo { get; set; }

        public T AllPageTotalInfo { get; set; }

        public T FirstOrDefault()
        {
            if (data != null)
            {
                return data.FirstOrDefault();
            }

            return default(T);
        }
    }

    public class QueryResult : QueryFilter
    {
        public int recordsTotal { get; set; }
        public int recordsFiltered { get { return recordsTotal; } }
        public DataTable data { get; set; }
    }
}
