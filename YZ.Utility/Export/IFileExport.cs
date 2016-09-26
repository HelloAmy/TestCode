using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    public interface IFileExport
    {

        /// <summary>
        /// 生成excel工作簿
        /// </summary>
        /// <param name="data">各个Sheet的源数据(DataTable类型)</param>
        /// <param name="columnSetting">各个Sheet的列头设置</param>
        /// <param name="textInfoSetting">表格文字信息设置（,如merge表头，列，等；默认样式请传值为null）</param>
        /// <param name="fileName">导出的excel文件路径</param>
        /// <param name="FileTitle">excel文件名称</param>
        /// <returns></returns>
        byte[] CreateFile(List<DataTable> data, List<List<ColumnData>> columnSetting, List<List<TextInfo>> textInfoList, out string fileName, string FileTitle);

        /// <summary>
        /// 生成excel工作簿
        /// </summary>
        /// <param name="dataList">各个Sheet的源数据(DataTable类型)</param>
        /// <param name="sheetNameList">各个Sheet的名称（string类型）</param>
        /// <param name="columnList">各个Sheet的源数据(DataTable类型)</param>
        /// <param name="textInfoList">表格文字信息设置（,如merge表头，列，等；默认样式请传值为null）</param>
        /// <param name="fileName">导出的excel文件路径</param>
        /// <param name="fileTitle">xcel文件名称</param>
        /// <returns></returns>
        byte[] CreateFile(List<DataTable> dataList, List<string> sheetNameList, List<List<ColumnData>> columnList, List<List<TextInfo>> textInfoList, out string fileName, string fileTitle,List<List<CellMergeRegion>> cellMergeRegionList=null );
    }
}
