// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.

/*
Tolis: This code is contains bad junior-level code (that magically works). 
We need to refactor it ASAP.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System.Text;
using MigraDoc.Rendering;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class ExportHelper
    {        
        public static ExportOptions ParseExportOptions(Newtonsoft.Json.Linq.JObject postedData)
        {
            if (postedData == null)
            {
                throw new ApplicationException("No data posted");
            }

            if (postedData["exportData"] == null)
            {
                throw new ApplicationException("No export data posted");
            }

            if (postedData["exportData"]["exportOptions"] == null)
            {
                throw new ApplicationException("No export options data posted");
            }

            var exportData = Utilities.Deserialize<ExportOptions>(postedData["exportData"]["exportOptions"].ToString());

            return exportData;
        }

        public enum Type
        {
            WORD,
            PDF,
            EXCEL,
            CSV
        }

        public enum Range
        {
            CURRENT,
            TOP100,
            ALL
        }

        public class ExportOptions
        {
            public int ID { get; set; }
            public Type Type { get; set; }
            public Range Range { get; set; }
            public bool ΟnlyGroups { get; set; }
            public string Filename { get; set; }
            public bool IncludeGridLines { get; set; }
            public bool PortraitOrientation { get; set; }
            public List<ColumnOptions> ColumnOptions { get; set; }
            public string GroupColor { get; set; }
            public string HeaderColor { get; set; }
            public string EvenColor { get; set; }
            public string OddColor { get; set; }
            public string AggregatorsRequest { get; set; }
            public string AggregateColor { get; set; }
            public bool NonGroupCount { get; set; }
            //internal Dictionary<string, string> AggregateColumnFormat;

            public static ExportOptions GetDefault(string exportMode, string fileName)
            {
                return new ExportOptions
                {
                    Range = Range.ALL,
                    Type = ParseStringToExportType(exportMode),
					Filename = fileName,
                    IncludeGridLines = true,
                    GroupColor = "rgb(255, 255, 255)",
                    HeaderColor = "rgb(221, 235, 247)",
                    EvenColor = "rgb(255, 255, 255)",
                    OddColor = "rgb(255, 255, 255)",
                    AggregateColor = "rgb(240, 240, 240)"
                };
            }

            public static Type ParseStringToExportType(string _exportModeRaw)
            {               
                var type = Type.EXCEL; // Default
                
                if (_exportModeRaw == "pdf")
                {
                    type = Type.PDF;                
                }
                else if (_exportModeRaw == "word")
                {
                    type = Type.WORD;                    
                }

                return type;
            }
        }


        public class ColumnOptions
        {
            public int ID { get; set; }
            public string Column { get; set; }
            public bool IsVisible { get; set; }
            public bool SumIsVisible { get; set; }
            public bool AverageIsVisible { get; set; }
            public bool CountIsVisible { get; set; }
        }

        public class ExportColumnDTO
        {
            public object Value;
            public string ColumnName;
            public bool IsVisible;
            public string Format;
            public string ExcelFormat;
            public string ColumnDataType;            
            public string Caption;
        }

        public class ExportRecordDTO
        {
            public List<ExportColumnDTO> Columns;

            public ExportRecordDTO()
            {
                Columns = new List<ExportColumnDTO>();
            }            

            public void MarkVisibleColumns(ExportOptions options)
            {
                foreach (var col in Columns)
                {
                    col.IsVisible = options.ColumnOptions.Any(c => c.Column == col.ColumnName && c.IsVisible);
                }
            }
        }


        internal class GroupStyleColorParse
        {
            internal int Red;
            internal int Green;
            internal int Blue;

            internal GroupStyleColorParse(int r, int g, int b)
            {
                Red = r;
                Green = g;
                Blue = b;
            }

            internal GroupStyleColorParse(string rgb)
            {
                try
                {
                    string rgbWithoutBracket = rgb.Substring(rgb.IndexOf('(') + 1, rgb.Length - 5);
                    string[] RGB = rgbWithoutBracket.Split(',');
                    Red = Convert.ToInt16(RGB[0]);
                    Green = Convert.ToInt16(RGB[1]);
                    Blue = Convert.ToInt16(RGB[2]);
                }
                catch { }
            }
        }

        public static string ExportList(List<ExportRecordDTO> result, ExportOptions options, int totalRows,
            Func<Document, Table, object> _pdfOvveride = null)
        {
            var link = string.Empty;

            switch (options.Type)
            {
                case Type.PDF:
                    link = ExportListToPDF(options, result, totalRows, _pdfOvveride);
                    break;

                case Type.EXCEL:
                    link = ExportListToOffice(options, "xls", result, totalRows);
                    break;

                case Type.WORD:
                    link = ExportListToOffice(options, "doc", result, totalRows);
                    break;
            }

            return link;
        }

        #region export in excel

        private static string ExportListToOffice(ExportOptions options, string fileExtension, List<ExportRecordDTO> result, int totalRows)
        {
            //Header Contents

            var firstResult = result.FirstOrDefault();
             
            firstResult?.MarkVisibleColumns(options);

            var columns = firstResult?.Columns;
            var visibleColumns = columns?.Where(c => c.IsVisible);

            var builder = new StringBuilder(result.Count() * visibleColumns.Count() * 50);
            string borderTable = (options.IncludeGridLines) ? "border='1'" : "";
            builder.AppendFormat("<table {0}>", borderTable);
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr>");
            foreach (ExportColumnDTO col in visibleColumns ?? new ExportColumnDTO[]{} )
            {
                var caption = string.IsNullOrWhiteSpace(col.Caption) ? col.ColumnName : col.Caption;
                builder.AppendLine($"<th style='background-color:{options.HeaderColor}'>{caption}</th>");
            }

            builder.AppendLine("</tr>");
            builder.AppendLine("</thead>");

            //Row Contents
            builder.AppendLine();
            builder.AppendLine("<tbody>");

            int currentItemIndex = 0;
            var doubleValue = 0.0;
            List<string> listInfoSumAvg = new List<string>();

            foreach (var item in result)
            {
                builder.AppendLine("<tr>");

                int indexOfListInfo = 0;
                
                foreach (ExportColumnDTO column in item.Columns)
                {
                    if (!visibleColumns.Any(c => c.ColumnName == column.ColumnName)) continue;

                    var excelCellFormat = CreateExcelFormatting(column);
                    var cellValue = GetOfficeValue(column, options.Type);


                    //Creation of Excel Content with original Format and the case format due excel
                    builder.AppendFormat("<td style='background-color: {0}; {1}'> {2} </td>",
                        currentItemIndex % 2 != 0 ? options.OddColor : options.EvenColor, 
                        excelCellFormat,
                        cellValue);
                                                            
                    //This branch keeps the sum for every column, if it is not possible keeps null
                    if (double.TryParse(column.ToString(), out doubleValue))
                    {
                        if (currentItemIndex == 0)
                        {
                            listInfoSumAvg.Add(doubleValue.ToString());
                        }
                        else
                        {
                            listInfoSumAvg[indexOfListInfo] = (Convert.ToDouble(listInfoSumAvg[indexOfListInfo]) + Convert.ToDouble(doubleValue)).ToString();
                        }
                    }
                    else if (currentItemIndex == 0)
                    {
                        listInfoSumAvg.Add(doubleValue.ToString());
                        listInfoSumAvg[indexOfListInfo] = null;
                    }
                    indexOfListInfo++;

                }

                builder.AppendLine("</tr>");
                currentItemIndex++;
            }

            //Display Rows for Sum, Avg and Count
           /* var distinctTypeOfAggregators = options.Aggregators.Select(y => y.Type).Distinct();

            if (distinctTypeOfAggregators.Contains("SUM"))
            {
                DisplayInExcelAggregates(opt, ref innerContents, listInfoSumAvg, "SUM", currentItemIndex);
            }
            if (distinctTypeOfAggregators.Contains("AVERAGE"))
            {
                DisplayInExcelAggregates(opt, ref innerContents, listInfoSumAvg, "AVERAGE", currentItemIndex);
            }
            if (opt.TotalNonGroupCount)
            {
                DisplayInExcelAggregates(opt, ref innerContents, listInfoSumAvg, "COUNT", currentItemIndex);
            }  */

            builder.AppendLine("</tbody>");
            builder.AppendLine("<tfoot>");
            if (options.NonGroupCount)
            {
                builder.AppendLine($"<tr><td colspan='{columns?.Count}'>{BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_AGGREGATORS_GrandCount")}{totalRows}</td></tr>");
            }
            builder.AppendLine("</tfoot>");
            builder.AppendLine("</table>");

            var fileContents = WrapContentsToOfficeFile(builder.ToString(), options.Filename);
            return CreateFileAndSendDownloadLink(options.Filename, fileContents, fileExtension);

        }

        private static string CreateExcelFormatting(ExportColumnDTO columnInfo)
        {          
            if (string.IsNullOrWhiteSpace(columnInfo.ExcelFormat)) return "";

            var datatypesThatNeedExcelFormatting = new string[] { "int", "long", "decimal", "double", "float", "datetime" };

            return datatypesThatNeedExcelFormatting.Contains(columnInfo.ColumnDataType.ToLowerInvariant())
                ? $"mso-number-format:\"{columnInfo.ExcelFormat}\""
                : string.Empty;            
        } 

        private static int CountNoMeaningZeros(string Part, int cntZeros)
        {
            if (Part.EndsWith("0"))
            {
                cntZeros = CountNoMeaningZeros(Part.Substring(0, Part.Length - 1), ++cntZeros);
            }
            else if (Part.StartsWith("0"))
            {
                cntZeros = CountNoMeaningZeros(Part.Substring(cntZeros + 1, Part.Length - 1), ++cntZeros);
            }
            return cntZeros;
        }
        
        private static string WrapContentsToOfficeFile(string contents, string listName)
        {           
            var officeFileContents = "";

            officeFileContents += "<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\"> \n";
            officeFileContents += "<head> \n";
            officeFileContents += "<meta http-equiv=Content-Type content=\"text/html; charset=utf-8\"> \n";
            officeFileContents += "<!--[if gte mso 9]> \n";
            officeFileContents += "<xml> \n";
            officeFileContents += "<x:ExcelWorkbook> \n";
            officeFileContents += "<x:ExcelWorksheets> \n";
            officeFileContents += "<x:ExcelWorksheet> \n";
            officeFileContents += string.Format("<x:Name>{0}</x:Name>", listName) + "\n";
            officeFileContents += "<x:WorksheetOptions> \n";
            officeFileContents += "<x:Panes> \n";
            officeFileContents += "</x:Panes> \n";

            officeFileContents += "</x:WorksheetOptions> \n";
            officeFileContents += "</x:ExcelWorksheet> \n";
            officeFileContents += "</x:ExcelWorksheets> \n";
            officeFileContents += "</x:ExcelWorkbook> \n";
            officeFileContents += "</xml> \n";
            officeFileContents += "<![endif]--> \n";
            officeFileContents += "</head> \n";
            officeFileContents += "<body> \n";

            officeFileContents += contents;

            officeFileContents += "</body> \n";
            officeFileContents += "</html>";

            return officeFileContents;
        }


        // todo : add pdfDocumentRender in parameters 
        private static string CreateFileAndSendDownloadLink(string name, string fileContents, string fileExtention, PdfDocumentRenderer pdfRenderer = null, PdfDocument pdfDocument = null)
        {
            var randomFolderName = Guid.NewGuid().ToString();
            var randomPath = Path.Combine(Path.GetTempPath(), randomFolderName);

            if (Directory.Exists(randomPath)) Directory.Delete(randomPath);
            Directory.CreateDirectory(randomPath);

            var filePath = Path.Combine(randomPath, name + "." + fileExtention);

            if (pdfRenderer == null && pdfDocument == null)
            {
                File.WriteAllText(filePath, fileContents);
            }
            else if (pdfDocument != null)
            {
                pdfDocument.Save(filePath);
            }
            else if (pdfRenderer != null)
            {
                pdfRenderer.Save(filePath);
            }
          
            return Path.Combine(randomFolderName, name + "." + fileExtention);
        }
        
        #endregion

        #region export in pdf
        
        //PM> Install-Package PdfSharp
        //license: http://www.pdfsharp.net/PDFsharp_License.ashx
        public static string ExportListToPDF(ExportOptions opt, List<ExportRecordDTO> result, int totalRows,
            Func<Document, Table, object> _pdfOvveride)
        {
            var firstResult = result.First();

            firstResult.MarkVisibleColumns(opt);

            var columns = result.First().Columns;
            var visibleColumns = columns.Where(c => c.IsVisible);

            var document = InitializePDFDocument(visibleColumns.Count(), out Unit availableWidth, out Unit availableColumnWidth, opt.PortraitOrientation);

            //create section wrapper of table
            Section section = new Section();
            document.Add(section);

            //create table
            Table table = new Table();  
            table.AddColumn();//counterColumn 

            Cell cell;
            Column column;


            List<ExportColumnDTO> columnCaptions = new List<ExportColumnDTO>();
            //create the columns based on headers
            foreach (var cols in visibleColumns)
            {                
                table.AddColumn();
                columnCaptions.Add(cols);
            }

            
            //create one row for the headers, the style of headers and the row contents
            var row = table.AddRow();
            GroupStyleColorParse mycolor = new GroupStyleColorParse(opt.HeaderColor);
            row.Shading.Color = Color.FromRgbColor(255, new Color((byte)mycolor.Red, (byte)mycolor.Green, (byte)mycolor.Blue));

            var styleHeader = document.AddStyle("Headers", "Normal");
            styleHeader.Font.Name = "Headers";
            styleHeader.Font.Bold = true;
            styleHeader.Font.Size = 14;
            TextMeasurement tmHeader = new TextMeasurement(document.Styles["Headers"].Font.Clone());


            var currentItemIndex = 0;
            
            foreach (var col in columnCaptions)
            {
                cell = row.Cells[currentItemIndex + 1]; //Cells[0] is emtpy, the header for counter column 
                column = cell.Column;
                column.Width = availableColumnWidth;
                Paragraph p = new Paragraph();
                var caption = string.IsNullOrWhiteSpace(col.Caption) ? col.ColumnName : col.Caption;
                p.AddFormattedText(WideWordsAdjust(cell, caption,  tmHeader), "Headers");
                cell.Add(p);
                currentItemIndex++;

            }


            //Fill with data(default style) the PDf table           
            int rowCounter = 0;
            
            var style = document.Styles["Normal"];
            TextMeasurement tm = new TextMeasurement(document.Styles["Normal"].Font.Clone());

            //var doubleValue = 0.0;
            mycolor = new GroupStyleColorParse(opt.OddColor);
            GroupStyleColorParse mycolorEven = new GroupStyleColorParse(opt.EvenColor);


           foreach (var record in result)
            {
                var rowIn = table.AddRow();
                rowCounter++;
                rowIn.Shading.Color = rowCounter % 2 != 0
                    ? Color.FromRgbColor(255, new Color((byte)mycolor.Red, (byte)mycolor.Green, (byte)mycolor.Blue))
                    : Color.FromRgbColor(255, new Color((byte)mycolorEven.Red, (byte)mycolorEven.Green, (byte)mycolorEven.Blue));

                cell = rowIn.Cells[0];
                cell.AddParagraph(rowCounter.ToString());
                AutoFitColumn(cell, rowCounter.ToString(), tm);

                int colCounter = 0;
                string validPDFFormat = "";
                foreach (var visibleColumn in record.Columns)
                {
                    if (!visibleColumns.Any(c => c.ColumnName == visibleColumn.ColumnName)) continue;
                    colCounter += 1;
                    //var recordColumn = record.Columns.FirstOrDefault(c => c.ColumnName == visibleColumn.ColumnName);                    
                    cell = rowIn.Cells[colCounter];
                    column = cell.Column;
                    column.Width = availableColumnWidth;
                    validPDFFormat = ApplyValueFormat(visibleColumn);
                    cell.AddParagraph(WideWordsAdjust(cell, validPDFFormat?.ToString(), tm));
                    
                    //colCounter++;
                }
            }

            if (opt.NonGroupCount)
            {
                var totalsRow = table.AddRow();
                totalsRow.Cells[0].MergeRight = visibleColumns.Count();
                totalsRow.Cells[0].AddParagraph($"{BaseViewPageBase<object>.GetResourceValue("GlobalResources", "RES_DATALIST_AGGREGATORS_GrandCount")}{totalRows}");
            }

            //Add the table to the section and the document is ready for render
            if (opt.IncludeGridLines) table.Borders.Visible = true;

            if (_pdfOvveride == null)
            {
                section.Add(table);
            }
            else
            {
                _pdfOvveride?.Invoke(document, table);
            }

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true)
            {
                Document = document
            };
            pdfRenderer.RenderDocument();

            return CreateFileAndSendDownloadLink(opt.Filename, null, "pdf", pdfRenderer);
        }

        private static void AutoFitColumn(Cell cell, string cols, TextMeasurement tm)
        {
            Column column = cell.Column;
            if (column.Width < Unit.FromMillimeter(cols.Length * 4)) column.Width = Unit.FromMillimeter(cols.Length * 4);
        }

        private static string WideWordsAdjust(Cell cell, string text, TextMeasurement tm, string formatColumn = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            Column column = cell.Column;
            Unit columnWidth = column.Width - (column.Table.Borders.Width * 2);

            var tooWideWords = text.Split(" ".ToCharArray()).Distinct().Where(s => TooWide(s, columnWidth, tm));
            var adjusted = new StringBuilder(text);
            foreach (string word in tooWideWords)
            {
                var adjustedWord = new StringBuilder();
                var current = string.Empty;
                foreach (char c in word)
                {
                    if (TooWide(current + c, columnWidth, tm))
                    {
                        adjustedWord.Append(current);
                        adjustedWord.Append(Chars.CR);//TODO: ERROR!! here there is Bug in case we have a very wide word
                        current = c.ToString();
                    }
                    else
                    {
                        current += c;
                    }
                }
                adjusted.Replace(word, adjustedWord.Append(current).ToString());
            }

            return adjusted.ToString();
        }

        private static bool TooWide(string word, Unit width, TextMeasurement tm)
        {
            float f = tm.MeasureString(word, UnitType.Millimeter).Width;
            return f > width.Millimeter;
        }

        private static Document InitializePDFDocument(int columnNumber, out Unit availableWidth, out Unit availableColumnWidth, bool orientationPDF, bool groupmode = false, bool includeAggregateColumn = false)
        {
            //create Document setup
            var document = new MigraDoc.DocumentObjectModel.Document();

            if (!orientationPDF)
            {
                document.DefaultPageSetup.Orientation = Orientation.Landscape;
            }
            else
            {
                document.DefaultPageSetup.Orientation = Orientation.Portrait;
            }

            document.DefaultPageSetup.PageFormat = PageFormat.A4;
            document.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            document.DefaultPageSetup.TopMargin = Unit.FromCentimeter(1);

            //calculate the static width of each column
            if (!includeAggregateColumn)
            {
                if (!orientationPDF)
                {
                    availableWidth = document.DefaultPageSetup.PageHeight - (document.DefaultPageSetup.LeftMargin * 2);
                }
                else
                {
                    availableWidth = document.DefaultPageSetup.PageWidth - (document.DefaultPageSetup.LeftMargin * 2);
                }
            }
            else //include aggregate column
            {
                //We multiply more than 2 (for each margin (left, right)) including the fixed width of the aggregate column 
                /*In groupMode the aggregate column has width equal with other columns due to dynamic item(s) 
                 * without groupmode column is autofit so we multiply with 2,7
                 */
                if (!orientationPDF)
                {
                    availableWidth = document.DefaultPageSetup.PageHeight - (document.DefaultPageSetup.LeftMargin * ((groupmode) ? 2 : 2.7));//No prtrait
                }
                else
                {
                    availableWidth = document.DefaultPageSetup.PageWidth - (document.DefaultPageSetup.LeftMargin * ((groupmode) ? 2 : 2.7));
                }
            }
            availableWidth.ConvertType(UnitType.Centimeter);
            availableColumnWidth = availableWidth / (columnNumber);
            availableColumnWidth.ConvertType(UnitType.Centimeter);

            return document;
        }

        #endregion

        #region MISC
        public static string ApplyValueFormat(string mambaDataType, object value, string formatting, CultureInfo culture = null)
        {
            if (value == null) return "";

            if (string.IsNullOrWhiteSpace(formatting)) return value.ToString();

            switch (mambaDataType.ToLowerInvariant())
            {
                case "int":
                    return culture != null
                        ? ((int)value).ToString(formatting, culture)
                        : ((int)value).ToString(formatting);

                case "long":
                    return culture != null
                        ? ((long)value).ToString(formatting, culture)
                        : ((long)value).ToString(formatting);

                case "float":
                    return culture != null
                        ? ((float)value).ToString(formatting, culture)
                        : ((float)value).ToString(formatting);

                case "decimal":
                    return culture != null
                        ? ((decimal)value).ToString(formatting, culture)
                        : ((decimal)value).ToString(formatting);

                case "double":
                    return culture != null
                        ? ((double)value).ToString(formatting, culture)
                        : ((double)value).ToString(formatting);

                case "datetime":
                    var result = ProceedDatetimeFields(value);
                    if (result == null) return "";
                    return culture != null
                        ? result.Value.ToString(formatting, culture)
                        : result.Value.ToString(formatting);

                default:
                    return value.ToString();
            }
        }

        private static string ApplyValueFormat(ExportColumnDTO columnInfo)
        {
            return ApplyValueFormat(columnInfo.ColumnDataType, columnInfo.Value, columnInfo.Format);
        }

        private static string GetOfficeValue(ExportColumnDTO column, Type officeType)
        {
            if (string.IsNullOrWhiteSpace(column.ExcelFormat) || officeType == Type.WORD)
                return ApplyValueFormat(column);

            if (column.ColumnDataType.ToLowerInvariant() == "datetime" && (column?.Value as DateTime?).HasValue)
            {
                return ProceedDatetimeFields(column?.Value)?.ToLocalTime().ToOADate().ToString() ?? "";
            }

            return column?.Value?.ToString() ?? "";
        }

        private static DateTime? ProceedDatetimeFields(object value)
        {
            var result = ((DateTime)value);
            if (result == null) return null;

            switch (result.Kind)
            {
                case DateTimeKind.Utc:
                    return result.ToLocalTime();
                case DateTimeKind.Local:
                    return result.ToLocalTime();
                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(result, DateTimeKind.Utc).ToLocalTime();
            }

            return result.ToLocalTime();
        }

        #endregion
    }

    
} 