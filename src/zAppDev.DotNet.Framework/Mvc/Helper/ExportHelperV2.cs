using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

using System.ComponentModel;

using UnitType = MigraDoc.DocumentObjectModel.UnitType;

#if NETFRAMEWORK
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace zAppDev.DotNet.Framework.Mvc
{
   
    public class ExportOptionsV2
    {
        public ExportHelper.Type Type { get; set; }
        public ExportHelper.Range Range { get; set; }
        public string Filename { get; set; }
        public string ExportTitle { get; set; }
        public bool IncludeGridLines { get; set; }
        public bool PortraitOrientation { get; set; }
        public List<ColumnOptionsV2> ColumnInfo { get; set; }
        public List<string> GroupInfo { get; set; }
        public string GroupColor { get; set; }
        public string HeaderColor { get; set; }
        public string EvenColor { get; set; }
        public string OddColor { get; set; }
        public string AggregateColor { get; set; }

        public static System.Drawing.Color GetColorFromRGB(string rgb)
        {
            //Default color
            var color = System.Drawing.Color.Transparent;
            var rgbRegex = new Regex("[0-9\\.]+", RegexOptions.Compiled);
            var result = rgbRegex.Matches(rgb);
            if (result.Count == 3)
            {
                color = System.Drawing.Color.FromArgb(Convert.ToInt16(result[0].Value), Convert.ToInt16(result[1].Value), Convert.ToInt16(result[2].Value));
            }

            return color;
        }

        public static ExportOptionsV2 GetDefault(string exportMode, string fileName)
        {
            var defaultV1 = ExportHelper.ExportOptions.GetDefault(exportMode, fileName);

            return new ExportOptionsV2
            {
                Range = defaultV1.Range,
                Type = defaultV1.Type,
                Filename = defaultV1.Filename,
                ExportTitle = "",
                IncludeGridLines = defaultV1.IncludeGridLines,
                GroupColor = defaultV1.GroupColor,
                HeaderColor = defaultV1.HeaderColor,
                EvenColor = defaultV1.EvenColor,
                OddColor = defaultV1.OddColor,
                AggregateColor = defaultV1.AggregateColor,
            };
        }
    }

    public class ColumnOptionsV2
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Formatting { get; set; }
        public string ExcelFormat { get; set; }
        public int Width { get; set; }
    }

    public class ExportHelperV2<T>
    {
        public readonly ExportOptionsV2 Options;
        /*****COLORS START*****/
        private System.Drawing.Color? _headerSystemColor;
        private MigraDoc.DocumentObjectModel.Color? _headerMigraDocColor;
        public System.Drawing.Color HeaderSystemColor
        {
            get
            {
                if (_headerSystemColor == null)
                    _headerSystemColor = ExportOptionsV2.GetColorFromRGB(Options.HeaderColor);
                return _headerSystemColor.Value;
            }
        }
        public MigraDoc.DocumentObjectModel.Color HeaderMigraDocColor
        {
            get
            {
                if (_headerMigraDocColor == null)
                    _headerMigraDocColor = new MigraDoc.DocumentObjectModel.Color(HeaderSystemColor.A, HeaderSystemColor.R, HeaderSystemColor.G, HeaderSystemColor.B);
                
                return _headerMigraDocColor.Value;
            }
        }

        private System.Drawing.Color? _aggregatorSystemColor;
        private MigraDoc.DocumentObjectModel.Color? _aggregatorMigraDocColor;
        public System.Drawing.Color AggregatorSystemColor
        {
            get
            {
                if (_aggregatorSystemColor == null)
                    _aggregatorSystemColor = ExportOptionsV2.GetColorFromRGB(Options.AggregateColor);
                return _aggregatorSystemColor.Value;
            }
        }
        public MigraDoc.DocumentObjectModel.Color AggregatorMigraDocColor
        {
            get
            {
                if (_aggregatorMigraDocColor == null)
                    _aggregatorMigraDocColor = new MigraDoc.DocumentObjectModel.Color(AggregatorSystemColor.A, AggregatorSystemColor.R, AggregatorSystemColor.G, AggregatorSystemColor.B);

                return _aggregatorMigraDocColor.Value;
            }
        }

        private System.Drawing.Color? _evenSystemColor;
        private MigraDoc.DocumentObjectModel.Color? _evenMigraDocColor;
        public System.Drawing.Color EvenSystemColor
        {
            get
            {
                if (_evenSystemColor == null)
                    _evenSystemColor = ExportOptionsV2.GetColorFromRGB(Options.EvenColor);
                return _evenSystemColor.Value;
            }
        }
        public MigraDoc.DocumentObjectModel.Color EvenMigraDocColor
        {
            get
            {
                if (_evenMigraDocColor == null)
                    _evenMigraDocColor = new MigraDoc.DocumentObjectModel.Color(EvenSystemColor.A, EvenSystemColor.R, EvenSystemColor.G, EvenSystemColor.B);

                return _evenMigraDocColor.Value;
            }
        }

        private System.Drawing.Color? _oddSystemColor;
        private MigraDoc.DocumentObjectModel.Color? _oddMigraDocColor;
        public System.Drawing.Color OddSystemColor
        {
            get
            {
                if (_oddSystemColor == null)
                    _oddSystemColor = ExportOptionsV2.GetColorFromRGB(Options.OddColor);
                return _oddSystemColor.Value;
            }
        }
        public MigraDoc.DocumentObjectModel.Color OddMigraDocColor
        {
            get
            {
                if (_oddMigraDocColor == null)
                    _oddMigraDocColor = new MigraDoc.DocumentObjectModel.Color(OddSystemColor.A, OddSystemColor.R, OddSystemColor.G, OddSystemColor.B);

                return _oddMigraDocColor.Value;
            }
        }
        /*****COLORS END*****/
        private int _totalColumns;
        private bool _groupItemColorSwitch;
        private readonly Dictionary<string, string> _aggregatorResources = new Dictionary<string, string> {
            { "COUNT", "RES_DATALIST_EXPORTV2_Aggregator_Count" },
            { "SUM", "RES_DATALIST_EXPORTV2_Aggregator_Sum" },
            { "AVERAGE", "RES_DATALIST_EXPORTV2_Aggregator_Average" },
            { "TOTAL_COUNT", "RES_DATALIST_EXPORTV2_Aggregator_TotalCount" },
            { "TOTAL_SUM", "RES_DATALIST_EXPORTV2_Aggregator_TotalSum" },
            { "TOTAL_AVERAGE", "RES_DATALIST_EXPORTV2_Aggregator_TotalAverage" }
        };
        private readonly Dictionary<string, Func<T, object>> _getters;
        private bool _writtenHeaders;

        //Excel specific params
        private readonly List<ExcelTableRange> _aggregatorExcelStyleRanges = new List<ExcelTableRange>();
        private class ExcelTableRange
        {
            public int FromRow { get; set; }
            public int FromColumn { get; set; }
            public int ToRow { get; set; }
            public int ToColumn { get; set; }
        }

        //PDF specific params
        private int _totalPDFColumnWidth;
        private TextMeasurement _PDFTextMeasurement;
        private MigraDoc.DocumentObjectModel.Unit _availablePDFWidthInMillimeter;
        private static readonly int AGGREGATOR_PDF_COLUMN_SIZE = 150;
        private static readonly int DEFAULT_PDF_COLUMN_SIZE = 150;


        public ExportHelperV2(ExportOptionsV2 options, Dictionary<string, Func<T, object>> getters)
        {
            Options = options;
            _getters = getters;
        }

#region Excel Exporting

        //Simple Export
        public string ExportToExcel(List<T> items, List<AggregatorInfo<T>> aggregators)
        {
            _totalColumns = Options.ColumnInfo.Count + (aggregators.Any() ? 1 : 0);
            _writtenHeaders = false;

            //Creates a blank workbook. Use the using statment, so the package is disposed when we are done.
            using (var p = new ExcelPackage())
            {
                //A workbook must have at least sheet cell, so lets add one... 
                var sheetName = !string.IsNullOrWhiteSpace(Options.ExportTitle) ? Options.ExportTitle : "Sheet1";
                var ws = p.Workbook.Worksheets.Add($"{sheetName} | Exported: {DateTime.Now.ToString("dd-MM-yyyy (HH-mm)")}");

                //Create header
                for (var i = 0; i < Options.ColumnInfo.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = Options.ColumnInfo[i].Caption;
                }
                if (aggregators.Any())
                {
                    ws.Cells[1, _totalColumns].Value = BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_EXPORTV2_Totals");
                }
                _writtenHeaders = true;

                SetExcelHeaderStyle(ws);

                for (var i = 0; i < items.Count; i++)
                {
                    for (int j = 0; j < Options.ColumnInfo.Count; j++)
                    {
                        var cellValue = GetCellValue(items[i], Options.ColumnInfo[j], false);
                        if (string.IsNullOrWhiteSpace(cellValue?.ToString())) continue;

                        ws.Cells[i + 2, j + 1].Value = cellValue;
                        ApplyExcelFormatting(ws.Cells[i + 2, j + 1], cellValue, Options.ColumnInfo[j]);
                    }

                    //Set line style
                    var lineColor = (i + 1) % 2 == 0 ? EvenSystemColor : OddSystemColor;
                    ws.Cells[i + 2, 1, i + 2, Options.ColumnInfo.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[i + 2, 1, i + 2, Options.ColumnInfo.Count].Style.Fill.BackgroundColor.SetColor(lineColor);
                }

                var currentLine = items.Count + 2;

                if (aggregators.Any())
                {
                    foreach (AggregatorType aggregatorType in Enum.GetValues(typeof(AggregatorType)))
                    {
                        var currentTypeAggregators = aggregators.Where(a => a.Type == aggregatorType);
                        if (!currentTypeAggregators.Any()) continue;

                        for (int i = 0; i < Options.ColumnInfo.Count; i++)
                        {
                            var aggregatorInfo = currentTypeAggregators.FirstOrDefault(a => a.Column == Options.ColumnInfo[i].Name);
                            if (aggregatorInfo == null) continue;

                            ws.Cells[currentLine, i + 1].Value = aggregatorInfo.Value;
                            //Count can be applied to any column type and the value is always int so we should skip the aggregator formatting in this case
                            if (aggregatorType != AggregatorType.COUNT)
                            {
                                ApplyExcelFormatting(ws.Cells[currentLine, i + 1], aggregatorInfo.Value, Options.ColumnInfo[i]);
                            }
                        }

                        ws.Cells[currentLine, Options.ColumnInfo.Count + 1].Value = BaseViewPageBase<object>.GetResourceValue("GlobalResources", _aggregatorResources[aggregatorType.ToString()]);

                        //Set aggregator style
                        ws.Cells[currentLine, 1, currentLine, _totalColumns].Style.Font.Bold = true;
                        ws.Cells[currentLine, 1, currentLine, _totalColumns].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentLine, 1, currentLine, _totalColumns].Style.Fill.BackgroundColor.SetColor(AggregatorSystemColor);

                        currentLine++;
                    }
                }

                var outputTable = ws.Cells[1, 1, --currentLine, _totalColumns];
                outputTable.AutoFitColumns(); //Autofit columns to contents
                SetExcelGridLines(ws, new ExcelTableRange { FromRow = 1, FromColumn = 1, ToRow = currentLine, ToColumn = _totalColumns }, Options.IncludeGridLines);

                return SaveExcelFile(p, Options.Filename);
            }
        }

        //Grouped Export
        public string ExportToExcel(GroupTree<T> groups, GroupTree<T> aggregators, Func<T, ActionResult> postProcessAction = null)
        {
            _totalColumns = Options.ColumnInfo.Count + (aggregators.Aggregates.Any(a => a.Column != "__Count") ? 1 : 0);
            _writtenHeaders = false;

            //Creates a blank workbook. Use the using statment, so the package is disposed when we are done.
            using (var p = new ExcelPackage())
            {
                //A workbook must have at least sheet cell, so lets add one... 
                var ws = p.Workbook.Worksheets.Add("Sheet1");
                //We skip one line in order to write the headers later on
                var currentLine = 2;

                currentLine = ParseSubGroupForExcel(ws, groups, aggregators, new List<string>(), currentLine, postProcessAction);

                var outputTable = ws.Cells[1, 1, --currentLine, _totalColumns];
                outputTable.AutoFitColumns(); //Autofit columns to contents
                SetExcelGridLines(ws, new ExcelTableRange { FromRow = 1, FromColumn = 1, ToRow = currentLine, ToColumn = _totalColumns }, Options.IncludeGridLines);

                return SaveExcelFile(p, Options.Filename);
            }
        }

        private int ParseSubGroupForExcel(ExcelWorksheet ws, GroupTree<T> group, GroupTree<T> aggregators, List<string> groupedColumns, int currentLine, Func<T, ActionResult> postProcessAction)
        {
            var groupDataOnly = false;
            var startingLine = currentLine;

            if (group.Key?.ToString() != "ROOT")
            {
                groupedColumns.Add(group.Column.Name.ToLower());
            }

            var groupColumnInfo = Options.ColumnInfo.SingleOrDefault(c => c.Name.ToLower() == group.Column.Name.ToLower());

            if (group.SubGroups.Any())
            {
                foreach (var subgroup in group.SubGroups)
                {
                    //The aggregators are NOT in the same order with the groups row data so we must select the proper subgroup by key
                    var subGroupAggregators = aggregators?.SubGroups?.SingleOrDefault(a => a.Key.Equals(subgroup.Key));

                    currentLine = ParseSubGroupForExcel(ws, subgroup, subGroupAggregators, groupedColumns, currentLine, postProcessAction);

                    //This only happens when "Group Data Only" option is enabled and no item lines are written
                    groupDataOnly = currentLine == startingLine;
                }

                if (group.Key?.ToString() != "ROOT")
                {
                    var endingLine = groupDataOnly ? startingLine : currentLine - 1;

                    ws.Cells[startingLine, groupedColumns.Count, endingLine, groupedColumns.Count].Value = group.Key;
                    ws.Cells[startingLine, groupedColumns.Count, endingLine, groupedColumns.Count].Merge = true;
                    ws.Cells[startingLine, groupedColumns.Count, endingLine, groupedColumns.Count].Style.Font.Bold = true;
                    ws.Cells[startingLine, groupedColumns.Count, endingLine, groupedColumns.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ApplyExcelFormatting(ws.Cells[startingLine, groupedColumns.Count, endingLine, groupedColumns.Count], group.Key, groupColumnInfo);

                    //We should increment the currentLine when only group data line is written
                    if (groupDataOnly) currentLine++;
                }
            }
            else
            {
                //Merge cells if needed
                if (group.Items.Count > 1)
                {
                    var mergeLastLineIndex = startingLine + group.Items.Count - 1;

                    ws.Cells[startingLine, groupedColumns.Count, mergeLastLineIndex, groupedColumns.Count].Value = group.Key;
                    ws.Cells[startingLine, groupedColumns.Count, mergeLastLineIndex, groupedColumns.Count].Merge = true;
                    ws.Cells[startingLine, groupedColumns.Count, mergeLastLineIndex, groupedColumns.Count].Style.Font.Bold = true;
                    ws.Cells[startingLine, groupedColumns.Count, mergeLastLineIndex, groupedColumns.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ApplyExcelFormatting(ws.Cells[startingLine, groupedColumns.Count, mergeLastLineIndex, groupedColumns.Count], group.Key, groupColumnInfo);
                }
                else
                {
                    ws.Cells[startingLine, groupedColumns.Count].Value = group.Key;
                    ws.Cells[startingLine, groupedColumns.Count].Style.Font.Bold = true;
                    ws.Cells[startingLine, groupedColumns.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ApplyExcelFormatting(ws.Cells[startingLine, groupedColumns.Count], group.Key, groupColumnInfo);

                    // This should happen only when get groups closed is selected
                    if (group.Items.Count == 0) currentLine++;
                }
            }

            var ungroupedColumns = Options.ColumnInfo.Where(c => !Options.GroupInfo.Contains(c.Name.ToLower())).ToList();

            // Write headers
            if (!_writtenHeaders)
            {
                for (var i = 0; i < groupedColumns.Count; i++)
                {
                    ws.Cells[1, i + 1].Value = Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == groupedColumns[i]?.ToLower())?.Caption;
                }
                for (var i = 0; i < ungroupedColumns.Count; i++)
                {
                    ws.Cells[1, groupedColumns.Count + i + 1].Value = Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == ungroupedColumns[i]?.Name.ToLower())?.Caption;
                }
                if (Options.ColumnInfo.Count < _totalColumns)
                {
                    ws.Cells[1, _totalColumns].Value = BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_EXPORTV2_Totals");
                }
                SetExcelHeaderStyle(ws);
                _writtenHeaders = true;
            }

            foreach (var item in group.Items)
            {
                postProcessAction?.Invoke(item);

                for (var i = 0; i < ungroupedColumns.Count; i++)
                {
                    var cellValue = GetCellValue(item, ungroupedColumns[i], false);
                    if (string.IsNullOrWhiteSpace(cellValue?.ToString())) continue;

                    ws.Cells[currentLine, groupedColumns.Count + i + 1].Value = cellValue;
                    ApplyExcelFormatting(ws.Cells[currentLine, groupedColumns.Count + i + 1], cellValue, ungroupedColumns[i]);
                }

                //Set line style
                var lineColor = _groupItemColorSwitch ? OddSystemColor : EvenSystemColor;
                _groupItemColorSwitch = !_groupItemColorSwitch;
                ws.Cells[currentLine, 1, currentLine, Options.ColumnInfo.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[currentLine, 1, currentLine, Options.ColumnInfo.Count].Style.Fill.BackgroundColor.SetColor(lineColor);
                currentLine++;
            }

            //Write aggregators
            if (aggregators != null && aggregators.Aggregates.Any(a => a.Column != "__Count"))
            {
                foreach (AggregatorType aggregatorType in Enum.GetValues(typeof(AggregatorType)))
                {
                    var isAggregatorWritten = false;
                    var currentTypeAggregators = aggregators.Aggregates.Where(a => a.Type == aggregatorType);
                    if (!currentTypeAggregators.Any()) continue;

                    
                    for (var i = 0; i < Options.GroupInfo.Count; i++)
                    {
                        var aggregatorInfo = currentTypeAggregators.FirstOrDefault(a => a.Column.ToLower() == Options.GroupInfo[i].ToLower());
                        if (aggregatorInfo == null) continue;
                        ws.Cells[currentLine, i + 1].Value = aggregatorInfo.Value;
                        //Count can be applied to any column type and the value is always int so we should skip the aggregator formatting in this case
                        if (aggregatorType != AggregatorType.COUNT)
                        {
                            ApplyExcelFormatting(ws.Cells[currentLine, i + 1], aggregatorInfo.Value, ungroupedColumns[i]);
                        }
                        isAggregatorWritten = true;
                    }

                    for (var i = 0; i < ungroupedColumns.Count; i++)
                    {
                        var aggregatorInfo = currentTypeAggregators.FirstOrDefault(a => a.Column.ToLower() == ungroupedColumns[i].Name.ToLower());
                        if (aggregatorInfo == null) continue;
                        ws.Cells[currentLine, Options.GroupInfo.Count + i + 1].Value = aggregatorInfo.Value;
                        //Count can be applied to any column type and the value is always int so we should skip the aggregator formatting in this case
                        if (aggregatorType != AggregatorType.COUNT)
                        {
                            ApplyExcelFormatting(ws.Cells[currentLine, Options.GroupInfo.Count + i + 1], aggregatorInfo.Value, ungroupedColumns[i]);
                        }
                        isAggregatorWritten = true;
                    }

                    if (isAggregatorWritten)
                    {
                        var displayAggregatorInfo = BaseViewPageBase<object>.GetResourceValue("GlobalResources", _aggregatorResources[(group.Key.ToString() == "ROOT" ? "TOTAL_" : "") + aggregatorType.ToString()]);
                        if (group.Key?.ToString() != "ROOT")
                        {
                            displayAggregatorInfo += $" ({Options.ColumnInfo.SingleOrDefault(c => c.Name == group.Column.Name)?.Caption}: {group.Key})";
                        }
                        ws.Cells[currentLine, _totalColumns].Value = displayAggregatorInfo;

                        //Set aggregator style
                        ws.Cells[currentLine, 1, currentLine, Options.ColumnInfo.Count].Style.Font.Bold = true;
                        ws.Cells[currentLine, 1, currentLine, _totalColumns].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentLine, 1, currentLine, _totalColumns].Style.Fill.BackgroundColor.SetColor(AggregatorSystemColor);

                        currentLine++;
                    }
                }
            }

            //Make the line between the current group and the next bold
            _aggregatorExcelStyleRanges.Add(new ExcelTableRange { FromRow = currentLine - 1, FromColumn = 1, ToRow = currentLine - 1, ToColumn = _totalColumns });

            //Remove the current group from the grouped columns list since the parameter is passed by reference
            if (groupedColumns.Count > 0)
            {
                groupedColumns.RemoveAt(groupedColumns.Count - 1);
            }

            return currentLine;
        }

        private void SetExcelHeaderStyle(ExcelWorksheet ws)
        {
            ws.Cells[1, 1, 1, _totalColumns].Style.Font.Bold = true;
            ws.Cells[1, 1, 1, _totalColumns].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, 1, 1, _totalColumns].Style.Fill.BackgroundColor.SetColor(HeaderSystemColor);
            ws.Cells[1, 1, 1, _totalColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[1, 1, 1, _totalColumns].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private void ApplyExcelFormatting(ExcelRange cells, object cellValue, ColumnOptionsV2 columnInfo)
        {
            var format = string.IsNullOrWhiteSpace(columnInfo.ExcelFormat) || cellValue is DateTime
                ? columnInfo.Formatting
                : columnInfo.ExcelFormat;

            if (!string.IsNullOrWhiteSpace(format) && (cellValue is double || cellValue is DateTime))
            {
                var formatting = cellValue is DateTime
                    ? zAppDev.DotNet.Framework.Utilities.Common.ConvertMomentFormat(format)
                    : format.Replace("'", "");

                cells.Style.Numberformat.Format = format;
            }
        }

        private void SetExcelGridLines(ExcelWorksheet ws, ExcelTableRange range, bool hasGridLines)
        {
            if (!hasGridLines) return;

            var table = ws.Cells[range.FromRow, range.FromColumn, range.ToRow, range.ToColumn];
            table.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            table.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            table.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            table.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            foreach (var aggregatorStyleRange in _aggregatorExcelStyleRanges)
            {
                ws.Cells[aggregatorStyleRange.FromRow, aggregatorStyleRange.FromColumn, aggregatorStyleRange.ToRow, aggregatorStyleRange.ToColumn].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
            }
        }

        private string SaveExcelFile(ExcelPackage package, string filename)
        {
            var randomFolderName = Guid.NewGuid().ToString();
            var randomPath = Path.Combine(Path.GetTempPath(), randomFolderName);
            if (Directory.Exists(randomPath)) Directory.Delete(randomPath, true);
            Directory.CreateDirectory(randomPath);
            var filePath = Path.Combine(randomPath, filename + ".xlsx");

            //Save the new workbook. We haven't specified the filename so use the Save as method.
            package.SaveAs(new FileInfo(filePath));
            return filePath;
        }

#endregion

#region PDF Exporting

        private string ExportToPDF(List<T> items, List<AggregatorInfo<T>> aggregators)
        {
            _totalColumns = Options.ColumnInfo.Count + (aggregators.Any() ? 1 : 0);
            _totalPDFColumnWidth = Options.ColumnInfo.Sum(c => c.Width) + (aggregators.Any() ? AGGREGATOR_PDF_COLUMN_SIZE : 0);
            var pdfDocument = InitializePDF();

            _writtenHeaders = false;

            var table = InitializePDFTable();

            //Before you can add a row, you must define the columns
            foreach (var columnInfo in Options.ColumnInfo)
            {
                //Calculate the column width analogy
                var columnWidth = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(columnInfo.Width *
                                                                                   _availablePDFWidthInMillimeter.Millimeter /
                                                                                   _totalPDFColumnWidth);
                table.AddColumn(columnWidth);
            }
            if (aggregators.Any())
            {
                //Calculate the column width analogy
                var columnWidth = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(AGGREGATOR_PDF_COLUMN_SIZE *
                                                                                   _availablePDFWidthInMillimeter.Millimeter /
                                                                                   _totalPDFColumnWidth);
                table.AddColumn(columnWidth);
            }
            _writtenHeaders = true;
            
            //Create the header of the table
            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
            headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
            headerRow.Format.Font.Bold = true;
            headerRow.Shading.Color = HeaderMigraDocColor;

            for (var i = 0; i < Options.ColumnInfo.Count; i++)
            {
                headerRow.Cells[i].AddParagraph(Options.ColumnInfo[i].Caption);
                SetParagraphFormatting(headerRow.Cells[i].Format);
            }
            if (aggregators.Any())
            {
                headerRow.Cells[_totalColumns - 1].AddParagraph(BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_EXPORTV2_Totals"));
                SetParagraphFormatting(headerRow.Cells[_totalColumns - 1].Format);
            }

            for (var i = 0; i < items.Count; i++)
            {
                var dataRow = table.AddRow();
                dataRow.Shading.Color = (i + 1) % 2 == 0 ? EvenMigraDocColor : OddMigraDocColor;
                dataRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                dataRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                dataRow.Format.Font.Size = 11;

                for (var j = 0; j < Options.ColumnInfo.Count; j++)
                {
                    var cellValue = GetCellValue(items[i], Options.ColumnInfo[j]);
                    if (string.IsNullOrWhiteSpace(cellValue?.ToString())) continue;
                    var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(dataRow.Cells[j], cellValue.ToString());

                    dataRow.Cells[j].AddParagraph(adjustedCellValue);
                    SetParagraphFormatting(dataRow.Cells[j].Format);
                }
            }

            if (aggregators.Any())
            {
                foreach (AggregatorType aggregatorType in Enum.GetValues(typeof(AggregatorType)))
                {
                    var currentTypeAggregators = aggregators.Where(a => a.Type == aggregatorType);
                    if (!currentTypeAggregators.Any()) continue;

                    var aggregatorRow = table.AddRow();

                    for (var i = 0; i < Options.ColumnInfo.Count; i++)
                    {
                        var aggregatorInfo = currentTypeAggregators.FirstOrDefault(a => a.Column == Options.ColumnInfo[i].Name);
                        if (aggregatorInfo == null) continue;

                        aggregatorRow.Cells[i].AddParagraph(aggregatorInfo.ValueFormatted);
                        SetParagraphFormatting(aggregatorRow.Cells[i].Format);
                    }

                    aggregatorRow.Cells[Options.ColumnInfo.Count].AddParagraph(BaseViewPageBase<object>.GetResourceValue("GlobalResources", _aggregatorResources[aggregatorType.ToString()]));
                    SetParagraphFormatting(aggregatorRow.Cells[Options.ColumnInfo.Count].Format);
                    aggregatorRow.Format.Font.Bold = true;
                    aggregatorRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                    aggregatorRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                    aggregatorRow.Shading.Color = AggregatorMigraDocColor;
                }
            }

            //Save PDF
            return SavePDFFile(pdfDocument, table, Options.Filename);
        }

        private string ExportToPDF(GroupTree<T> groups, GroupTree<T> aggregators, Func<T, ActionResult> postProcessAction)
        {
            _totalColumns = Options.ColumnInfo.Count + (aggregators.Aggregates.Any(a => a.Column != "__Count") ? 1 : 0);
            _totalPDFColumnWidth = Options.ColumnInfo.Sum(c => c.Width) + (aggregators.Aggregates.Any(a => a.Column != "__Count") ? AGGREGATOR_PDF_COLUMN_SIZE : 0);
            _writtenHeaders = false;

            var pdfDocument = InitializePDF();
            var table = InitializePDFTable();

            ParseSubGroupForPDF(table, groups, aggregators, new List<string>(), 1, postProcessAction);
            
            return SavePDFFile(pdfDocument, table, Options.Filename);
        }

        private int ParseSubGroupForPDF(MigraDoc.DocumentObjectModel.Tables.Table table, GroupTree<T> group, GroupTree<T> aggregators, List<string> groupedColumns, int currentLine, Func<T, ActionResult> postProcessAction)
        {
            var startingLine = currentLine;

            if (group.Key?.ToString() != "ROOT")
            {
                groupedColumns.Add(group.Column.Name.ToLower());
            }

            if (group.SubGroups.Any())
            {
                foreach (var subgroup in group.SubGroups)
                {
                    //The aggregators are NOT in the same order with the groups row data so we must select the proper subgroup by key
                    var subGroupAggregators = aggregators?.SubGroups?.SingleOrDefault(a => a.Key.Equals(subgroup.Key));

                    currentLine = ParseSubGroupForPDF(table, subgroup, subGroupAggregators, groupedColumns, currentLine, postProcessAction);
                }

                if (group.Key?.ToString() != "ROOT")
                {
                    var groupCell = table.Rows[startingLine].Cells[groupedColumns.Count - 1];
                    var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(groupCell, group.Key?.ToString() ?? "");

                    groupCell.AddParagraph(adjustedCellValue);
                    SetParagraphFormatting(groupCell.Format);
                    groupCell.Format.Font.Bold = true;
                    groupCell.MergeDown = currentLine - startingLine - 1;
                }
            }

            var ungroupedColumns = Options.ColumnInfo.Where(c => !Options.GroupInfo.Contains(c.Name.ToLower())).ToList();

            foreach (var item in group.Items)
            {
                if (!_writtenHeaders)
                {
                    foreach (var groupedColumn in groupedColumns)
                    {
                        //Calculate the column width analogy
                        var columnInfoWidth = Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == groupedColumn?.ToLower())?.Width ?? DEFAULT_PDF_COLUMN_SIZE;
                        var columnWidth = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(columnInfoWidth *
                                                                                           _availablePDFWidthInMillimeter.Millimeter /
                                                                                           _totalPDFColumnWidth);
                        table.AddColumn(columnWidth);
                    }
                    foreach (var ungroupedColumn in ungroupedColumns)
                    {
                        var columnInfoWidth = Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == ungroupedColumn?.Name.ToLower())?.Width ?? DEFAULT_PDF_COLUMN_SIZE;
                        var columnWidth = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(columnInfoWidth *
                                                                                           _availablePDFWidthInMillimeter.Millimeter /
                                                                                           _totalPDFColumnWidth);
                        table.AddColumn(columnWidth);
                    }
                    if (Options.ColumnInfo.Count < _totalColumns)
                    {
                        //Calculate the column width analogy
                        var columnWidth = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(AGGREGATOR_PDF_COLUMN_SIZE *
                                                                                           _availablePDFWidthInMillimeter.Millimeter /
                                                                                           _totalPDFColumnWidth);
                        table.AddColumn(columnWidth);
                    }

                    //Create the header of the table
                    var headerRow = table.AddRow();
                    headerRow.HeadingFormat = true;
                    headerRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                    headerRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                    headerRow.Format.Font.Bold = true;
                    headerRow.Shading.Color = HeaderMigraDocColor;

                    for (var i = 0; i < groupedColumns.Count; i++)
                    {
                        headerRow.Cells[i].AddParagraph(Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == groupedColumns[i]?.ToLower())?.Caption);
                        SetParagraphFormatting(headerRow.Cells[i].Format);
                    }
                    for (var i = 0; i < ungroupedColumns.Count; i++)
                    {
                        headerRow.Cells[groupedColumns.Count + i].AddParagraph(Options.ColumnInfo.SingleOrDefault(c => c?.Name?.ToLower() == ungroupedColumns[i]?.Name.ToLower())?.Caption);
                        SetParagraphFormatting(headerRow.Cells[groupedColumns.Count + i].Format);
                    }
                    if (Options.ColumnInfo.Count < _totalColumns)
                    {
                        headerRow.Cells[_totalColumns - 1].AddParagraph(BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_EXPORTV2_Totals"));
                        SetParagraphFormatting(headerRow.Cells[_totalColumns - 1].Format);
                    }

                    _writtenHeaders = true;
                }

                postProcessAction?.Invoke(item);
                var itemRow = table.AddRow();

                for (var i = 0; i < ungroupedColumns.Count; i++)
                {
                    var cellValue = GetCellValue(item, ungroupedColumns[i]);
                    if (string.IsNullOrWhiteSpace(cellValue?.ToString())) continue;

                    var currentCell = itemRow.Cells[groupedColumns.Count + i];
                    var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(currentCell, cellValue.ToString());

                    currentCell.AddParagraph(adjustedCellValue);
                    SetParagraphFormatting(currentCell.Format);
                }

                //Set line style
                itemRow.Shading.Color = _groupItemColorSwitch ? OddMigraDocColor : EvenMigraDocColor;
                itemRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                itemRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                _groupItemColorSwitch = !_groupItemColorSwitch;
                currentLine++;
            }

            if (!group.SubGroups.Any())
            {
                //Merge cells if needed
                if (group.Items.Count > 1)
                {
                    var mergeLastLineIndex = startingLine + group.Items.Count - 1 + (aggregators?.Aggregates?.Where(a => a.Column != "__Count")?.Select(a => a.Type)?.Distinct()?.Count() ?? 0);
                    var groupCell = table.Rows[startingLine].Cells[groupedColumns.Count - 1];
                    var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(groupCell, group.Key?.ToString() ?? "");

                    groupCell.AddParagraph(adjustedCellValue);
                    SetParagraphFormatting(groupCell.Format);
                    groupCell.Format.Font.Bold = true;
                    groupCell.MergeDown = mergeLastLineIndex - startingLine;
                }
                else
                {
                    var groupCell = table.Rows[startingLine].Cells[groupedColumns.Count - 1];
                    var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(groupCell, group.Key?.ToString() ?? "");

                    groupCell.AddParagraph(adjustedCellValue);
                    SetParagraphFormatting(groupCell.Format);
                    groupCell.Format.Font.Bold = true;
                }
            }

            //Write aggregators
            if (aggregators != null && aggregators.Aggregates.Any(a => a.Column != "__Count"))
            {
                foreach (AggregatorType aggregatorType in Enum.GetValues(typeof(AggregatorType)))
                {
                    var isAggregatorWritten = false;
                    var currentTypeAggregators = aggregators.Aggregates.Where(a => a.Type == aggregatorType);
                    if (!currentTypeAggregators.Any()) continue;

                    //Aggregators on grouped columns are disabled
                    /*for (var i = 0; i < Options.GroupInfo.Count; i++)
                    {
                        //NOT IMPLEMENTED FOR PDF!!!
                    }*/

                    var aggregatorRowCache = new List <Tuple<int, string>>();

                    for (var i = 0; i < ungroupedColumns.Count; i++)
                    {
                        var aggregatorInfo = currentTypeAggregators.FirstOrDefault(a => a.Column.ToLower() == ungroupedColumns[i].Name.ToLower());
                        if (aggregatorInfo == null) continue;
                        aggregatorRowCache.Add(new Tuple<int, string>(Options.GroupInfo.Count + i, aggregatorInfo.ValueFormatted));
                        isAggregatorWritten = true;
                    }

                    if (isAggregatorWritten)
                    {
                        var aggregatorRow = table.AddRow();
                        var displayAggregatorInfo = BaseViewPageBase<object>.GetResourceValue("GlobalResources", _aggregatorResources[(group.Key.ToString() == "ROOT" ? "TOTAL_" : "") + aggregatorType.ToString()]);
                        if (group.Key?.ToString() != "ROOT")
                        {
                            displayAggregatorInfo += $" ({Options.ColumnInfo.SingleOrDefault(c => c.Name == group.Column.Name)?.Caption}: {group.Key})";
                        }

                        foreach (var tuple in aggregatorRowCache)
                        {
                            var aggregatorCell = aggregatorRow.Cells[tuple.Item1];
                            var adjustedCellValue = AdjustIfTooWideToFitInPDFCell(aggregatorCell, tuple.Item2);
                            aggregatorCell.AddParagraph(adjustedCellValue);
                            SetParagraphFormatting(aggregatorCell.Format);
                        }

                        var aggregatorDisplayInfoCell = aggregatorRow.Cells[_totalColumns - 1];
                        var adjustedaggregatorDisplayInfoCellValue = AdjustIfTooWideToFitInPDFCell(aggregatorDisplayInfoCell, displayAggregatorInfo);
                        aggregatorDisplayInfoCell.AddParagraph(adjustedaggregatorDisplayInfoCellValue);
                        SetParagraphFormatting(aggregatorDisplayInfoCell.Format);

                        //Set aggregator style
                        aggregatorRow.Shading.Color = AggregatorMigraDocColor;
                        aggregatorRow.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
                        aggregatorRow.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
                        aggregatorRow.Format.Font.Bold = true;

                        currentLine++;
                    }
                }
            }

            //Make the line between the current group and the next bold
            table.Rows[currentLine - 1].Borders.Bottom.Width = 0.8;

            //Remove the current group from the grouped columns list since the parameter is passed by reference
            if (groupedColumns.Count > 0)
            {
                groupedColumns.RemoveAt(groupedColumns.Count - 1);
            }

            return currentLine;
        }

        private MigraDoc.DocumentObjectModel.Document InitializePDF()
        {
            //Create a new MigraDoc document
            var pdfDocument = new MigraDoc.DocumentObjectModel.Document();

            //Set page setup options
            var pageSetup = pdfDocument.DefaultPageSetup.Clone();

            if (Options.PortraitOrientation)
            {
                pageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Portrait;
                _availablePDFWidthInMillimeter = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(pageSetup.PageWidth.Millimeter -
                                                                                                 pageSetup.LeftMargin.Millimeter -
                                                                                                 pageSetup.RightMargin.Millimeter);
            }
            else
            {
                pageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
                _availablePDFWidthInMillimeter = MigraDoc.DocumentObjectModel.Unit.FromMillimeter(pageSetup.PageHeight.Millimeter -
                                                                                                 pageSetup.TopMargin.Millimeter -
                                                                                                 pageSetup.BottomMargin.Millimeter);
            }

            //Set fonts
            var style = pdfDocument.Styles.AddStyle("CustomTableStyle", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Size = 11;

            _PDFTextMeasurement = new TextMeasurement(style.Font.Clone());

            //Each MigraDoc document needs at least one section
            pdfDocument.AddSection();
            var headerTitle = new MigraDoc.DocumentObjectModel.Paragraph();
            headerTitle.AddText(Options.ExportTitle);
            headerTitle.Format.Alignment = MigraDoc.DocumentObjectModel.ParagraphAlignment.Center;
            headerTitle.Format.Font.Bold = true;
            pdfDocument.LastSection.Headers.Primary.Elements.Add(headerTitle);
            pdfDocument.LastSection.PageSetup = pageSetup;

            return pdfDocument;
        }

        private MigraDoc.DocumentObjectModel.Tables.Table InitializePDFTable()
        {
            //Create the item table and set grid lines
            return new MigraDoc.DocumentObjectModel.Tables.Table
            {
                Borders = { Width = Options.IncludeGridLines ? 0.5 : 0 },
                Style = "CustomTableStyle"
            };
        }

        private string AdjustIfTooWideToFitInPDFCell(MigraDoc.DocumentObjectModel.Tables.Cell cell, string value)
        {
            var column = cell.Column;
            var availableWidth = column.Width - column.Table.Borders.Width - cell.Borders.Width - column.LeftPadding - column.RightPadding;

            var tooWideWords = value.Split(" ".ToCharArray()).Distinct().Where(s => TooWideForPDFCell(s, availableWidth));

            var adjusted = new System.Text.StringBuilder(value);
            foreach (string word in tooWideWords)
            {
                var replacementWord = MakeFitToPDFCell(word, availableWidth);
                adjusted.Replace(word, replacementWord);
            }

            return adjusted.ToString();
        }

        private bool TooWideForPDFCell(string word, MigraDoc.DocumentObjectModel.Unit cellWidth)
        {
            var wordWidth = _PDFTextMeasurement.MeasureString(word, MigraDoc.DocumentObjectModel.UnitType.Point).Width;
            return wordWidth > cellWidth.Point;
        }

        private string MakeFitToPDFCell(string word, MigraDoc.DocumentObjectModel.Unit cellWidth)
        {
            var adjustedWord = new System.Text.StringBuilder();
            var current = string.Empty;
            foreach (char c in word)
            {
                if (TooWideForPDFCell(current + c, cellWidth))
                {
                    adjustedWord.Append(current);
                    adjustedWord.Append(" ");
                    current = c.ToString();
                }
                else
                {
                    current += c;
                }
            }
            adjustedWord.Append(current);

            return adjustedWord.ToString();
        }

        private void SetParagraphFormatting(MigraDoc.DocumentObjectModel.ParagraphFormat format)
        {
            format.Font.Size = _totalColumns > 9 ? 7 : 11;
        }

        private string SavePDFFile(MigraDoc.DocumentObjectModel.Document pdfDocument, MigraDoc.DocumentObjectModel.Tables.Table table, string filename)
        {
            //Add the table to the section
            pdfDocument.LastSection.Add(table);

            var randomFolderName = Guid.NewGuid().ToString();
            var randomPath = Path.Combine(Path.GetTempPath(), randomFolderName);
            if (Directory.Exists(randomPath)) Directory.Delete(randomPath, true);
            Directory.CreateDirectory(randomPath);
            var filePath = Path.Combine(randomPath, filename + ".pdf");

            var renderer = new MigraDoc.Rendering.PdfDocumentRenderer(true) { Document = pdfDocument };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);

            return filePath;
        }

#endregion

        private object GetCellValue(T item, ColumnOptionsV2 columnOptions, bool applyFormatting = true)
        {
            if (columnOptions == null) return null;

            var cellValue = _getters[columnOptions.Name](item);
            if (cellValue == null) return null;

            if (string.IsNullOrWhiteSpace(columnOptions.Formatting)) return cellValue;

            if (cellValue is int || cellValue is decimal || cellValue is byte || cellValue is long || cellValue is double || cellValue is short || cellValue is float)
            {
                cellValue = applyFormatting ? 
                    (object)double.Parse(cellValue.ToString()).ToString(columnOptions.Formatting)
                    : double.Parse(cellValue.ToString());
            }
            else if (cellValue is DateTime)
            {
                var format = zAppDev.DotNet.Framework.Utilities.Common.ConvertMomentFormat(columnOptions.Formatting);
                var dateTime = (cellValue as DateTime?).GetValueOrDefault();

                cellValue = applyFormatting ? (object)dateTime.ToString(format) : dateTime;
            }

            return cellValue;
        }

        public string Export(List<T> items, List<AggregatorInfo<T>> aggregators)
        {
            switch (Options.Type)
            {
                case ExportHelper.Type.EXCEL:
                    return ExportToExcel(items, aggregators);
                case ExportHelper.Type.PDF:
                    return ExportToPDF(items, aggregators);
                case ExportHelper.Type.WORD:
                    throw new Exception("Word export is not implemented yet");
                default:
                    throw new Exception("Invalid export type!");
            }
        }

        public string Export(GroupTree<T> groups, GroupTree<T> aggregators, Func<T, ActionResult> postProcessAction = null)
        {
            switch (Options.Type)
            {
                case ExportHelper.Type.EXCEL:
                    return ExportToExcel(groups, aggregators, postProcessAction);
                case ExportHelper.Type.PDF:
                    return ExportToPDF(groups, aggregators, postProcessAction);
                case ExportHelper.Type.WORD:
                    throw new Exception("Word export is not implemented yet");
                default:
                    throw new Exception("Invalid export type!");
            }
        }
    }
}