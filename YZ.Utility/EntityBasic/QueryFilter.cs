using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace YZ.Utility
{
    public class QueryFilter
    {
        public QueryFilter()
        {
            this.PageIndex = 0;
            this.PageSize = 10;
        }

        /// <summary>
        ///  数据权限控制指定的授权码
        /// </summary>
        public string DataPermissionAuthKey { get; set; }
        /// <summary>
        /// 是否不启用数据权限控制
        /// </summary>
        public bool PassDataPermissionAuth { get; set; }
        /// <summary>
        /// 是否不启用过滤已删除数据
        /// </summary>
        public bool PassFilterDeletedStatus { get; set; }

        /// <summary>
        /// 是否只查本级及上级的数据
        /// </summary>
        public bool OnlyTopAndSelf { get; set; }

        /// <summary>
        /// 数据权限控制指定的组织结构代码
        /// </summary>
        public string LimitThisCompanyCode { get; set; }

        /// <summary>
        /// 数据权限控制指定的品类SysNo
        /// </summary>
        public List<int> LimitThisCategories { get; set; }

        /// <summary>
        /// 是启用 Solr 查询
        /// </summary>
        public bool EnableSolrQuery { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortFields { get; set; }

        public int draw { get; set; }

        /// <summary>
        /// 替换SortFileds中的别名。
        ///     如：SysNo DESC可替换成 t.SysNo DESC
        ///         Name ASC, SysNo DESC可替换成 t1.Name ASC, t2.SysNo DESC 
        /// </summary>
        /// <param name="mapping">传入字段的名称，及替换后名称，如果要为每个字段都指定默认别名前缀，
        /// 可加入一个默认别名，Key设为string.Empty，Value设为表别名.
        /// Excample:
        /// var sortFiledMapping = new Dictionary&lt;string, string&gt;();
        /// sortFiledMapping.Add("", "Contract");
        /// sortFiledMapping.Add("TenderName", "TenderBasicInfo.Name");
        /// filter.UseSortFieldsAlias(sortFiledMapping);
        /// </param>
        public void UseSortFieldsAlias(Dictionary<string, string> mapping)
        {
            StringBuilder newSortFileds = new StringBuilder();

            string sortFields = this.SortFields;
            if (string.IsNullOrWhiteSpace(sortFields))
                return;

            string[] fieldUnits = sortFields.Split(',');
            string defaultAlias = mapping[""];

            for (int i = 0; i < fieldUnits.Length; i++)
            {
                string fieldUnit = fieldUnits[i].Trim();
                string[] fieldNameAndOrder = fieldUnit.Split(' ');
                if (fieldNameAndOrder.Length == 0)
                    continue;
                string filedName = fieldNameAndOrder[0].Trim();

                var m = mapping.Where(a => a.Key.ToLower() == filedName.ToLower()).ToList();
                if (m.Count == 0)
                {
                    if (newSortFileds.Length > 0)
                        newSortFileds.Append(", ");

                    if (!string.IsNullOrWhiteSpace(defaultAlias) && !filedName.Contains("."))
                        newSortFileds.Append(defaultAlias + "." + fieldUnit);
                    else
                        newSortFileds.Append(fieldUnit);

                    break;
                }

                if (newSortFileds.Length > 0)
                    newSortFileds.Append(", ");
                newSortFileds.Append(m[0].Value);

                for (int j = 1; j < fieldNameAndOrder.Length; j++)
                {
                    if (!string.IsNullOrWhiteSpace(fieldNameAndOrder[j]))
                    {
                        newSortFileds.Append(" " + fieldNameAndOrder[j]);
                        break;
                    }
                }
            }

            this.SortFields = newSortFileds.ToString();
        }
    }
}
