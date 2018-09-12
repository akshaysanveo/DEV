using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Eperformance1
{
        public static class Extensions
        {
            public static List<TSource> ToList<TSource>(this DataTable dataTable) where TSource : new()
            {
                var dataList = new List<TSource>();

                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
                var objFieldNames = (from PropertyInfo aProp in typeof(TSource).GetProperties(flags)
                                     select new
                                     {
                                         Name = aProp.Name,
                                         Type = Nullable.GetUnderlyingType(aProp.PropertyType) ??
                                 aProp.PropertyType
                                     }).ToList();
                var dataTblFieldNames = (from DataColumn aHeader in dataTable.Columns
                                         select new
                                         {
                                             Name = aHeader.ColumnName,
                                             Type = aHeader.DataType
                                         }).ToList();
                var commonFields = objFieldNames.Intersect(dataTblFieldNames).ToList();

                foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
                {
                    var aTSource = new TSource();
                    foreach (var aField in commonFields)
                    {
                        PropertyInfo propertyInfos = aTSource.GetType().GetProperty(aField.Name);
                        var value = (dataRow[aField.Name] == DBNull.Value) ?
                        null : dataRow[aField.Name]; //if database field is nullable
                        propertyInfos.SetValue(aTSource, value, null);
                    }
                    dataList.Add(aTSource);
                }
                return dataList;
            }
        }

        public class Node
        {
            public int ParentNodeId { get; set; }
            public int NodeId { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string SHORTNAME { get; set; }
            public string ProjectStatus { get; set; }
        }

        public struct JQGridResults
        {
            public int page;
            public int total;
            public int records;
            public JQGridRow[] rows;

        }

        public struct JQGridRow
        {
            public int id;
            public string[] cell;
        }

        public class RowCell
        {
            public object TableName { get; set; }
            public object ColumnName { get; set; }
            public object ColumnValue { get; set; }
            public object ColumnType { get; set; }
        }

        public class CoumnsSettings
        {
            public String ColumnId { get; set; }
            public String ColumnIndex { get; set; }
            public String TableName { get; set; }
            public String Width { get; set; }
            public String Position { get; set; }
            public bool Hidden { get; set; }
        }

        public class DeliveryReportRowData
        {
            public String TaskId { get; set; }
            public String Concerns { get; set; }
            public String OnSiteFeedBack { get; set; }
            public String WeekStartDate { get; set; }
            public String WeekEndDate { get; set; }
        }
    }
