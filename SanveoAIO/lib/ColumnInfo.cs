using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eperformance1
{
    public class SQLColumn
    {
        public string ColumnName { get; set; }
        public string FriendlyColumnName { get; set; }
        public string KeyColumn { get; set; }
        public string QueryColumnName { get; set; }
        public bool AllowNull { get; set; }
        public bool Visible { get; set; }
        public int Length { get; set; }
        public string DataType { get; set; }
        public int Position { get; set; }
        public string Type { get; set; }
        public string MultiValueTable { get; set; }
        public bool IsMaster { get; set; }

        //public string ColumnTableName { get; set; }
        //public bool IsEditable { get; set; }
        //public bool IsUpdatable { get; set; }
        //public int SqlDataType { get; set; }
    }

    public class SQLColumn2
    {
        public string ColumnName { get; set; }
        public string FriendlyColumnName { get; set; }
        public string KeyColumn { get; set; }
        public string QueryColumnName { get; set; }
        public bool AllowNull { get; set; }
        public bool Visible { get; set; }
        public int Length { get; set; }
        public string DataType { get; set; }
        public int Position { get; set; }
        public string Type { get; set; }
        public string MultiValueTable { get; set; }
        public bool IsMaster { get; set; }
        public string ColumnTableName { get; set; }
        public bool IsEditable { get; set; }
        public bool IsUpdatable { get; set; }
        public int SqlDataType { get; set; }
        public int ColumnId { get; set; }
        public string ColumnWidth { get; set; }
        public int ColumnPosition { get; set; }
        public int SortPosition { get; set; }
        public bool Hidden { get; set; }
    }

}
