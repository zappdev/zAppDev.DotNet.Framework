using System.Collections.Generic;


namespace CLMS.Framework.Mvc
{
    public class PickListResult : AutocompleteResult
    {
        public string[] Term { get; protected set; }
        public string[] Columns { get; protected set; }
        public string[] ColumnDataTypes { get; protected set; }
        public string[] Css { get; protected set; }
        public string OrderBy { get; protected set; }
        public string OrderByDir { get; protected set; }
        public PickListResult(int startRow, int rowSize, IList<KeyValuePair<string, string[]>> collection, int totalRows, string[] columns, string[] columnDataTypes
            , string orderBy, string[] term, string orderByDir, string[] css = null, bool[] columnSearchable = null, bool[] columnOrderable = null)
            : base(startRow, rowSize, totalRows, columnSearchable, columnOrderable)
        {
            Columns = columns;
            ColumnDataTypes = columnDataTypes;
            Collection = collection;
            OrderBy = orderBy;
            Term = term;
            OrderByDir = orderByDir;
            Css = css;
        }
    }

    public class DropDownBoxResult : AutocompleteResult
    {
        public DropDownBoxResult(int startRow, int rowSize, IList<KeyValuePair<string, string[]>> collection, int totalRows, string[] css = null)
            : base(startRow, rowSize, totalRows)
        {
            Collection = collection;
            Css = css;
        }

        public string[] Css { get; protected set; }
    }

    public class AutocompleteResult
    {
        protected AutocompleteResult() { }
        public AutocompleteResult(int startRow, int rowSize, IList<KeyValuePair<string, string>> collection, int totalRows)
        {
            StartRow = startRow;
            RowSize = rowSize;
            Collection = collection;
            TotalRows = totalRows;
        }

        protected AutocompleteResult(int startRow, int rowSize, int totalRows, bool[] columnSearchable = null, bool[] columnOrderable = null, bool[] columnGroupable = null)
        {
            StartRow = startRow;
            RowSize = rowSize;
            TotalRows = totalRows;
            ColumnSearchable = columnSearchable;
            ColumnOrderable = columnOrderable;
            ColumnGroupable = columnGroupable;
        }

        public int StartRow { get; protected set; }
        public int RowSize { get; protected set; }
        public object Collection { get; set; }
        public int TotalRows { get; protected set; }
        public bool[] ColumnSearchable { get; protected set; }
        public bool[] ColumnOrderable { get; protected set; }
        public bool[] ColumnGroupable { get; protected set; }
        public string Link { get; set; }
    }
}