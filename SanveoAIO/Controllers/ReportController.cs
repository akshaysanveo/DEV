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
    public class ReportController : Controller
    {
        // GET: Report

        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();
        public ActionResult Report()
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
                Value = 2,
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

                ViewBag.Add_Report = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Report")["AR_Add"].ToString();
                ViewBag.Edit_Report = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Report")["AR_Edit"].ToString();
                ViewBag.Delete_Report = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Report")["AR_Delete"].ToString();
                ViewBag.View_Report = dataSet2.Tables[0].AsEnumerable().First(r => r.Field<string>("ModuleName") == "Report")["AR_View"].ToString();

            }

            return View();
        }

        public JsonResult GetModelUrnGridData([DataSourceRequest] DataSourceRequest request, string NodeName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var data = db.Database.SqlQuery<GetSaveModelUrnDetails_Result>("EXEC GetSaveModelUrnDetails @compid={0},@userid={1},@FileName={2}", user.Comp_ID, user.U_Id, NodeName).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetCategoryData(string URN, string ModelName, string Templatefile, string versionNo, string RuleNamee)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (URN != "" && ModelName != "" && Templatefile != "" && versionNo != "" && RuleNamee != "")
            {
                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = URN,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@ModelName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ModelName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Filename",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Templatefile,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CompId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@versionNo",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt16(versionNo),
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@RuleName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = RuleNamee,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_ReportExcelValues", parameters.ToArray());
                //  DataTable dt24 = dataSet.Tables[0];
            }

            return Json(new { dtable = "", fname = "" }, "text/plain");
        }

        public JsonResult GetCategoryHistoryData(string URN, string ModelName, string RuleName, string versionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string filename = "";
            string indexvalue = "";
            string catindex = "";
            string startinex = "";
            string endindex = "";
            String SheetNumber = "";
            String Template = "";
            string[] datarray = new string[5000000];

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RuleName",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetRuleData", parameters1.ToArray());
            DataTable dtruledat = dtRuleData.Tables[0];

            if (dtruledat.Rows.Count > 0)
            {
                for (int j = 0; j < dtruledat.Rows.Count; j++)
                {
                    catindex = dtruledat.Rows[0]["CategoryIndex"].ToString();
                    startinex = dtruledat.Rows[0]["StartIndex"].ToString();
                    endindex = dtruledat.Rows[0]["EndIndex"].ToString();
                    SheetNumber = dtruledat.Rows[0]["SheetNumber"].ToString();
                    Template = dtruledat.Rows[0]["FileName"].ToString();
                }

            }

            filename = ModelName + ".xlsx";
            string path = Server.MapPath("~/UploadFiles/") + Template;

            string src = Server.MapPath("~/UploadFiles/") + filename;

            System.IO.File.Copy(path, src, true);

            indexvalue = catindex + "," + startinex + "," + endindex + "," + SheetNumber;


            List<SqlParameter> parameters2 = new List<SqlParameter>();
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@FileName",
                SqlDbType = SqlDbType.VarChar,
                Value = Template,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
                SqlDbType = SqlDbType.VarChar,
                Value = versionNo,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@ModelName",
                SqlDbType = SqlDbType.VarChar,
                Value = ModelName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@Rulename",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRulehistory = SqlManager.ExecuteDataSet("SP_GetExcelHistoryRuleData", parameters2.ToArray());
            DataTable dtrulehistorydata = dtRulehistory.Tables[0];

            if (dtrulehistorydata.Rows.Count > 0)
            {
                int catname = 1;
                string ddd = "";
                int catloop = 1;
                int good = 1;

                for (int j = 0; j < dtrulehistorydata.Rows.Count; j++)
                {
                    ddd = dtrulehistorydata.Rows[j]["Category_Name"].ToString();

                    if (dtrulehistorydata.Rows[j]["RuleValues"].ToString().Length != 0)
                    {
                        datarray[catloop] = dtrulehistorydata.Rows[j]["RuleValues"].ToString();
                    }

                    if (catname == 1)
                    {
                        datarray[0] = ddd;
                        catname = 2;
                    }

                    if (catname == 2 && ddd != datarray[0])
                    {
                        good = 2;
                        if (dtrulehistorydata.Rows[j]["RuleValues"].ToString().Length != 0)
                        {
                            Array.Resize(ref datarray, catloop);
                            ReadData(datarray, filename, indexvalue);
                            datarray = new string[5000000];

                            datarray[0] = ddd;
                            datarray[1] = dtrulehistorydata.Rows[j]["RuleValues"].ToString();

                            catloop = 2;

                            catname = 1;
                        }
                        else
                        {
                            if (catloop > 2)
                            {
                                Array.Resize(ref datarray, catloop);
                                ReadData(datarray, filename, indexvalue);
                                datarray = new string[5000000];
                            }

                            datarray[0] = ddd;
                            Array.Resize(ref datarray, 1);
                            ReadData(datarray, filename, indexvalue);

                            datarray = new string[5000000];

                            catloop = 1;
                            catname = 2;

                        }

                    }
                    else
                    {

                        catloop++;
                    }

                }

                if (good == 2 && ddd == datarray[0])
                {
                    Array.Resize(ref datarray, catloop);
                    ReadData(datarray, filename, indexvalue);
                }

                if (good == 1 && ddd == datarray[0])
                {
                    Array.Resize(ref datarray, catloop);
                    ReadData(datarray, filename, indexvalue);
                }
            }

            return Json(new { dtable = "", fname = filename }, "text/plain");
        }

        int hi = 0;
        public string ReadData(string[] fulldata, string filename, string indexvalue)
        {
            var indexcolumn = indexvalue.Split(',');

            int catno = NumberFromExcelColumn(indexcolumn[0]);
            int startno = NumberFromExcelColumn(indexcolumn[1]);
            int endno = NumberFromExcelColumn(indexcolumn[2]);
            int SheetNumber = Convert.ToInt32(indexcolumn[3]);
            string finalnos = catno + "," + startno + "," + endno;

            var indexno = finalnos.Split(',');

            if (indexno.Length == 3)
            {
                string filePath = Server.MapPath("~/UploadFiles/") + filename;
                var fileinfo = new FileInfo(filePath);
                var category = fulldata[0];
                using (ExcelPackage p = new ExcelPackage(fileinfo))
                {
                    //  ExcelWorksheet workSheet = p.Workbook.Worksheets.First();
                    ExcelWorksheet workSheet = p.Workbook.Worksheets[SheetNumber];
                    DataTable dt = new DataTable();
                    int t = workSheet.Dimension.End.Column;
                    int insrow = 1;
                    int r = 2;

                    int rowval = 1;
                    int cellval = 1;

                    cellval = workSheet.Dimension.End.Column;
                    rowval = workSheet.Dimension.End.Row;

                    //for (rowval = workSheet.Dimension.Start.Row; rowval <= workSheet.Dimension.End.Row; rowval++) //(IXLRow row in workSheet.Rows())
                    for (int kk = 0; kk < rowval; kk++)
                    {
                        insrow = 1;
                        // for (cellval = workSheet.Dimension.Start.Column; cellval <= workSheet.Dimension.End.Column; cellval++) //(IXLCell cell in row.Cells())
                        for (int j = 0; j < cellval; j++)
                        {
                            if (insrow == (int.Parse(indexno[0]) + 1) || (insrow >= (int.Parse(indexno[1]) + 1) && insrow <= (int.Parse(indexno[2]) + 1)))// || insrow >= 12)
                            {
                                string value = workSheet.Cells[r, insrow].Text; //string value = workSheet.Cells(r, insrow).Value.ToString();

                                if (fulldata.Length == 1)
                                {
                                    if (value.Trim() == fulldata[0].Trim() && value.Trim() != "")
                                    {
                                        int i = kk + 1;
                                        //  int insertrow = finaldata.Length - 1;
                                        // int insertrow = fulldata.Length - 1;
                                        // int rowadd = i + 1;
                                        int rowadd = i + 2;

                                        // workSheet.InsertRow(rowadd, insertrow);
                                        //p.Save();

                                        int y = i + 1;
                                        // for (int l = 1; l <= fulldata.Length - 1; l++)
                                        //{
                                        // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);

                                        // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);
                                        // var data = fdata.Split(',');
                                        int k = 1;
                                        int m = 3;

                                        //for (cellvalx = workSheet.Dimension.Start.Column; cellvalx <= workSheet.Dimension.End.Column; cellvalx++) //foreach (IXLCell cell1 in row.Cells())
                                        for (int jj = 0; jj < cellval; jj++)
                                        {

                                            if (k == (int.Parse(indexno[0]) + 1) || (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1)))
                                            {
                                                if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                {

                                                }
                                                else
                                                {
                                                    //workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                    //workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                                }

                                                if (k == (int.Parse(indexno[0]) + 1))
                                                {

                                                    workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                    workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                                }

                                                m++;

                                            }



                                            k++;
                                        }
                                        y++;
                                        // }
                                    }
                                }

                                if (fulldata.Length >= 2)
                                {
                                    if (value.Trim() == fulldata[0].Trim())
                                    {
                                        int i = kk + 1;
                                        //  int insertrow = finaldata.Length - 1;
                                        int insertrow = fulldata.Length - 1;
                                        // int rowadd = i + 1;
                                        int rowadd = i + 2;

                                        workSheet.InsertRow(rowadd, insertrow);


                                        p.Save();

                                        int y = i + 2;
                                        for (int l = 1; l <= fulldata.Length - 1; l++)
                                        {
                                            var fdata = fulldata[l];

                                            // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);
                                            var data = fdata.Split(new string[] { "~}" }, StringSplitOptions.None);
                                            // var data = fdata.Split("~}");
                                            int k = 1;
                                            int m = 4;

                                            //for (cellvalx = workSheet.Dimension.Start.Column; cellvalx <= workSheet.Dimension.End.Column; cellvalx++) //foreach (IXLCell cell1 in row.Cells())
                                            for (int jj = 0; jj < cellval; jj++)
                                            {
                                                //if (k == 2)
                                                //{
                                                //    if (data[m].Trim() == "")
                                                //    {
                                                //        workSheet.Cells[y, k].Value = "";
                                                //    }
                                                //    else
                                                //    {
                                                //        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                //        {

                                                //        }
                                                //        else
                                                //        {

                                                //            workSheet.Cells[y, k].Value = data[3];


                                                //            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                //            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                                                //            // range.Style.Border.Top.Color.SetColor(Color.Red);
                                                //        }

                                                //    }


                                                //}

                                                if (k == 1)
                                                {
                                                    if (data[m].Trim() == "")
                                                    {
                                                        workSheet.Cells[y, k].Value = "";
                                                    }
                                                    else
                                                    {
                                                        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                        {

                                                        }
                                                        else
                                                        {

                                                            workSheet.Cells[y, k].Value = data[1];
                                                            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                        }

                                                    }

                                                }

                                                //if (k == 3)
                                                //{
                                                //    if (data[m].Trim() == "")
                                                //    {
                                                //        workSheet.Cells[y, k].Value = "";
                                                //    }
                                                //    else
                                                //    {
                                                //        if (k >= (int.Parse(indexno[1]) + 1) && k < (int.Parse(indexno[2]) + 1))
                                                //        {

                                                //        }
                                                //        else
                                                //        {

                                                //            workSheet.Cells[y, k].Value = data[2];
                                                //            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                //            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                //        }

                                                //    }

                                                //}

                                                if (k == (int.Parse(indexno[0]) + 1) || (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1)))
                                                {
                                                    if (data[m].Trim() == "0.000 ft")
                                                    { data[m] = "Blank"; }

                                                    if (data[m].Trim() == "")
                                                    {
                                                        workSheet.Cells[y, k].Value = "family name missing";
                                                    }
                                                    else
                                                    {
                                                        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                        {
                                                            if (data[m].Trim() == "NA")
                                                            {
                                                                // workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Red;
                                                            }
                                                            else if (data[m].Trim() == "Blank")
                                                            {
                                                                // workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Yellow;
                                                            }
                                                            else
                                                            {
                                                                workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Green;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            workSheet.Cells[y, k].Value = data[0];

                                                            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                        }
                                                    }
                                                    m++;

                                                }



                                                k++;
                                            }
                                            y++;
                                        }
                                    }
                                }
                            }
                            insrow++;
                        }
                        r++;
                    }
                    p.Save();
                    // workBook.SaveAs(filePath);
                }

            }
            return string.Empty;
        }

        public JsonResult GetTemplateName()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            IEnumerable<GetTemplateName_Result> items = db.Database.SqlQuery<GetTemplateName_Result>("GetTemplateName @Compid={0},@GroupId={1},@TradeId={2}", user.Comp_ID, user.G_Id, user.Trade_ID).ToList().Select(c => new GetTemplateName_Result
            {

                RuleName = c.RuleName,
                FileName = c.FileName
            });
            return Json(items, JsonRequestBehavior.AllowGet);


        }

        public string GetFiledetails([DataSourceRequest] DataSourceRequest request, string ModelName, string versionNo, string rulename)
        {
            int VersionId = 0;

            if (!(int.TryParse(versionNo, out VersionId)))
            {
                VersionId = 0;
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            UserInfo user = new UserInfo();
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            else
            {
                throw new Exception("session expired");
            }
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@ModelName",
                SqlDbType = SqlDbType.VarChar,
                Value = ModelName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Versionno",
                SqlDbType = SqlDbType.VarChar,
                Value = VersionId,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@RuleName",
                SqlDbType = SqlDbType.VarChar,
                Value = rulename,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet = SqlManager.ExecuteDataSet("GetFiledetails", parameters.ToArray());
            DataTable dt = dataSet.Tables[0];
            string Dextract = dt.Rows[0]["Dextract"].ToString();
            string FileName = dt.Rows[0]["FileName"].ToString();
            string ModifiedDate = dt.Rows[0]["ModifiedDate"].ToString();
            string RunRule = dt.Rows[0]["RunRule"].ToString();
            string Template = dt.Rows[0]["Template"].ToString();
            string Urn = dt.Rows[0]["Urn"].ToString();


            return Newtonsoft.Json.JsonConvert.SerializeObject(new { Dextract = Dextract, FileName = FileName, ModifiedDate = ModifiedDate, RunRule = RunRule, Template = Template, Urn = Urn });


            //var data = db.Database.SqlQuery<GetFiledetails_Result>("EXEC GetFiledetails @ModelName={0},@Versionno={1}, @compid={2}", ModelName, VersionId, user.Comp_ID).ToList();
            //DataSourceResult result = data.ToDataSourceResult(request);
            //return Json(result, JsonRequestBehavior.AllowGet);
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

        #region  GetExcelDataReport
        public string GetExcelDataReport(string FileName, string ModelName, int VersionNo, string RuleName)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@ModelName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ModelName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Filename",
                    SqlDbType = SqlDbType.VarChar,
                    Value = FileName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@RuleName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = RuleName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CompId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@versionNo",
                    SqlDbType = SqlDbType.Int,
                    Value = VersionNo,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet ds = SqlManager.ExecuteDataSet("SP_GetExcelDataInHtml", parameters.ToArray());
                string data = JsonConvert.SerializeObject(ds);
                return data;

            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public JsonResult UpdateExcelHistoryData(string FileName, string ModelName, int VersionNo, string RuleName, string RuleValues, int ID)
        {
            try
            {

                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@ModelName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ModelName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Filename",
                    SqlDbType = SqlDbType.VarChar,
                    Value = FileName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@RuleName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = RuleName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CompId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@versionNo",
                    SqlDbType = SqlDbType.Int,
                    Value = VersionNo,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@RuleValues",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = RuleValues,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@ID",
                    SqlDbType = SqlDbType.Int,
                    Value = ID,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@current_uid",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet ds = SqlManager.ExecuteDataSet("SP_UpdateExcelHistory", parameters.ToArray());
                return Json("", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCategoryHistoryDataCopy(string URN, string ModelName, string RuleName, string versionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string filename = "";
            string indexvalue = "";
            string catindex = "";
            string startinex = "";
            string endindex = "";
            String SheetNumber = "";
            String Template = "";
            string[] datarray = new string[5000000];

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RuleName",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetRuleData", parameters1.ToArray());
            DataTable dtruledat = dtRuleData.Tables[0];

            if (dtruledat.Rows.Count > 0)
            {
                for (int j = 0; j < dtruledat.Rows.Count; j++)
                {
                    catindex = dtruledat.Rows[0]["CategoryIndex"].ToString();
                    startinex = dtruledat.Rows[0]["StartIndex"].ToString();
                    endindex = dtruledat.Rows[0]["EndIndex"].ToString();
                    SheetNumber = dtruledat.Rows[0]["SheetNumber"].ToString();
                    Template = dtruledat.Rows[0]["FileName"].ToString();
                }

            }

            filename = ModelName + ".xlsx";
            string path = Server.MapPath("~/UploadFiles/") + Template;

            string src = Server.MapPath("~/UploadFiles/") + filename;

            System.IO.File.Copy(path, src, true);

            indexvalue = catindex + "," + startinex + "," + endindex + "," + SheetNumber;


            List<SqlParameter> parameters2 = new List<SqlParameter>();
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@FileName",
                SqlDbType = SqlDbType.VarChar,
                Value = Template,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
                SqlDbType = SqlDbType.VarChar,
                Value = versionNo,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@ModelName",
                SqlDbType = SqlDbType.VarChar,
                Value = ModelName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@Rulename",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRulehistory = SqlManager.ExecuteDataSet("SP_GetExcelHistoryRuleDataCopy", parameters2.ToArray());
            DataTable dtrulehistorydata = dtRulehistory.Tables[0];

            if (dtrulehistorydata.Rows.Count > 0)
            {
                int catname = 1;
                string ddd = "";
                int catloop = 1;
                int good = 1;

                for (int j = 0; j < dtrulehistorydata.Rows.Count; j++)
                {
                    ddd = dtrulehistorydata.Rows[j]["Category_Name"].ToString();

                    if (dtrulehistorydata.Rows[j]["RuleValues"].ToString().Length != 0)
                    {
                        datarray[catloop] = dtrulehistorydata.Rows[j]["RuleValues"].ToString() + "]" + dtrulehistorydata.Rows[j]["Edited"].ToString();
                    }

                    if (catname == 1)
                    {
                        datarray[0] = ddd;
                        catname = 2;
                    }

                    if (catname == 2 && ddd != datarray[0])
                    {
                        good = 2;
                        if (dtrulehistorydata.Rows[j]["RuleValues"].ToString().Length != 0)
                        {
                            Array.Resize(ref datarray, catloop);
                            ReadDataCopy(datarray, filename, indexvalue);
                            datarray = new string[5000000];

                            datarray[0] = ddd;
                            datarray[1] = dtrulehistorydata.Rows[j]["RuleValues"].ToString() + "]" + dtrulehistorydata.Rows[j]["Edited"].ToString();

                            catloop = 2;

                            catname = 1;
                        }
                        else
                        {
                            if (catloop > 2)
                            {
                                Array.Resize(ref datarray, catloop);
                                ReadDataCopy(datarray, filename, indexvalue);
                                datarray = new string[5000000];
                            }

                            datarray[0] = ddd;
                            Array.Resize(ref datarray, 1);
                            ReadDataCopy(datarray, filename, indexvalue);

                            datarray = new string[5000000];

                            catloop = 1;
                            catname = 2;

                        }

                    }
                    else
                    {
                        catloop++;
                    }

                }

                if (good == 1 && ddd == datarray[0])
                {
                    Array.Resize(ref datarray, catloop);
                    ReadDataCopy(datarray, filename, indexvalue);
                }
            }



            return Json(new { dtable = "", fname = filename }, "text/plain");
        }

        public string ReadDataCopy(string[] fulldata, string filename, string indexvalue)
        {
            var indexcolumn = indexvalue.Split(',');

            int catno = NumberFromExcelColumn(indexcolumn[0]);
            int startno = NumberFromExcelColumn(indexcolumn[1]);
            int endno = NumberFromExcelColumn(indexcolumn[2]);
            int SheetNumber = Convert.ToInt32(indexcolumn[3]);
            string finalnos = catno + "," + startno + "," + endno;

            var indexno = finalnos.Split(',');

            if (indexno.Length == 3)
            {
                string filePath = Server.MapPath("~/UploadFiles/") + filename;
                var fileinfo = new FileInfo(filePath);
                var category = fulldata[0];
                using (ExcelPackage p = new ExcelPackage(fileinfo))
                {
                    //  ExcelWorksheet workSheet = p.Workbook.Worksheets.First();
                    ExcelWorksheet workSheet = p.Workbook.Worksheets[SheetNumber];
                    DataTable dt = new DataTable();
                    int t = workSheet.Dimension.End.Column;
                    int insrow = 1;
                    int r = 2;

                    int rowval = 1;
                    int cellval = 1;

                    cellval = workSheet.Dimension.End.Column;
                    rowval = workSheet.Dimension.End.Row;

                    //for (rowval = workSheet.Dimension.Start.Row; rowval <= workSheet.Dimension.End.Row; rowval++) //(IXLRow row in workSheet.Rows())
                    for (int kk = 0; kk < rowval; kk++)
                    {

                        insrow = 1;
                        // for (cellval = workSheet.Dimension.Start.Column; cellval <= workSheet.Dimension.End.Column; cellval++) //(IXLCell cell in row.Cells())
                        for (int j = 0; j < cellval; j++)
                        {

                            int k1 = 1;
                            for (int jj = 0; jj < cellval + 5; jj++)
                            {
                                if (k1 == (int.Parse(indexno[2]) + 2))
                                {
                                    workSheet.Cells[1, k1].Value = "Edited";

                                    workSheet.Cells[1, k1].Style.Border.Top.Style =
                                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    workSheet.Cells[1, k1].Style.Border.Bottom.Style =
                                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    workSheet.Cells[1, k1].Style.Border.Left.Style =
                                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                    workSheet.Cells[1, k1].Style.Border.Right.Style =
                                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                    workSheet.Cells[1, k1].Style.Border.Top.Color
                                        .SetColor(System.Drawing.Color.Gray);
                                    workSheet.Cells[1, k1].Style.Border.Bottom.Color
                                        .SetColor(System.Drawing.Color.Gray);
                                    workSheet.Cells[1, k1].Style.Border.Left.Color
                                        .SetColor(System.Drawing.Color.Gray);
                                    workSheet.Cells[1, k1].Style.Border.Right.Color
                                        .SetColor(System.Drawing.Color.Gray);

                                }

                                k1++;
                            }


                            if (insrow == (int.Parse(indexno[0]) + 1) || (insrow >= (int.Parse(indexno[1]) + 1) && insrow <= (int.Parse(indexno[2]) + 1)))// || insrow >= 12)
                            {
                                string value = workSheet.Cells[r, insrow].Text; //string value = workSheet.Cells(r, insrow).Value.ToString();

                                if (fulldata.Length == 1)
                                {
                                    if (value.Trim() == fulldata[0].Trim() && value.Trim() != "")
                                    {
                                        int i = kk + 1;
                                        //  int insertrow = finaldata.Length - 1;
                                        // int insertrow = fulldata.Length - 1;
                                        // int rowadd = i + 1;
                                        int rowadd = i + 2;

                                        // workSheet.InsertRow(rowadd, insertrow);
                                        //p.Save();

                                        int y = i + 1;
                                        // for (int l = 1; l <= fulldata.Length - 1; l++)
                                        //{
                                        // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);

                                        // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);
                                        // var data = fdata.Split(',');
                                        int k = 1;
                                        int m = 3;

                                        //for (cellvalx = workSheet.Dimension.Start.Column; cellvalx <= workSheet.Dimension.End.Column; cellvalx++) //foreach (IXLCell cell1 in row.Cells())
                                        for (int jj = 0; jj < cellval; jj++)
                                        {

                                            if (k == (int.Parse(indexno[0]) + 1) || (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1)))
                                            {
                                                if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                {

                                                }
                                                else
                                                {
                                                    //workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                    //workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                                }

                                                if (k == (int.Parse(indexno[0]) + 1))
                                                {

                                                    workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                    workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                                }

                                                m++;

                                            }



                                            k++;
                                        }
                                        y++;
                                        // }
                                    }
                                }

                                if (fulldata.Length >= 2)
                                {
                                    if (value.Trim() == fulldata[0].Trim())
                                    {
                                        int i = kk + 1;
                                        //  int insertrow = finaldata.Length - 1;
                                        int insertrow = fulldata.Length - 1;
                                        // int rowadd = i + 1;
                                        int rowadd = i + 2;

                                        workSheet.InsertRow(rowadd, insertrow);
                                        p.Save();

                                        int y = i + 2;


                                        for (int l = 1; l <= fulldata.Length - 1; l++)
                                        {
                                            var fdata = fulldata[l];
                                            var dataedit = fdata.Split(']');
                                            // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);
                                            var data = dataedit[0].Split(new string[] { "~}" }, StringSplitOptions.None);
                                            // var data = dataedit[0].Split(',');
                                            int k = 1;
                                            int m = 4;


                                            for (int jj = 0; jj < cellval + 5; jj++)
                                            {
                                                if (k == (int.Parse(indexno[2]) + 2))
                                                {
                                                    if (dataedit[1] != "")
                                                    {
                                                        workSheet.Cells[y, k].Value = dataedit[1];
                                                        workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                        workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);

                                                        workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                        workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                        workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                        workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                        workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                        workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                        workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                        workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                    }

                                                }

                                                k++;
                                            }
                                            y++;
                                        }

                                        y = i + 2;
                                        for (int l = 1; l <= fulldata.Length - 1; l++)
                                        {
                                            var fdata = fulldata[l];
                                            var dataedit = fdata.Split(']');
                                            // var fdata = fulldata[l].Remove(fulldata[l].Length - 1);
                                            var data = dataedit[0].Split(new string[] { "~}" }, StringSplitOptions.None);
                                            // var data = dataedit[0].Split(',');
                                            int k = 1;
                                            int m = 4;

                                            //for (cellvalx = workSheet.Dimension.Start.Column; cellvalx <= workSheet.Dimension.End.Column; cellvalx++) //foreach (IXLCell cell1 in row.Cells())

                                            for (int jj = 0; jj < cellval; jj++)
                                            {
                                                //if (k == 5)
                                                //{
                                                //    if (data[m].Trim() == "")
                                                //    {
                                                //        workSheet.Cells[y, k].Value = "";
                                                //    }
                                                //    else
                                                //    {
                                                //        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                //        {

                                                //        }
                                                //        else
                                                //        {
                                                //            workSheet.Cells[y, k].Value = data[3];


                                                //            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                //            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                                                //            // range.Style.Border.Top.Color.SetColor(Color.Red);
                                                //        }

                                                //    }


                                                //}

                                                if (k == 1)
                                                {
                                                    if (data[m].Trim() == "")
                                                    {
                                                        workSheet.Cells[y, k].Value = "";
                                                    }
                                                    else
                                                    {
                                                        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            workSheet.Cells[y, k].Value = data[1];
                                                            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                        }

                                                    }

                                                }

                                                //if (k == 7)
                                                //{
                                                //    if (data[m].Trim() == "")
                                                //    {
                                                //        workSheet.Cells[y, k].Value = "";
                                                //    }
                                                //    else
                                                //    {
                                                //        if (k >= (int.Parse(indexno[1]) + 1) && k < (int.Parse(indexno[2]) + 1))
                                                //        {

                                                //        }
                                                //        else
                                                //        {
                                                //            workSheet.Cells[y, k].Value = data[2];
                                                //            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                //            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                //            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                //        }

                                                //    }

                                                //}



                                                if (k == (int.Parse(indexno[0]) + 1) || (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1)))
                                                {

                                                    if (data[m].Trim() == "0.000 ft")
                                                    { data[m] = "Blank"; }
                                                    if (data[m].Trim() == "")
                                                    {
                                                        workSheet.Cells[y, k].Value = "family name missing";
                                                    }
                                                    else
                                                    {
                                                        if (k >= (int.Parse(indexno[1]) + 1) && k <= (int.Parse(indexno[2]) + 1))
                                                        {
                                                            if (data[m].Trim() == "NA")
                                                            {
                                                                // workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);

                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Red;
                                                            }
                                                            else if (data[m].Trim() == "Blank")
                                                            {
                                                                // workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Yellow;
                                                            }
                                                            else
                                                            {
                                                                workSheet.Cells[y, k].Value = data[m];
                                                                workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                                workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);

                                                                workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                                workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                                workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                                workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                                //workSheet.Cell(y, k).Style.Fill.BackgroundColor = XLColor.Green;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            workSheet.Cells[y, k].Value = data[0];

                                                            workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                            workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                            workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                            workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);


                                                        }
                                                    }
                                                    m++;

                                                }

                                                //if (k == (int.Parse(indexno[2]+2)))
                                                //{
                                                //    if (dataedit[1] != "")
                                                //    {
                                                //        workSheet.Cells[y, k].Value = dataedit[1];
                                                //        workSheet.Cells[y, k].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                //        workSheet.Cells[y, k].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Blue);

                                                //        workSheet.Cells[y, k].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //        workSheet.Cells[y, k].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //        workSheet.Cells[y, k].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                                //        workSheet.Cells[y, k].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                                                //        workSheet.Cells[y, k].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                                                //        workSheet.Cells[y, k].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                                                //        workSheet.Cells[y, k].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                                                //        workSheet.Cells[y, k].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                                                //    }

                                                //}

                                                k++;
                                            }
                                            y++;
                                        }
                                    }
                                }
                            }
                            insrow++;
                        }
                        r++;
                    }
                    p.Save();
                    // workBook.SaveAs(filePath);
                }

            }
            return string.Empty;
        }
        #endregion

        public string UpdateCopyExcelData(string FileNAme, string URN, string ModelName, string RuleName, string versionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }



            List<SqlParameter> parameters2 = new List<SqlParameter>();

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
                SqlDbType = SqlDbType.VarChar,
                Value = versionNo,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@FileName",
                SqlDbType = SqlDbType.VarChar,
                Value = FileNAme,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@ModelName",
                SqlDbType = SqlDbType.VarChar,
                Value = ModelName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@Rulename",
                SqlDbType = SqlDbType.VarChar,
                Value = RuleName,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRulehistory = SqlManager.ExecuteDataSet("SP_UpdateExcelHistoryDataCopy", parameters2.ToArray());
            string data = JsonConvert.SerializeObject(dtRulehistory);
            return data;

        }

        public string GetProfileData()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            string data = "";
            List<SqlParameter> parameters2 = new List<SqlParameter>();
            try
            {
                parameters2.Add(new SqlParameter()
                {
                    ParameterName = "@Compid",
                    SqlDbType = SqlDbType.VarChar,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet dtRulehistory = SqlManager.ExecuteDataSet("SP_GetColorsForReport", parameters2.ToArray());

                data = JsonConvert.SerializeObject(dtRulehistory);

            }
            catch (Exception ex)
            {
                data = ex.Message;
            }

            return data;
        }
    }

}

