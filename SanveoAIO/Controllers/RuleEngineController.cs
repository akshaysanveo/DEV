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
    public class RuleEngineController : Controller
    {
        // GET: RuleEngine
        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();

        public ActionResult RuleEngine()
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
            }
            else
                return RedirectToAction("Login", "Login");

            List<SqlParameter> parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@GID",
                SqlDbType = SqlDbType.VarChar,
                Value = Session["GID"],
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@MID",
                SqlDbType = SqlDbType.VarChar,
                Value = 1,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetModuleAccess", parameters.ToArray());
            if (dataSet2.Tables[0].Rows.Count > 0)
            {
                ViewBag.Add_RuleEngine = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Rule Engine")["AR_Add"].ToString();
                ViewBag.Edit_RuleEngine = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Rule Engine")["AR_Edit"].ToString();
                ViewBag.Delete_RuleEngine = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Rule Engine")["AR_Delete"].ToString();
                ViewBag.View_RuleEngine = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Rule Engine")["AR_View"].ToString();

            }

            return View();
        }

        //<------ Start of RuleEngine Popup Methods-------->

        public ActionResult GetRuleGrid([DataSourceRequest] DataSourceRequest request, int? ModuleId)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (ModuleId == null)
            {
                ModuleId = 0;
            }

            var result = db.Database.SqlQuery<GetRuleGridDetails_Result>("GetRuleGridDetails @compid={0},@id={1},@Tradeid={2},@GroupId={3}", user.Comp_ID, ModuleId, user.Trade_ID, user.G_Id).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRuleDetails(string TemplateName, string urn, string rulename)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string ListData = "";
            string catindex = "";
            string startinex = "";
            string endindex = "";
            string Profileid = "";
            string ProfileName = "";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@fileName",
                SqlDbType = SqlDbType.VarChar,
                Value = TemplateName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@RuleName",
                SqlDbType = SqlDbType.VarChar,
                Value = rulename,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dtRuleData = SqlManager.ExecuteDataSet("GetRuleDetails", parameters.ToArray());
            DataTable dtPropdat = dtRuleData.Tables[0];
            DataTable dtcatdat = dtRuleData.Tables[1];
            DataTable dtprofile = dtRuleData.Tables[2];

            ListData += "<div class='col-xs-6' style='padding-left:0px;'><br/>";
            //  ListData += "<table><tr><td><label style='font-size: 16px;'>Excel Category</label></td><td><label style='font-size: 16px;'>Model Category</label></td></tr></table>";
            ListData += "<table id='Categorytable' class='table table-bordered table-striped' style='display:block;width:600px;height:200px;border:1px solid black;overflow-y:scroll;overflow-x:hidden'>";
            ListData += "<thead style='display:table-header-group;overflow: auto;overflow-y: auto;overflow-x: hidden;'><tr><td style='width:17%;'><label>Excel Category</label></td><td style='width:260px;'><label>Model Category</label></td><td style='width:180px;'><label>Category Filter</label></td></tr></thead>";
            ListData += "<tbody style='display:table-header-group;overflow: auto;overflow-y: auto;overflow-x: hidden;'>";

            if (dtcatdat.Rows.Count > 0)
            {
                for (int j = 0; j < dtcatdat.Rows.Count; j++)
                {
                    ListData += "<tr>";
                    ListData += "<td style='width:17%;'>" + dtcatdat.Rows[j]["Category_Name"].ToString() + "</td>";
                    ListData += "<td style='width:260px;'><select id='ddl_" + dtcatdat.Rows[j]["Category_Name"].ToString() + "' style='width:100%;height:30px' class='form-control'>";
                    ListData += "<option value = 'Select' >Select</ option >";

                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@urn",
                        SqlDbType = SqlDbType.VarChar,
                        Value = urn,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    DataSet dtCatData = SqlManager.ExecuteDataSet("GetCommonCategory", parameters1.ToArray());
                    DataTable dtCatdat1 = dtCatData.Tables[0];
                    if (dtCatdat1.Rows.Count > 0)
                    {
                        for (int v = 0; v < dtCatdat1.Rows.Count; v++)
                        {
                            string catname = dtCatdat1.Rows[v]["Category_Name"].ToString();
                            string catmap = dtcatdat.Rows[j]["Category_Map"].ToString();
                            string catnamevalue = dtCatdat1.Rows[v]["Category_Name"].ToString();//.Replace(" ","_");
                            if (catnamevalue == catmap)
                            {
                                ListData += "<option selected='selected' value = '" + catname + "' > " + catname + "</ option >";
                            }
                            else
                            {
                                ListData += "<option value = '" + catname + "' > " + catname + "</ option >";
                            }
                        }
                    }
                    ListData += "</select></td>";
                    ListData += "<td style='width:180px;'><input type = 'text' class='form-control' style='width:100%; height:30px' id = '" + dtcatdat.Rows[j]["Category_Name"].ToString() + "' placeholder='' value='" + dtcatdat.Rows[j]["Filter"].ToString() + "' /></td></tr>";
                }
            }

            //ListData += "</tbody>";
            ListData += "</table></div>";
            string[] profile = new string[] { };

            //if (dtprofile.Rows.Count > 0)
            //{
            //    profile = dtprofile.Rows[0]["Profile"].ToString();
            //}
            //rahin
            ListData += "<div class='col-xs-6'  style='margin-left:0px;'><br/>";
            //ListData +=  "<table width='100%'><tr><td style='width:18%'><label style='font-size: 17px;'>Property</label></td><td style='width:6%'><label style='font-size: 17px;'>Profile</label></td><td style='width:5%'><label style='font-size: 17px;'>Verification</label></td></tr></table>";

            ListData += "<table id='myPrpertyList' class='table table-bordered table-striped' style='display:block;height:200px;width:99%;border:1px solid black;overflow:auto'>";
            ListData += "<thead style='display:table-header-group;overflow:auto;overflow-y:auto;overflow-x:hidden;' ><tr><td style='width:20%'><label >Property</label></td><td  style='width:9%'><label>Profile</label></td><td style='width:5%'><label>Verification</label></td></tr></thead>";
            ListData += "<tbody style='display:table-header-group;overflow:auto;overflow-y:auto;overflow-x:hidden;'>";
            if (dtPropdat.Rows.Count > 0)
            {
                for (int j = 0; j < dtPropdat.Rows.Count; j++)
                {
                    catindex = dtPropdat.Rows[0]["CategoryIndex"].ToString();
                    startinex = dtPropdat.Rows[0]["StartIndex"].ToString();
                    endindex = dtPropdat.Rows[0]["EndIndex"].ToString();
                    Profileid = dtPropdat.Rows[j]["FkProfile"].ToString();
                    ProfileName = dtPropdat.Rows[j]["Profile"].ToString();
                    ListData += "<tr>";
                    ListData += "<td style='width:20%'>" + dtPropdat.Rows[j]["PropertyName"].ToString() + "</td>";
                    ListData += "<td  style='width:15%'>";
                    ListData += "<select id='ddl_" + dtPropdat.Rows[j]["PropertyName"].ToString() + "' style='width:90%;height:28px' class='form-control'>";
                    int selected = 0;
                    for (int i = 0; i < dtprofile.Rows.Count; i++)
                    {
                        string ProfileID = dtprofile.Rows[i]["ID"].ToString();
                        string ProfileNameMst = dtprofile.Rows[i]["Profile"].ToString();
                       
                        if (ProfileName == ProfileNameMst)
                        {
                            ListData += "<option value='" + ProfileID + "' selected>" + ProfileNameMst + "</option>";
                            selected = 1;
                        }
                        else
                        {
                            if (ProfileName == "Office")
                            {
                                if (selected == 0)
                                    ListData += "<option value='" + ProfileID + "' selected>" + ProfileNameMst + "</option>";
                                else
                                    ListData += "<option value='" + ProfileID + "'>" + ProfileNameMst + "</option>";
                            }
                            else { ListData += "<option value='" + ProfileID + "'>" + ProfileNameMst + "</option>"; }
                        }


                    }
                    ListData += "</select>";
                    ListData += "</td>";
                    ListData += "<td  style='width:5%'><select id='ddl_" + dtPropdat.Rows[j]["PropertyName"].ToString() + "_YN' class='form-control' style='width:80px; height:28px'>";
                    if (dtPropdat.Rows[j]["Verify"].ToString() == "Yes")
                    {
                        ListData += "<option value='Yes' selected>Yes</option>";
                        ListData += "<option value='No'>No</option>";
                    }
                    else if (dtPropdat.Rows[j]["Verify"].ToString() == "No")
                    {
                        ListData += "<option value='Yes'>Yes</option>";
                        ListData += "<option value='No' selected>No</option>";
                    }
                    else
                    {
                        ListData += "<option value='Yes'>Yes</option>";
                        ListData += "<option value='No' selected>No</option>";
                    }
                    ListData += "</select></td>";
                    ListData += "</tr>";
                }
            }
            ListData += "</tbody></table>";
            ListData += "</div>";

            return Json(new { catindex = catindex, startinex = startinex, endindex = endindex, ListData = ListData }, "text/plain");
        }

        public JsonResult SaveEditedRuleDetails(int flag, string PropertyData, string filename, string indexvalue, string filter, string Rulename, string PropValues, string tradeId)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            filter = filter.Substring(0, filter.Length - 1);

            var TotalFilter = filter.Split('#');

            var TotalProp = PropValues.Split(new string[] { "#$" }, StringSplitOptions.None);
            string catvalue = "";
            string caname = "";
            string catmaping = "";
            string propprofile = "";
            string propverify = "";
            string propname = "";

            var indexno = indexvalue.Split(',');

            // Flag 0 for Eding Commond
            if (flag == 0)
            {
                for (int j = 0; j < TotalFilter.Length; j++)
                {
                    var ff = TotalFilter[j].Split('~');
                    caname = ff[0].ToString();
                    catvalue = ff[1].ToString();
                    catmaping = ff[2].ToString();

                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@fileName",
                        SqlDbType = SqlDbType.VarChar,
                        Value = filename,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@categoryName",
                        SqlDbType = SqlDbType.VarChar,
                        Value = caname,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@categoryFilter",
                        SqlDbType = SqlDbType.VarChar,
                        Value = catvalue,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@catMaping",
                        SqlDbType = SqlDbType.VarChar,
                        Value = catmaping,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@Compid",
                        SqlDbType = SqlDbType.VarChar,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@rulename",
                        SqlDbType = SqlDbType.VarChar,
                        Value = Rulename,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@CurrentUserId",
                        SqlDbType = SqlDbType.Int,
                        Value = user.U_Id,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dtRuleData = SqlManager.ExecuteDataSet("Sp_SaveCategoryFilter", parameters.ToArray());
                }

                for (int j = 0; j < TotalProp.Length - 1; j++)
                {
                    var ff = TotalProp[j].Split('~');
                    propname = ff[0].ToString();
                    propverify = ff[1].ToString();
                    propprofile = ff[2].ToString();

                    var finaldat1a = db.Sp_SaveEditRunEngine(filename, Rulename, indexno[0].ToUpper(), indexno[1].ToUpper(), indexno[2].ToUpper(), int.Parse(indexno[3]), int.Parse(indexno[4]), user.Comp_ID, user.U_Id, propname, Convert.ToInt16(propprofile), propverify);
                }
            }

            // Flag 1 for Inserting Commond
            if (flag == 1)
            {
                // For Deleting Previous Category of RuleName
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@fileName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = filename,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Compid",
                    SqlDbType = SqlDbType.VarChar,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@RuleName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Rulename,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_DeleteCategoryDetails", parameters.ToArray());

                //For Storing New  Category Rule Data
                for (int j = 0; j < TotalFilter.Length; j++)
                {
                    var ff = TotalFilter[j].Split('~');
                    caname = ff[0].ToString();
                    catvalue = ff[1].ToString();
                    catmaping = ff[2].ToString();

                    var finaldata = db.SaveCategoryFilter(filename, caname, catvalue, catmaping, user.Comp_ID, user.U_Id, Rulename, Convert.ToInt16(tradeId));
                }


                /// For Property Data Save
                if ((indexno[0].ToUpper() != "") || (indexno[1].ToUpper() != "") || (indexno[2].ToUpper() != "") || (filename != ""))
                {
                    //For Deleting Prevoius Property RuleName Data
                    db.SP_DeleteExistingRule(Rulename, user.Comp_ID);

                    //For Storing New Property Rule Data
                    for (int j = 0; j < TotalProp.Length - 1; j++)
                    {
                        var ff = TotalProp[j].Split('~');
                        propname = ff[0].ToString();
                        propverify = ff[1].ToString();
                        propprofile = ff[2].ToString();

                        var finaldata = db.SP_SaveRuleData(Rulename, propname, indexno[0].ToUpper(), indexno[1].ToUpper(), indexno[2].ToUpper(), filename.ToString(), int.Parse(indexno[3]), int.Parse(indexno[4]), user.Comp_ID, user.U_Id, Convert.ToInt16(propprofile), propverify, Convert.ToInt16(tradeId));
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteRuleDetails(string RuleName, string CategoryIndex, string StartIndex, string EndIndex, string FileName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@fileName",
                SqlDbType = SqlDbType.VarChar,
                Value = FileName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@RuleName",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dtRuleData = SqlManager.ExecuteDataSet("DeleteRuleDetails", parameters.ToArray());
            // DataTable dtPropdat = dtRuleData.Tables[0];


            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(IEnumerable<HttpPostedFileBase> files, string indexvalues, string urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string tabledata = "";
            string ListData = "";
            string storecategory = "";
            if (indexvalues != null)
            {
                var indexno1 = indexvalues.Split(',');
                int catno = NumberFromExcelColumn(indexno1[0]);
                int startno = NumberFromExcelColumn(indexno1[1]);
                int endno = NumberFromExcelColumn(indexno1[2]);
                int StartRowNumber = Convert.ToInt32(indexno1[3]);
                int SheetNumber = Convert.ToInt32(indexno1[4]);
                string finalnos = catno + "," + startno + "," + endno;
                List<SqlParameter> Profileparameters = new List<SqlParameter>();
                Profileparameters.Add(new SqlParameter()
                {
                    ParameterName = "@Compid",
                    SqlDbType = SqlDbType.VarChar,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetRuleProfile", Profileparameters.ToArray());
                DataTable dtprofile = dtRuleData.Tables[0];
                var indexno = finalnos.Split(',');
                if (indexno.Length == 3)
                {
                    // The Name of the Upload component is "files"
                    if (files != null)
                    {
                        foreach (var uploadfile in files)
                        {
                            var fileName = System.IO.Path.GetFileName(uploadfile.FileName);
                            var physicalPath = System.IO.Path.Combine(Server.MapPath("~/UploadFiles"), fileName);
                            if (uploadfile != null && uploadfile.ContentLength > 0)
                            {
                                //ExcelDataReader works on binary excel file
                                Stream stream = uploadfile.InputStream;
                                IExcelDataReader reader = null;
                                if (uploadfile.FileName.EndsWith(".xls"))
                                {
                                    //reads the excel file with .xls extension
                                    uploadfile.SaveAs(physicalPath);
                                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                                }
                                else if (uploadfile.FileName.EndsWith(".xlsx"))
                                {
                                    uploadfile.SaveAs(physicalPath);
                                    //reads excel file with .xlsx extension
                                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                                }
                                else
                                {
                                    //Shows error if uploaded file is not Excel file
                                    ModelState.AddModelError("File", "This file format is not supported");
                                    //return View();
                                }
                                //treats the first row of excel file as Coluymn Names
                                //  reader.IsFirstRowAsColumnNames = false;
                                //Adding reader data to DataSet()
                                DataSet result = reader.AsDataSet();
                                DataTable datatable_old = result.Tables[SheetNumber - 1];
                                int startrow = StartRowNumber;

                                DataTable datatable = datatable_old.AsEnumerable()
                                              .Where((row, index) => index >= startrow - 1)
                                              .CopyToDataTable();

                                foreach (DataColumn column in datatable.Columns)
                                {
                                    string cName = datatable.Rows[0][column.ColumnName].ToString();
                                    if (!datatable.Columns.Contains(cName) && cName != "")
                                    {
                                        column.ColumnName = cName;
                                    }

                                }

                                DataRow row1 = datatable.Rows[0];
                                datatable.Rows.Remove(row1);
                                //datatable.Rows[0].Delete();

                                DataRow dr1 = result.Tables[0].NewRow();
                                for (int i = 0; i < result.Tables[0].Columns.Count; i++)
                                {
                                    dr1[result.Tables[0].Columns[i].ColumnName] = result.Tables[0].Rows[0][i];
                                    // AddData = result.Tables[0].Rows[0][i];
                                }

                                ArrayList arr = new ArrayList();
                                tabledata += "<table id='TblGrid' style='width:1200%'>";
                                tabledata += "<thead>";
                                tabledata += "<tr>";

                                int j1 = 0;
                                foreach (DataColumn dr in datatable.Columns)
                                {
                                    if (j1 == int.Parse(indexno[0]) || j1 >= int.Parse(indexno[1]) && j1 <= int.Parse(indexno[2]))
                                    {
                                        if (dr.ColumnName.Trim() != "")
                                        {
                                            tabledata += "<th data-field=" + '"' + dr.ColumnName.Trim() + '"' + ">" + dr.ColumnName.Trim() + "</th>";
                                        }
                                    }
                                    j1++;
                                }
                                tabledata += "</tr>";
                                tabledata += "</thead>";
                                tabledata += "<tbody>";

                                ListData += "<nav><div class='col-xs-6' style=' padding-left: 0px;height:40px'><br/>";
                                //  ListData += "<label for='' style='font-size: 17px; '>Excel Category</label><label for='' style='font-size: 16px;padding-left:12%'>Model Category</label></br>";
                                ListData += "<table id='Categorytable' style='display:block;width:600px;height:200px;border:1px solid black; overflow-y:scroll;overflow-x:hidden;' class='table table-bordered table-striped'>";
                                ListData += "<thead style='display:table-header-group;overflow: auto;overflow-y: auto;overflow-x: hidden;'><tr><td style='width:17%;'><label>Excel Category</label></td><td style='width:260px;'><label>Model Category</label></td><td style='width:180px;'><label>Category Filter</label></td></tr></thead>";
                                ListData += "<tbody style=' display:table-header-group;;overflow: auto;overflow-y: auto;overflow-x: hidden;'>";

                                foreach (DataRow dr in datatable.Rows)
                                {
                                    int loop = 0;
                                    int k = 0;
                                    foreach (DataColumn col in datatable.Columns)
                                    {
                                        if (k == int.Parse(indexno[0]) || k >= int.Parse(indexno[1]) && k <= int.Parse(indexno[2]))
                                        {
                                            if (loop == 0)
                                            {
                                                tabledata += "<tr>";
                                            }
                                            if (dr[col.ColumnName].ToString() != "")
                                            {
                                                tabledata += "<td style='width:17%;'>" + dr[col.ColumnName].ToString() + "</td>";
                                                if (k == int.Parse(indexno[0]))
                                                {
                                                    ListData += "<tr>";
                                                    arr.Add(dr[col.ColumnName].ToString());
                                                    ListData += "<td style='width:17%;'>" + dr[col.ColumnName].ToString() + "</td>";
                                                    ListData += "<td style='width:260px;'><select id='ddl_" + dr[col.ColumnName].ToString() + "' style='width:100%; height:30px' class='form-control'>";
                                                    ListData += "<option value = 'Select' >Select</ option >";

                                                    //For Get Model Categorys
                                                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                                                    parameters1.Add(new SqlParameter()
                                                    {
                                                        ParameterName = "@urn",
                                                        SqlDbType = SqlDbType.VarChar,
                                                        Value = urn,
                                                        Direction = System.Data.ParameterDirection.Input
                                                    });
                                                    parameters1.Add(new SqlParameter()
                                                    {
                                                        ParameterName = "@compid",
                                                        SqlDbType = SqlDbType.Int,
                                                        Value = user.Comp_ID,
                                                        Direction = System.Data.ParameterDirection.Input
                                                    });
                                                    DataSet dtCatData = SqlManager.ExecuteDataSet("GetCommonCategory", parameters1.ToArray());

                                                    DataTable dtCatdat = dtCatData.Tables[0];

                                                    // DataSet dtCatData = SqlManager.ExecuteDataSet("GetCommonCategory");
                                                    //  DataTable dtCatdat = dtCatData.Tables[0];
                                                    if (dtCatdat.Rows.Count > 0)
                                                    {
                                                        for (int v = 0; v < dtCatdat.Rows.Count; v++)
                                                        {
                                                            string catname = dtCatdat.Rows[v]["Category_Name"].ToString();
                                                            string catnamevalue = dtCatdat.Rows[v]["Category_Name"].ToString(); //.Replace(" ","_");
                                                            ListData += "<option value = '" + catname + "' > " + catname + "</ option >";
                                                        }
                                                    }

                                                    ListData += "</select></td>";
                                                    ListData += "<td style='width:180px;'><input type = 'text'  class='form-control' style='width:180px; height:30px' id = '" + dr[col.ColumnName].ToString() + "' placeholder='' /></td></tr>";

                                                    storecategory += dr[col.ColumnName].ToString() + "|";
                                                }
                                            }
                                            else
                                            {
                                                tabledata += "<td></td>";
                                            }

                                            loop++;
                                            if (loop == datatable.Columns.Count)
                                            {
                                                tabledata += "</tr>";
                                            }
                                        }
                                        k++;
                                    }

                                }

                                ListData += "<tbody>";
                                ListData += "</table></div></nav>";

                                // var value2 = storecategory.Split('|');

                                storecategory = storecategory.Substring(0, storecategory.Length - 1);
                                // for (int i = 0; i < value2.Length - 1; i++)
                                //  {
                                var resudslt = db.SaveRuleExcelCategory(fileName, storecategory, user.Comp_ID, user.U_Id);
                                //}

                                //string[] profile = new string[] { };
                                //if (dtprofile.Rows.Count > 0)
                                //{
                                //    profile = dtprofile.Rows[0]["Profile"].ToString().Split(',');
                                //}
                                //rahin
                                ListData += "<div class='col-xs-6' style='margin-left:0px'><br/>";
                                // ListData += "<label for='' style='font-size: 17px; '>Property</label><label style='font-size: 17px;'>Profile</label><label style='font-size: 17px;'>Verification</label></br>";
                                ListData += "<table id='myPrpertyList' style='display:block;height:200px;width:99%;border:1px solid black;overflow:auto' class='table table-bordered table-striped'> ";
                                ListData += "<thead style='display:table-header-group;overflow:auto;overflow-y:auto;overflow-x:hidden;' ><tr><td style='width:20%'><label >Property</label></td><td  style='width:9%'><label>Profile</label></td><td style='width:5%'><label'>Verification</label></td></tr></thead>";
                                ListData += "<tbody style='display:table-header-group;overflow:auto;overflow-y:auto;overflow-x:hidden;'>";
                                int j = 0;
                                foreach (DataColumn dr in datatable.Columns)
                                {
                                    if (j >= int.Parse(indexno[1]) && j <= int.Parse(indexno[2]))
                                    {
                                        if (dr.ColumnName.Trim() != "")
                                        {
                                            string profileid = dr.ColumnName.Trim().Replace("&", "_").ToString();
                                            int DashEndIndex = profileid.IndexOf("-") + 1;
                                            profileid = profileid.Substring(DashEndIndex);

                                            ListData += "<td style='width:20%'>" + profileid + "</td>";
                                            ListData += "<td style='width:9%'>";
                                            ListData += "<select id='ddl_" + profileid + "' style='width:80%; height:28px' class='form-control'>";
                                            for (int i = 0; i < dtprofile.Rows.Count; i++)
                                            {
                                                string ProfileID = dtprofile.Rows[i]["ID"].ToString();
                                                string ProfileName = dtprofile.Rows[i]["Profile"].ToString();
                                                if (ProfileName == "Office")
                                                    ListData += "<option value='" + ProfileID + "' selected>" + ProfileName + "</option>";
                                                else
                                                    ListData += "<option value='" + ProfileID + "'>" + ProfileName + "</option>";

                                            }
                                            ListData += "</select>";
                                            ListData += "</td>";
                                            ListData += "<td style='width:5%'><select id='ddl_" + profileid + "_YN' class='form-control'  style='width:80px; height:28px'>";
                                            ListData += "<option value='Yes'>Yes</option>";
                                            ListData += "<option value='No' selected>No</option>";
                                            ListData += "</select></td>";
                                            ListData += "</tr>";
                                        }
                                    }
                                    j++;
                                }
                                ListData += "</tbody></table></div>";
                                tabledata += "</tbody>";
                                tabledata += "</table>";
                                tabledata += "<script>$('#TblGrid').kendoGrid({" +
                                    "height:200," +
                                    "sortable:true," +
                                     "resizable:true" +
                                "});</script>";
                            }
                        }
                    }
                }
            }

            return Json(new { tabledata = tabledata, ListData = ListData }, "text/plain");
        }


        public ActionResult Remove(string[] fileNames)
        {
            // The parameter of the Remove action must be called "fileNames"
            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = System.IO.Path.GetFileName(fullName);
                    var physicalPath = System.IO.Path.Combine(Server.MapPath("~/UploadFiles"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        // The files are not actually removed in this demo
                        // System.IO.File.Delete(physicalPath);
                    }
                }
            }

            // Return an empty string to signify success
            return Content("");
        }

        public static int NumberFromExcelColumn(string column)
        {
            int retVal = 0;
            string col = column.ToUpper();
            for (int iChar = col.Length - 1; iChar >= 0; iChar--)
            {
                char colPiece = col[iChar];
                int colNum = colPiece - 64;
                retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
            }
            return retVal - 1;
        }

        public JsonResult GetddlTrade()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            IEnumerable<SP_GetUserTrade_Result> items = db.Database.SqlQuery<SP_GetUserTrade_Result>("SP_GetUserTrade @CompId={0}", user.Comp_ID).ToList().Select(c => new SP_GetUserTrade_Result
            {
                Id = c.Id,
                Name = c.Name
            });
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        //<<------End of RuleEngine Popup Methods ------------>>
    }
}
