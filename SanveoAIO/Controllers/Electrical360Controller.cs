using System;
using System.Collections.Generic;
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
using System.Linq;

namespace SanveoAIO.Controllers
{
    public class Electrical360Controller : Controller
    {
        // GET: Electrical360

        // GET: ADA
        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();

        public ActionResult Electrical360()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
                return View();
            }
            else
                return RedirectToAction("Login", "Login");
        }

        //--Project Profile Start Here-------------

        public ActionResult SaveMapFile(IEnumerable<HttpPostedFileBase> Mapfiles)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (Mapfiles != null)
            {
                foreach (var uploadfile in Mapfiles)
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
                        DataTable datatable = result.Tables[0];

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

                        DataRow dr1 = result.Tables[0].NewRow();
                        for (int i = 0; i < result.Tables[0].Columns.Count; i++)
                        {
                            dr1[result.Tables[0].Columns[i].ColumnName] = result.Tables[0].Rows[0][i];

                        }

                        DataTable ExcelData = new DataTable();
                        ExcelData.Columns.Add("ExcelParameter");
                        ExcelData.Columns.Add("ParameterData");


                        foreach (DataColumn dr in datatable.Columns)
                        {
                            if (dr.ColumnName.Trim() != "")
                            {
                                DataRow newCustomersRow = ExcelData.NewRow();
                                newCustomersRow["ExcelParameter"] = dr.ColumnName.Trim();

                                ExcelData.Rows.Add(newCustomersRow);
                            }
                        }

                        int g = 0;
                        foreach (DataColumn drcol in datatable.Columns)
                        {
                            string ParameterData = "";
                            foreach (DataRow dr in datatable.Rows)
                            {
                                ParameterData += Convert.ToString(dr[drcol.ColumnName.Trim()]) + "~";
                            }

                            ParameterData = ParameterData.Substring(0, ParameterData.Length - 1);

                            ExcelData.Rows[g]["ParameterData"] = ParameterData;

                            g++;
                        }

                        List<SqlParameter> parameters1 = new List<SqlParameter>();

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@CompId",
                            SqlDbType = SqlDbType.Int,
                            Value = user.Comp_ID,
                            Direction = System.Data.ParameterDirection.Input
                        });


                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@tblExceldata",
                            SqlDbType = SqlDbType.Structured,
                            Value = ExcelData,
                            Direction = System.Data.ParameterDirection.Input
                        });


                        DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_EleExcelMappingData", parameters1.ToArray());


                    }
                }
            }

            return Json("", "text/plain");
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

        public JsonResult GetExcelParameter()
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            IEnumerable<SP_GetEleExcelParameter_Result> items = db.Database
                .SqlQuery<SP_GetEleExcelParameter_Result>("SP_GetEleExcelParameter @Compid={0}", user.Comp_ID)
                .ToList().Select(c => new SP_GetEleExcelParameter_Result
                {

                    Id = c.Id,
                    ExcelParameter = c.ExcelParameter
                });
            return Json(items, JsonRequestBehavior.AllowGet);

        }

        public JsonResult SaveRevitExcelMapping(string RevitFeederId, string RevitFrom, string RevitTo, string RevitConductType, string RevitConductSize, string ExcelFeederId, string ExcelFrom, string ExcelTo, string ExcelConductType, string ExcelConductSize, string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RevitFeederID",
                SqlDbType = SqlDbType.VarChar,
                Value = RevitFeederId,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ExcelFeederID",
                SqlDbType = SqlDbType.VarChar,
                Value = ExcelFeederId,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RevitFrom",
                SqlDbType = SqlDbType.VarChar,
                Value = RevitFrom,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ExcelFrom",
                SqlDbType = SqlDbType.VarChar,
                Value = ExcelFrom,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RevitTo",
                SqlDbType = SqlDbType.VarChar,
                Value = RevitTo,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ExcelTo",
                SqlDbType = SqlDbType.VarChar,
                Value = ExcelTo,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RevitConductType",
                SqlDbType = SqlDbType.VarChar,
                Value = RevitConductType,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ExcelConductType",
                SqlDbType = SqlDbType.VarChar,
                Value = ExcelConductType,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@RevitConductSize",
                SqlDbType = SqlDbType.VarChar,
                Value = RevitConductSize,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ExcelConductSize",
                SqlDbType = SqlDbType.VarChar,
                Value = ExcelConductSize,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveRevitExcelMapping", parameters1.ToArray());


            return Json("", "text/plain");
        }

        public string GetRevitExcelMapping(string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetRevitExcelMapping", parameters1.ToArray());

            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count > 0)
                datatable = dataSet2.Tables[0];


            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        //--Project Profile End Here-------------



        // ---Std Bends Start here----------

        public ActionResult GetBends([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetstdBends_Result>("SP_GetstdBends @CompId={0}", user.Comp_ID).ToList();

            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveBends(int BendsId, string BendsName)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var Error = new ObjectParameter("Error", typeof(string));
            var data = db.SP_SaveBends(BendsId, BendsName,user.Comp_ID, Error);
            string data1 = Error.Value.ToString();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBends(string Id)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            int UserId = Int32.Parse(Id);
            var data = db.SP_DeleteStdBends(UserId,user.Comp_ID);

            return Json("Data Deleted Successfully", JsonRequestBehavior.AllowGet);
        }

        // ---Std Bends End here----------


        //---FeederM Start here---------

        public string GetEleExcelFeederIdData(string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetEleExcelFeederIdData", parameters1.ToArray());

            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count > 0)
                datatable = dataSet2.Tables[0];


            string data = JsonConvert.SerializeObject(dataSet2);
            return data;
        }

        public string GetEleExcelConduitSizeData(string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetEleExcelConduitSizeData", parameters1.ToArray());

            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count > 0)
                datatable = dataSet2.Tables[0];


            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        public JsonResult GetFamily(string urn, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                IEnumerable<SP_GetElectricalFamily_Result> items1 = db.Database.SqlQuery<SP_GetElectricalFamily_Result>("EXEC SP_GetElectricalFamily @urn={0},@version={1},@CompId={2}", urn, version, user.Comp_ID).ToList().Select(c => new SP_GetElectricalFamily_Result
                {
                    Id = c.Id,
                    Family_Name = c.Family_Name
                });
                return Json(items1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public string GetFeederGeometryCheck(string Urn,string Families)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Families",
                SqlDbType = SqlDbType.VarChar,
                Value = Families,
                Direction = System.Data.ParameterDirection.Input
            });


            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFeederIdEndsEqpt", parameters1.ToArray());

            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count > 0)
                datatable = dataSet2.Tables[0];


            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        //---FeederM End here


        // ---blankids Start here----------

        public ActionResult GetElectricalData([DataSourceRequest] DataSourceRequest request, string urn, string categoryname, string propertyname, string propertyvalue)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetElectricalData_Result>("SP_GetElectricalData @Compid={0},@Urn={1},@CategoryName={2},@PropertyName={3},@PropertyValue={4}", user.Comp_ID, urn, categoryname, propertyname, propertyvalue).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        //---blankids End here


        ///--XLS Tab Start Here---

        public JsonResult SaveCoduitsPoints(string Urn, string VersionNo, string ConduitsDataStore)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            // For Middle Co-ordiante

            ConduitsDataStore = ConduitsDataStore.Substring(0, ConduitsDataStore.Length - 1);

            var ffMiddle = ConduitsDataStore.Split('$');

            DataTable dtffMiddle = new DataTable();

            dtffMiddle.Columns.Add("Forgeid");
            dtffMiddle.Columns.Add("CategoryName");
            dtffMiddle.Columns.Add("Family_Type");
            dtffMiddle.Columns.Add("Instance_Name");
            dtffMiddle.Columns.Add("Urn");
            dtffMiddle.Columns.Add("Version");
            dtffMiddle.Columns.Add("CompId");
            dtffMiddle.Columns.Add("Points");

            for (int i = 0; i < ffMiddle.Length; i++)
            {

                string value1 = ffMiddle[i].ToString();
                //var value2 = value1.Split("|>|");

                var value2 = value1.Split(new string[] { "~}" }, StringSplitOptions.None);

                dtffMiddle.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7]);

            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@version",
                SqlDbType = SqlDbType.VarChar,
                Value = VersionNo,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@tblRunBoundingData",
                SqlDbType = SqlDbType.Structured,
                Value = dtffMiddle,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveConduitsBoundingPoints", parameters1.ToArray());


            return Json("", "text/plain");
        }

        public string GetRUN(string Urn, string FeederId)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@FeederId",
                SqlDbType = SqlDbType.VarChar,
                Value = FeederId,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFeederIDRun", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        public JsonResult GetFeederId(string urn)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            IEnumerable<SP_GetFeederId_Result> items = db.Database
                .SqlQuery<SP_GetFeederId_Result>("SP_GetFeederId @Compid={0},@Urn={1}", user.Comp_ID,urn)
                .ToList().Select(c => new SP_GetFeederId_Result
                {

                    Id = c.Id,
                    Property_Value = c.Property_Value
                });
            return Json(items, JsonRequestBehavior.AllowGet);

        }

        public string GetFeedersDetails(string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
           
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetfeederDetails", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        public string GetBendCheck()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetstdBends", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }


        ////----XLS tab End Here



        //----Update Tab Start Here---

        public JsonResult GetPropertyName(string urn, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                IEnumerable<SP_GetElectricalPropertiesName_Result> items1 = db.Database.SqlQuery<SP_GetElectricalPropertiesName_Result>("EXEC SP_GetElectricalPropertiesName @urn={0},@version={1},@CompId={2}", urn, version, user.Comp_ID).ToList().Select(c => new SP_GetElectricalPropertiesName_Result
                {
                    Id = c.Id,
                    PropertyName = c.PropertyName
                });
                return Json(items1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public string GetUpdateFeederDetails(string Urn, string Properties)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyName",
                SqlDbType = SqlDbType.VarChar,
                Value = Properties,
                Direction = System.Data.ParameterDirection.Input
            });


            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetUpdateFeederDetails", parameters1.ToArray());

            string data = JsonConvert.SerializeObject(dataSet2);
            return data;
        }

        //---Update Tab End Here----  
    }
}