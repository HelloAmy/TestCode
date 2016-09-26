using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace YZ.Utility.Export
{
    /// <summary>
    /// 由于MyXls不支持列超过255，所以使用NPOI来处理列超过255的情况
    /// </summary>
    public class NPOIExcelFileExporter
    {
        private const int MaxDataTableRowCountLimit = 65535;
        public byte[] ExportXlsToDownload(DataTable dt, string sheetName, List<ColumnData> columnList,
            out string fileName, List<CellMergeRegion> cellMergeRegionList = null)
        {
            if (dt != null && dt.Rows != null
                   && dt.Rows.Count > MaxDataTableRowCountLimit)
            {
                string msg = string.Format("对不起，导出失败！本次导出所查询出的结果记录条数超过了允许导出的最大记录条数（{0}），请进一步设置查询条件，以缩小所查出的记录的条数！", MaxDataTableRowCountLimit);
                throw BuildBizException(msg);
            }
            fileName = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_ffff") + ".xlsx";
            var file = BuildSheet(dt, sheetName, columnList, cellMergeRegionList);
            return file.ToArray();
        }
         
        /// <summary>
        /// 组装数据
        /// </summary>
        /// <param name="dt">column content</param>
        /// <param name="sheetName">datasheet name</param>
        /// <param name="columns">column header</param>
        /// <param name="cellMergeRegions">column header merge region</param>
        /// <returns></returns>
        private MemoryStream BuildSheet(DataTable dt, string sheetName, List<ColumnData> columns,
            List<CellMergeRegion> cellMergeRegions)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                MemoryStream ms = new MemoryStream();
                var sheet = workbook.CreateSheet(sheetName);
                var headerRow = sheet.CreateRow(0);
                // handling header.
                var i = 0;
                var columnsCount = columns.Count;
                foreach (var column in columns)
                {
                    headerRow.CreateCell(i).SetCellValue(column.Title);
                    //add style
                    if (columnsCount < 30)
                    {
                        headerRow.Cells[i].CellStyle = GetCellStyle(workbook, null, null,
                            FillPattern.NoFill, null,
                            HorizontalAlignment.Center,
                            VerticalAlignment.Bottom);
                    }
                    i++;
                }
                //merge header
                foreach (var cellMergeResion in cellMergeRegions)
                {
                    sheet.AddMergedRegion(new CellRangeAddress(cellMergeResion.RowMin - 1, cellMergeResion.RowMax - 1,
                        cellMergeResion.ColMin - 1, cellMergeResion.ColMax - 1));
                }

                // handling value.
                int rowIndex = 1;
                foreach (DataRow row in dt.Rows)
                {
                    var dataRow = sheet.CreateRow(rowIndex);
                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = dataRow.CreateCell(column.Ordinal);
                        cell.SetCellValue(new XSSFRichTextString(row[column].ToString()));
                        //add style
                        if (columnsCount < 30)
                        {
                            cell.CellStyle = GetCellStyle(workbook, null, null,
                                FillPattern.NoFill, null,
                                rowIndex == 1 ? HorizontalAlignment.Center : HorizontalAlignment.Right,
                                VerticalAlignment.Bottom);
                        }
                    }
                    rowIndex++;
                }
                workbook.Write(ms);
                return ms;
            }
            catch (Exception ex)
            {
                var msg = string.Format("导出出错:{0}", ex.StackTrace.ToString());
                throw BuildBizException(msg);
            }

        }

        /// <summary>
        /// 获取单元格样式
        /// </summary>
        /// <param name="workbook">Excel操作类</param>
        /// <param name="font">单元格字体</param>
        /// <param name="fillForegroundColor">图案的颜色</param>
        /// <param name="fillPattern">图案样式</param>
        /// <param name="fillBackgroundColor">单元格背景</param>
        /// <param name="ha">垂直对齐方式</param>
        /// <param name="va">垂直对齐方式</param>
        /// <returns></returns>
        private static ICellStyle GetCellStyle(IWorkbook workbook, IFont font, XSSFColor fillForegroundColor,
            FillPattern fillPattern, XSSFColor fillBackgroundColor, HorizontalAlignment ha, VerticalAlignment va)
        {
            ICellStyle cellstyle = workbook.CreateCellStyle();
            cellstyle.FillPattern = fillPattern;
            cellstyle.Alignment = ha;
            cellstyle.VerticalAlignment = va;
            if (fillForegroundColor != null)
            {
                cellstyle.FillForegroundColor = fillForegroundColor.Indexed;
               
            }
            if (fillBackgroundColor != null)
            {
                cellstyle.FillBackgroundColor = fillBackgroundColor.Indexed;
            }
            if (font != null)
            {
                cellstyle.SetFont(font);
            }
            //有边框
            cellstyle.BorderBottom = BorderStyle.Thin;
            cellstyle.BorderLeft = BorderStyle.Thin;
            cellstyle.BorderRight = BorderStyle.Thin;
            cellstyle.BorderTop = BorderStyle.Thin;
            cellstyle.BottomBorderColor = HSSFColor.Black.Index;
            cellstyle.LeftBorderColor = HSSFColor.Black.Index;
            cellstyle.RightBorderColor = HSSFColor.Black.Index;
            cellstyle.TopBorderColor = HSSFColor.Black.Index;
            return cellstyle;
        }

        private Exception BuildBizException(string msg)
        {
            Type type = Type.GetType("YZ.Utility.BusinessException, YZ.Utility");
            return (Exception)Activator.CreateInstance(type, msg);
        }
    }
}
