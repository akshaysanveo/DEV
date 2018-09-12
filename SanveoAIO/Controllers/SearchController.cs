using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SanveoAIO.Domain.Models;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.IO;
using System.Data;
using Excel;
using System.Data.Entity.Core.Objects;
using ClosedXML.Excel;
using System.Threading;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;
using Newtonsoft.Json.Linq;
using telmvc;
using System.Collections;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using SanveoAIO.hubs;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using DocumentFormat.OpenXml.Drawing;

namespace SanveoAIO.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search

        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();

        public ActionResult Search()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];

                string val = user.validity;
                string username = user.FirstName;
                ViewBag.validity = val;
                ViewBag.username = username;
                ViewBag.PageName = "Index";
                ViewBag.compType = user.Comp_Type;
                ViewBag.userid = "progressBar" + user.U_Id;

                return View();
            }
            else
                return RedirectToAction("Login", "Login");
        }


        //<------ Start of Search Popup Methods-------->      

        public JsonResult GetAutoTextData([DataSourceRequest] DataSourceRequest request, string AutoText, string Id, string CategoryName, string CheckValue)
        {
            string AT = AutoText.Trim();
            user = (UserInfo) Session["UserInfo"];
            var data = db.Database.SqlQuery<sp_GetAutoCompleteData1_Result>("sp_GetAutoCompleteData @Text={0},@Urn={1},@CategoryName={2},@PropertyName={3},@compid={4}", AT, Id, CategoryName, CheckValue, user.Comp_ID).ToList();

            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAutoTextDataOption1([DataSourceRequest] DataSourceRequest request, string AutoText, string Id, string CategoryName)
        {
            string AT = AutoText.Trim();
            user = (UserInfo) Session["UserInfo"];
            var data = db.Database.SqlQuery<sp_GetAutoCompleteDataForge1_Result>("sp_GetAutoCompleteDataForge @Text={0},@Urn={1},@CategoryName={2},@compid={3}", AT, Id, CategoryName, user.Comp_ID).ToList();

            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSearchGridModelData1(string Id, string CategoryName, string CheckValue)
        {
            if (CheckValue != "")
            {
                string tabledata = "";
                string Exceltabledata = "";
                List<SqlParameter> parameters1 = new List<SqlParameter>();

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Id,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@CategoryName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = CategoryName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@PropertyName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = CheckValue,
                    Direction = System.Data.ParameterDirection.Input
                });


                DataSet dataSet2 = SqlManager.ExecuteDataSet("GetPropertySearchData", parameters1.ToArray());

                DataTable datatable = dataSet2.Tables[0];


                tabledata += "<table id='TblGrid1' style='font-size:15px'>";
                Exceltabledata += "<table id='TblGrid1Excel' style='display:none' >";
                tabledata += "<thead>";
                Exceltabledata += "<thead>";
                tabledata += "<tr>";
                Exceltabledata += "<tr>";

                foreach (DataColumn dr in datatable.Columns)
                {

                    if (dr.ColumnName.Trim() != "")
                    {
                        tabledata += "<th>" + dr.ColumnName.Trim() + "</th>";
                        Exceltabledata += "<th>" + dr.ColumnName.Trim() + "</th>";
                        // data - field = " + '"' + dr.ColumnName.Trim() + '"' + "
                    }

                }

                tabledata += "</tr>";
                tabledata += "</thead>";
                tabledata += "<tbody>";
                Exceltabledata += "</tr>";
                Exceltabledata += "</thead>";
                Exceltabledata += "<tbody>";

                foreach (DataRow dr in datatable.Rows)
                {
                    int loop = 0;
                    int i = 0;
                    string textbox = "";
                    foreach (DataColumn col in datatable.Columns)
                    {
                        if (loop == 0)
                        {
                            tabledata += "<tr>";
                        }

                        if (loop == 1)
                        {
                            // id = '" + dr[col.ColumnName].ToString() + "' onclick = 'SaveCategoryName(this.name,this.id);'
                            Exceltabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";

                            tabledata += "<td style='width:10%'><input type ='button' value = '" + dr[col.ColumnName].ToString() + "' id ='" + dr[col.ColumnName].ToString() + "' onclick='window.parent.Forgeid(this.id);' /></td>";
                            textbox = dr[col.ColumnName].ToString();
                        }
                        if (loop <= 3)
                        {
                            if (dr[col.ColumnName].ToString() != "")
                            {
                                if (loop != 1)
                                {
                                    Exceltabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";
                                    tabledata += "<td style='width:10%'>" + dr[col.ColumnName].ToString() + "</td>";
                                }
                            }
                            else
                            {
                                Exceltabledata += "<td></td>";
                                tabledata += "<td></td>";
                            }
                        }
                        else if (loop > 3)
                        {
                            if (dr[col.ColumnName].ToString() != "")
                            {
                                Exceltabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";

                                tabledata += "<td><a href='javascript:void(0)' header='" + col.ColumnName + "' id='link_" + textbox + "_" + i + "' onclick='enabletextbox(this.id)' class='editlink'>" + dr[col.ColumnName].ToString() + "<a/><input type='text' style='display:none' id='text_" + textbox + "_" + i + "' class='edittextbox' onkeydown='handleKeyPress(event)' onblur='disabletextbox()'></td>";
                            }
                            else
                            {
                                Exceltabledata += "<td></td>";
                                // tabledata += "<td></td>";
                                tabledata += "<td align='center'><a href='javascript:void(0)' header='" + col.ColumnName + "' id='link_" + textbox + "_" + i + "' onclick='enabletextbox(this.id)' class='editlink'>-<a/><input type='text' style='display:none' id='text_" + textbox + "_" + i + "' class='edittextbox' onkeydown='handleKeyPress(event)' onblur='disabletextbox()'></td>";
                            }
                        }
                        loop++;
                        i++;
                        if (loop == datatable.Columns.Count)
                        {
                            Exceltabledata += "</tr>";
                            tabledata += "</tr>";
                        }

                    }

                }

                tabledata += "</tbody>";
                tabledata += "</table>";
                Exceltabledata += "</tbody>";
                Exceltabledata += "</table>";


                // return Json(tabledata, "text/plain");
                return Json(new { Exceltabledata = Exceltabledata, tabledata = tabledata }, "text/plain");
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSearchGridModelDataOption1(string Id, string CategoryName, string filtervalue)
        {

            string tabledata = "";

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Id,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CategoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = CategoryName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyValue",
                SqlDbType = SqlDbType.VarChar,
                Value = filtervalue,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("GetPropertySearchDataOption1", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];


            tabledata += "<table id='TblGrid11' style='font-size:15px'>";

            tabledata += "<thead>";

            tabledata += "<tr>";


            foreach (DataColumn dr in datatable.Columns)
            {

                if (dr.ColumnName.Trim() != "")
                {
                    tabledata += "<th>" + dr.ColumnName.Trim() + "</th>";

                    // data - field = " + '"' + dr.ColumnName.Trim() + '"' + "
                }

            }

            tabledata += "</tr>";
            tabledata += "</thead>";
            tabledata += "<tbody>";

            foreach (DataRow dr in datatable.Rows)
            {
                int loop = 0;
                int i = 0;

                foreach (DataColumn col in datatable.Columns)
                {
                    if (loop == 0)
                    {
                        tabledata += "<tr>";
                    }


                    if (dr[col.ColumnName].ToString() != "")
                    {
                        // tabledata += "<td style='width:10%'>" + dr[col.ColumnName].ToString() + "</td>";
                        tabledata += "<td style='width:10%'><input type ='button' value = '" + dr[col.ColumnName].ToString() + "' id ='" + dr[col.ColumnName].ToString() + "' onclick='window.parent.Forgeid(this.id);' /></td>";
                    }
                    else
                    {

                        tabledata += "<td></td>";
                    }


                    loop++;
                    i++;
                    if (loop == datatable.Columns.Count)
                    {

                        tabledata += "</tr>";
                    }

                }

            }

            tabledata += "</tbody>";
            tabledata += "</table>";


            return Json(tabledata, "text/plain");

        }

        public JsonResult GetAutoGridData(string AutoText, string Id, string CategoryName, string CheckValue)
        {
            //if ((AutoText.ToString().Contains("'")))
            //{
            //    AutoText = AutoText.Substring(0, 1);

            //    //AutoText = AutoText.Replace(@"""", " ");
            //    //AutoText = AutoText.Replace("'", " ");

            //}


            string tabledata = "";
            string Exceltabledata = "";
            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Text",
                SqlDbType = SqlDbType.VarChar,
                Value = AutoText,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Id,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CategoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = CategoryName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyName",
                SqlDbType = SqlDbType.VarChar,
                Value = CheckValue,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("sp_GetAutoGridData", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];


            tabledata += "<table id='TblGrid1' >";
            tabledata += "<thead>";
            tabledata += "<tr>";

            Exceltabledata += "<table id='TblGrid1Excel' style='display:none' >";
            Exceltabledata += "<thead>";
            Exceltabledata += "<tr>";

            foreach (DataColumn dr in datatable.Columns)
            {

                if (dr.ColumnName.Trim() != "")
                {
                    tabledata += "<th>" + dr.ColumnName.Trim() + "</th>";
                    Exceltabledata += "<th>" + dr.ColumnName.Trim() + "</th>";
                    // data - field = " + '"' + dr.ColumnName.Trim() + '"' + "
                }

            }

            tabledata += "</tr>";
            tabledata += "</thead>";
            tabledata += "<tbody>";

            Exceltabledata += "</tr>";
            Exceltabledata += "</thead>";
            Exceltabledata += "<tbody>";

            foreach (DataRow dr in datatable.Rows)
            {
                int loop = 0;
                int i = 0;
                string textbox = "";
                foreach (DataColumn col in datatable.Columns)
                {
                    if (loop == 0)
                    {
                        tabledata += "<tr>";
                        Exceltabledata += "<tr>";
                    }

                    if (loop == 1)
                    {
                        // id = '" + dr[col.ColumnName].ToString() + "' onclick = 'SaveCategoryName(this.name,this.id);'

                        tabledata += "<td><input type ='button' value = '" + dr[col.ColumnName].ToString() + "' id ='" + dr[col.ColumnName].ToString() + "' onclick='window.parent.Forgeid(this.id);' /></td>";
                        textbox = dr[col.ColumnName].ToString();

                        Exceltabledata += "<td>'" + dr[col.ColumnName].ToString() + "'</td>";
                    }
                    if (loop <= 3)
                    {
                        if (dr[col.ColumnName].ToString() != "")
                        {
                            if (loop != 1)
                            {
                                tabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";
                                Exceltabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";
                            }
                        }
                        else
                        {
                            tabledata += "<td></td>";
                            Exceltabledata += "<td></td>";
                        }
                    }
                    else if (loop > 3)
                    {
                        if (dr[col.ColumnName].ToString() != "")
                        {
                            tabledata += "<td><a href='javascript:void(0)' header='" + col.ColumnName + "' id='link_" + textbox + "_" + i + "' onclick='enabletextbox(this.id)' class='editlink'>" + dr[col.ColumnName].ToString() + "<a/><input type='text' style='display:none' id='text_" + textbox + "_" + i + "' class='edittextbox' onkeydown='handleKeyPress(event)' onblur='disabletextbox()'></td>";

                            Exceltabledata += "<td>" + dr[col.ColumnName].ToString() + "</td>";
                        }
                        else
                        {
                            //  tabledata += "<td></td>";

                            tabledata += "<td><a href='javascript:void(0)' header='" + col.ColumnName + "' id='link_" + textbox + "_" + i + "' onclick='enabletextbox(this.id)' class='editlink'>-<a/><input type='text' style='display:none' id='text_" + textbox + "_" + i + "' class='edittextbox' onkeydown='handleKeyPress(event)' onblur='disabletextbox()'></td>";

                            Exceltabledata += "<td></td>";
                        }
                    }
                    loop++;
                    i++;
                    if (loop == datatable.Columns.Count)
                    {
                        tabledata += "</tr>";
                        Exceltabledata += "</tr>";
                    }

                }

            }

            tabledata += "</tbody>";
            tabledata += "</table>";

            Exceltabledata += "</tbody>";
            Exceltabledata += "</table>";


            // return Json(tabledata, "text/plain");

            return Json(new { Exceltabledata = Exceltabledata, tabledata = tabledata }, "text/plain");

        }

        public ActionResult UpdateProperyData(string Forgeid, string Header, string TextValue, string URN)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Forgeid",
                SqlDbType = SqlDbType.VarChar,
                Value = Forgeid,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Header",
                SqlDbType = SqlDbType.VarChar,
                Value = Header,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@TextValue",
                SqlDbType = SqlDbType.VarChar,
                Value = TextValue,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@URN",
                SqlDbType = SqlDbType.VarChar,
                Value = URN,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CurrentID",
                SqlDbType = SqlDbType.Int,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("sp_UpdateProperyData", parameters.ToArray());
            //    DataTable dtruledat = dtRuleData.Tables[0];

            // var finaldata = db.sp_UpdateProperyData(Forgeid, Header, TextValue, URN);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEditedValues(string URN)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@URN",
                SqlDbType = SqlDbType.VarChar,
                Value = URN,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("sp_GetEditedValues", parameters.ToArray());
            DataTable dtruledat = dtRuleData.Tables[0];
            string path = Server.MapPath("~/UploadFiles/") + "RevitExport.txt";
            System.IO.File.WriteAllText(path, String.Empty);
            if (dtruledat.Rows.Count > 0)
            {
                for (int j = 0; j < dtruledat.Rows.Count; j++)
                {
                    string revit = dtruledat.Rows[j]["Revitid"].ToString();
                    string PropertyName = dtruledat.Rows[j]["Property_Name"].ToString();
                    string PropertyValue = dtruledat.Rows[j]["Property_Value"].ToString();

                    string createText = revit + "," + PropertyName + "," + PropertyValue + Environment.NewLine;

                    System.IO.File.AppendAllText(path, createText);
                }
            }

            //DownloadFile("RevitExport.txt");

            return Json("", JsonRequestBehavior.AllowGet);
        }

        //<<------End of Search Popup Methods ------------>>
    }
}