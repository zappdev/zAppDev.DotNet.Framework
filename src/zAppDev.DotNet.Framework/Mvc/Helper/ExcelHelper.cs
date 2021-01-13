using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Mvc.Helper
{
    public class ExcelCellStyle
    {
        public bool Bold { get; set; }
        public bool Merge { get; set; }
        public string BackgroundColor { get; set; }
        public ExcelHorizontalAlignment? HorizontalAlignment { get; set; }
    }
    public class ExcelHelper
    {
        private readonly ExcelWorksheet _worksheet;
        public ExcelHelper(ExcelPackage package)
        {
            _worksheet = package.Workbook.Worksheets[0];
        }

        ExcelRange GetCell(int row, int column)
        {
            return _worksheet.Cells[row, column];
        }

        ExcelRange GetCells(int row, int column, int count)
        {
            return _worksheet.Cells[row, column, row, count];
        }

        ExcelRange GetRowAsCells(int row)
        {
            var maximumColumn = 0;
            foreach(var cell in _worksheet.Cells)
            {
                if (cell.End.Column > maximumColumn)
                    maximumColumn = cell.End.Column;
            }
            var lastColumn = maximumColumn;// _worksheet.Cells.Last().End.Column;
            return _worksheet.Cells[row, 1, row, lastColumn];
        }

        void StyleCells(ExcelRange cells, ExcelCellStyle style)
        {
            if (style == null) return;
            cells.Merge = style.Merge;
            cells.Style.Font.Bold = style.Bold;
            if (!string.IsNullOrWhiteSpace(style.BackgroundColor))
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(ExportOptionsV2.GetColorFromRGB(style.BackgroundColor));
            }
            if (style.HorizontalAlignment.HasValue)
            {
                cells.Style.HorizontalAlignment = style.HorizontalAlignment.Value;
            }
            
        }

        public void AddRowOnTop(string value, ExcelCellStyle style = null)
        {
            _worksheet.InsertRow(1, 1);
            _worksheet.Cells[1, 1].Value = value;
            if(style != null)
            {
                var row = GetRowAsCells(1);
                StyleCells(row, style);
                if (!row.Merge) row.Merge = true;
            }
            
        }

        public void AddRowAtBottom(string value, ExcelCellStyle style = null)
        {
            var lastRow = _worksheet.Cells.Last().End.Row + 1;
            _worksheet.InsertRow(lastRow, 1);
            _worksheet.Cells[lastRow, 1].Value = value;

            var row = GetRowAsCells(lastRow);
            StyleCells(row, style);
            if (!row.Merge) row.Merge = true;
        }
    }
}
