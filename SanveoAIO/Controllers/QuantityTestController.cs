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
    public class QuantityTestController : Controller
    {
        // GET: Quantity
        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();
        public ActionResult QuantityTest()
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

        //<------ Start of Quantity Popup Methods-------->

        public JsonResult GetQuantityGridModelData([DataSourceRequest] DataSourceRequest request, string urn, string filename, string categoryname, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                if (string.IsNullOrWhiteSpace(filename) == false && string.IsNullOrWhiteSpace(version) == false)
                {
                    var data = db.Database.SqlQuery<GetQuantityGridModelDataNew_Result>("GetQuantityGridModelDataNew @urn={0},@filename={1},@version={2},@CompId={3},@categoryName={4}", urn, filename, version, user.Comp_ID, categoryname).ToList();
                    DataSourceResult result = data.ToDataSourceResult(request);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetQuantity2GridModelData([DataSourceRequest] DataSourceRequest request, string filename, string categoryname, string version)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (filename != "" && categoryname != "" && version != "")
            {
                var data = db.Database.SqlQuery<GetQuantity2GridModelData_Result>("GetQuantity2GridModelData @filename={0},@categoryName={1},@version={2},@CompId={3}", filename, categoryname, version, user.Comp_ID).ToList();
                DataSourceResult result = data.ToDataSourceResult(request);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCustomQuantityData([DataSourceRequest] DataSourceRequest request, string URN, string categoryname, string PropertyName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (URN != "" && categoryname != "" && PropertyName != "")
            {
                var data = db.Database.SqlQuery<SP_GetCustomQuantities_Result>("SP_GetCustomQuantities @urn={0},@Category={1},@PropertyName={2},@CompId={3}", URN, categoryname, PropertyName, user.Comp_ID).ToList();
                DataSourceResult result = data.ToDataSourceResult(request);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAdminFileCategory([DataSourceRequest] DataSourceRequest request, string urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var data = db.Database.SqlQuery<SP_GetCategoryPropertyMap_Result>("SP_GetCategoryPropertyMap @urn={0},@CompId={1}", urn, user.Comp_ID).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCatPropertyMapName(string urn, string category)
        {
            if (category != "")
            {

                IEnumerable<GetCatPropertyMapName_Result> items = db.Database
                    .SqlQuery<GetCatPropertyMapName_Result>("GetCatPropertyMapName @Urn={0},@category={1}", urn, category)
                    .ToList().Select(c => new GetCatPropertyMapName_Result
                    {

                        Id = c.Id,
                        PropertyName = c.PropertyName
                    });
                return Json(items, JsonRequestBehavior.AllowGet);

            }
            else
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMeasurementName()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            IEnumerable<SP_GetMeasurmentName_Result> items = db.Database
                .SqlQuery<SP_GetMeasurmentName_Result>("SP_GetMeasurmentName @CompId={0}", user.Comp_ID)
                .ToList().Select(c => new SP_GetMeasurmentName_Result
                {

                    Id = c.Id,
                    Name = c.Name
                });
            return Json(items, JsonRequestBehavior.AllowGet);

        }

        public JsonResult SavePropertyMap(string Id, string CategoryName, string PropertyName, string Urn, string MeasurementName, string GroupbyName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            int id = Int32.Parse(Id);

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Id",
                SqlDbType = SqlDbType.Int,
                Value = id,
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
                Value = PropertyName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@GroupName",
                SqlDbType = SqlDbType.VarChar,
                Value = GroupbyName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@MeasurementName",
                SqlDbType = SqlDbType.VarChar,
                Value = MeasurementName,
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

            SqlParameter message = new SqlParameter()
            {
                ParameterName = "@OUTMESSAGE",
                SqlDbType = SqlDbType.VarChar,
                Size = 1000,
                Direction = System.Data.ParameterDirection.Output
            };
            parameters1.Add(message);

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_SaveCategoryPropertyMapDetails", parameters1.ToArray());
            string errormessage = message.Value.ToString();

            // return Json(errormessage, "text/plain");

            return this.Json(new DataSourceResult
            {
                Errors = errormessage
            });

        }


        public string GetCategoryCount(string filename, string version, string urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@filename",
                SqlDbType = SqlDbType.VarChar,
                Value = filename,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = urn,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@version",
                SqlDbType = SqlDbType.VarChar,
                Value = version,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetCategoryCountNew", parameters.ToArray());

            string data = JsonConvert.SerializeObject(ds);
            data = data.Replace("{\"Table\":", "");
            data = data.Remove(data.Length - 1, 1);
            return data;

        }

       

        public string GetFamilyCount(string filename, string version, string categoryname, string urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@filename",
                SqlDbType = SqlDbType.VarChar,
                Value = filename,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@version",
                SqlDbType = SqlDbType.VarChar,
                Value = version,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@urn",
                SqlDbType = SqlDbType.VarChar,
                Value = urn,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@categoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = categoryname,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@MGUID",
                SqlDbType = SqlDbType.NVarChar,
                Value = urn,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet ds = SqlManager.ExecuteDataSet("SP_GetFamilyCountNew", parameters.ToArray());


           

            string data = JsonConvert.SerializeObject(ds);
            data = data.Replace("{\"Table\":", "");
            data = data.Remove(data.Length - 1, 1);
            return data;

        }

        public ActionResult GetCategoryHighLight(string Id, string CategoryName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string forgeid = "";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@urn",
                SqlDbType = SqlDbType.VarChar,
                Value = Id,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@categoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = CategoryName,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("GetCategoryHighLight", parameters.ToArray());
            DataTable dtruledat = dtRuleData.Tables[0];
            if (dtruledat.Rows.Count > 0)
            {
                for (int j = 0; j < dtruledat.Rows.Count; j++)
                {
                    forgeid += dtruledat.Rows[j]["Forgeid"].ToString() + "|";

                }
            }

            return Json(forgeid, JsonRequestBehavior.AllowGet);
        }

        public string GetVersionwiseDiff(string FileName, string CategoryName, string BaseVersion)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@FileName",
                SqlDbType = SqlDbType.VarChar,
                Value = FileName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CategoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = CategoryName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@baseversion",
                SqlDbType = SqlDbType.VarChar,
                Value = BaseVersion,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetVersionwiseDiffNew", parameters.ToArray());
            DataTable dt = ds.Tables[0];
            string data = JsonConvert.SerializeObject(dt);
            return data;
        }

        public JsonResult GetCategory(string urn, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                IEnumerable<SP_GetCategory_Result> items1 = db.Database.SqlQuery<SP_GetCategory_Result>("EXEC SP_GetCategory @urn={0},@version={1},@CompId={2}", urn, version, user.Comp_ID).ToList().Select(c => new SP_GetCategory_Result
                {
                    Id = c.Id,
                    Category_Name = c.Category_Name
                });
                return Json(items1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetRooms(string urn, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                IEnumerable<SP_GetModelRooms_Result> items1 = db.Database.SqlQuery<SP_GetModelRooms_Result>("EXEC SP_GetModelRooms @urn={0},@version={1},@CompId={2}", urn, version, user.Comp_ID).ToList().Select(c => new SP_GetModelRooms_Result
                {
                    Forgeid = c.Forgeid,
                    Rooms = c.Rooms
                });
                return Json(items1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }


        public string GetPropertywiseDiff(string FileName, string CategoryName, string BaseVersion, string propvalue)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@FileName",
                SqlDbType = SqlDbType.VarChar,
                Value = FileName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CategoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = CategoryName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@baseversion",
                SqlDbType = SqlDbType.VarChar,
                Value = BaseVersion,
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
                ParameterName = "@Propvalue",
                SqlDbType = SqlDbType.VarChar,
                Value = propvalue,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet ds = SqlManager.ExecuteDataSet("SP_GetPropertywiseDiffNew", parameters.ToArray());
            DataTable dt = ds.Tables[0];
            string data = JsonConvert.SerializeObject(dt);
            return data;
        }

        public string GetDiffVersion(string FileName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@file",
                SqlDbType = SqlDbType.VarChar,
                Value = FileName,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_Diffversion", parameters.ToArray());
            DataTable dt = ds.Tables[0];
            string data = JsonConvert.SerializeObject(dt);
            return data;
        }

        public JsonResult GetPropertyValue(string urn, string category, string propname, string GetProp)
        {
            if (urn != "")
            {
                IEnumerable<GetPropertyValue_Result> items = db.Database.SqlQuery<GetPropertyValue_Result>("GetPropertyValue @Guid={0},@category={1},@propname={2},@getprop={3}", urn, category, propname, GetProp).ToList().Select(c => new GetPropertyValue_Result
                {
                    Id = c.Id,
                    Property_Value = c.Property_Value
                });
                return Json(items, JsonRequestBehavior.AllowGet);

            }
            else
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeletePropertyMap(string Id, string CategoryName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            int id = Int32.Parse(Id);

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Id",
                SqlDbType = SqlDbType.Int,
                Value = id,
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
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_DeleteCategoryPropertyMapDetails", parameters1.ToArray());
            string errormessage = "Mapping deleted successfully";
            // return Json("", "text/plain");
            return this.Json(new DataSourceResult
            {
                Errors = errormessage
            });
        }

        //<<------End of Quantity Popup Methods -------->>

        public JsonResult SendQuantityEmail(string TableValueQuantity)

        {
            String Result = "Mail Send Successfully.";
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
                ParameterName = "@U_Id",
                SqlDbType = SqlDbType.VarChar,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet EmailTable = SqlManager.ExecuteDataSet("SP_SendClearenceEmail", parameters.ToArray());
            DataTable dataTableSetMailConfiguration = EmailTable.Tables[0];
            DataTable dataTableToSendEmails = EmailTable.Tables[1];

            string tempFromAddress = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailFromAddress")["Value"].ToString();
            string tempFromPassword = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailFromPassword")["Value"].ToString();
            string tempMailHostName = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailHostName")["Value"].ToString();
            int tempMailPortNumber = int.Parse(dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailPortNumber")["Value"].ToString());
            //string tempMailToAddress = dataTableSetMailConfiguration.AsEnumerable().First(r => r.Field<string>("KeyName") == "MailToAddress")["Value"].ToString();


            string Body = TableValueQuantity.ToString();
            string Subject = "Quantity Report";

            string tempMailToAddress = string.Empty;
            if (dataTableToSendEmails.Rows.Count > 0)
            {
                for (int i = 0; i < dataTableToSendEmails.Rows.Count; i++)
                    tempMailToAddress += EmailTable.Tables[1].Rows[i]["EmailId"].ToString() + ",";

            }
            tempMailToAddress = tempMailToAddress.Remove(tempMailToAddress.Length - 1);


            var smtp = new SmtpClient();
            {
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(tempFromAddress, tempFromPassword);
                smtp.Host = tempMailHostName;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                if (tempMailHostName == "smtp.office365.com")
                {
                    smtp.TargetName = "STARTTLS/smtp.office365.com";
                }
                smtp.Port = tempMailPortNumber;
            }
            MailMessage msg = new MailMessage();
            MailAddress mailFromAddress = new MailAddress(tempFromAddress);
            msg.From = mailFromAddress;
            msg.To.Add(tempMailToAddress);
            msg.Subject = Subject;
            msg.IsBodyHtml = true;

            msg.Body = Body;
            smtp.Send(msg);

            return Json(Result, JsonRequestBehavior.AllowGet);

        }


        public string GetModelForgeid(string Urn, string VersionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
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

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetModelForgeid", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        public JsonResult SaveBoundingPoints(string Urn, string VersionNo,string DataStore)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

       
            DataStore = DataStore.Substring(0, DataStore.Length - 1);

            var ff = DataStore.Split('#');

            int sdfdf = ff.Length;

            DataTable dt = new DataTable();

            dt.Columns.Add("Forgeid");
            dt.Columns.Add("CategoryName");
            dt.Columns.Add("Family_Type");
            dt.Columns.Add("Instance_Name");
            dt.Columns.Add("Urn");
            dt.Columns.Add("Version");
            dt.Columns.Add("CompId");
            dt.Columns.Add("TopLeftX");
            dt.Columns.Add("TopLeftY");
            dt.Columns.Add("BottomRightX");
            dt.Columns.Add("BottomRightY");
            dt.Columns.Add("MinZ");
            dt.Columns.Add("MaxZ");


            for (int i = 0; i < ff.Length; i++)
            {

                string value1 = ff[i].ToString();
                //var value2 = value1.Split("|>|");

                var value2 = value1.Split(new string[] { "~}" }, StringSplitOptions.None);

                dt.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7],
                    value2[8], value2[9], value2[10], value2[11], value2[12]);

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
                ParameterName = "@tblBoundingData",
                SqlDbType = SqlDbType.Structured,
                Value = dt,
                Direction = System.Data.ParameterDirection.Input
            });

    
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveBoundingPoints", parameters1.ToArray());


            return Json("", "text/plain");
        }

        public JsonResult SaveBoundingMiddlePoints(string Urn, string VersionNo, string DataStoreMiddle)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            // For Middle Co-ordiante

            DataStoreMiddle = DataStoreMiddle.Substring(0, DataStoreMiddle.Length - 1);

            var ffMiddle = DataStoreMiddle.Split('#');


            DataTable dtffMiddle = new DataTable();

            dtffMiddle.Columns.Add("Forgeid");
            dtffMiddle.Columns.Add("CategoryName");
            dtffMiddle.Columns.Add("Family_Type");
            dtffMiddle.Columns.Add("Instance_Name");
            dtffMiddle.Columns.Add("Urn");
            dtffMiddle.Columns.Add("Version");
            dtffMiddle.Columns.Add("CompId");
            dtffMiddle.Columns.Add("MiddleX");
            dtffMiddle.Columns.Add("MiddleY");
            dtffMiddle.Columns.Add("MiddleZ");
          


            for (int i = 0; i < ffMiddle.Length; i++)
            {

                string value1 = ffMiddle[i].ToString();
                //var value2 = value1.Split("|>|");

                var value2 = value1.Split(new string[] { "~}" }, StringSplitOptions.None);

                dtffMiddle.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7], value2[8], value2[9]);

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
                ParameterName = "@tblMiddleData",
                SqlDbType = SqlDbType.Structured,
                Value = dtffMiddle,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveBoundingMiddlePoints", parameters1.ToArray());


            return Json("", "text/plain");
        }

        public string GetRoomElement(string Urn, string VersionNo,string Roomid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
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
                ParameterName = "@Roomid",
                SqlDbType = SqlDbType.VarChar,
                Value = Roomid,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetRoomElements", parameters1.ToArray());
            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count>0)
             datatable = dataSet2.Tables[0]; 
        

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }


        public string GetRoomElementGroup(string Urn, string VersionNo, string Roomid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
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
                ParameterName = "@Roomid",
                SqlDbType = SqlDbType.VarChar,
                Value = Roomid,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetRoomElementsGroup", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];

            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }

        public JsonResult SaveRoomPoints(string Urn, string VersionNo, string DataRoomBoundingBox,string DataRoomBoundingBoxmin)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            // For Middle Co-ordiante

            DataRoomBoundingBox = DataRoomBoundingBox.Substring(0, DataRoomBoundingBox.Length - 1);

            var ffMiddle = DataRoomBoundingBox.Split('$');

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


            //For MIN

            DataRoomBoundingBoxmin = DataRoomBoundingBoxmin.Substring(0, DataRoomBoundingBoxmin.Length - 1);

            var ffMiddlemin = DataRoomBoundingBoxmin.Split('$');

            DataTable dtffMiddlemin = new DataTable();

            dtffMiddlemin.Columns.Add("Forgeid");
            dtffMiddlemin.Columns.Add("CategoryName");
            dtffMiddlemin.Columns.Add("Family_Type");
            dtffMiddlemin.Columns.Add("Instance_Name");
            dtffMiddlemin.Columns.Add("Urn");
            dtffMiddlemin.Columns.Add("Version");
            dtffMiddlemin.Columns.Add("CompId");
            dtffMiddlemin.Columns.Add("Points");

            for (int i = 0; i < ffMiddlemin.Length; i++)
            {

                string value1 = ffMiddlemin[i].ToString();
                //var value2 = value1.Split("|>|");

                var value2 = value1.Split(new string[] { "~}" }, StringSplitOptions.None);

                dtffMiddlemin.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7]);

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
                ParameterName = "@tblRoomBoundingData",
                SqlDbType = SqlDbType.Structured,
                Value = dtffMiddle,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@tblRoomBoundingDataMin",
                SqlDbType = SqlDbType.Structured,
                Value = dtffMiddlemin,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveRoomBoundingPoints", parameters1.ToArray());


            return Json("", "text/plain");
        }


        public string GetSCMQTYValue(string urn,string Category,string Level,string Standard)
        {
            if (urn != "")
            {

                List<SqlParameter> parameters1 = new List<SqlParameter>();
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Category",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Category,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Level",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Level,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Standard",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Standard,
                    Direction = System.Data.ParameterDirection.Input
                });



                DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetSCMQuantityValue", parameters1.ToArray());
                string data = JsonConvert.SerializeObject(dtRuleData);
                return data;


            }
            else
            {
                return "";
            }
           
        }

        public JsonResult GetLevelSize(string urn,string Category_Name)
        {
            if (urn != "")
            {
                string JHOOK1 = "";
                string JunctionBox1 = "";
              

                List<SqlParameter> parameters1 = new List<SqlParameter>();
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Mguid",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Category_Name",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = Category_Name,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetLevelSize", parameters1.ToArray());
                DataTable JHOOK = dtRuleData.Tables[0];
                // DataTable JunctionBox = dtRuleData.Tables[1];


                for (int j = 0; j < JHOOK.Rows.Count; j++)
                {
                    JHOOK1 += JHOOK.Rows[j]["Reference_Level"].ToString() + ",";

                }

                //if (JHOOK.Rows.Count > 0)
                //{
                //    JHOOK1 = JHOOK.Rows[0]["Reference_Level"].ToString();


                //}
                //if (JunctionBox.Rows.Count > 0)
                //{
                //    JunctionBox1 = JunctionBox.Rows[0]["Count2"].ToString();
                //}
               


                return Json(JHOOK1  , JsonRequestBehavior.AllowGet);


            }
            else
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSupplyChainData([DataSourceRequest] DataSourceRequest request, string Urn, string Category)
        {
            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var result = db.Database.SqlQuery<SP_GetSupplyChainMasterDetails_Result>("SP_GetSupplyChainMasterDetails  @Urn={0},@Category_Name={1}", Urn, Category).ToList();
            result1 = result.ToDataSourceResult(request);

            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPropertyName(string urn, string category)
        {
            try
            {
                if (category != "")
                {

                    IEnumerable<GetPropertyName_Result> items = db.Database
                        .SqlQuery<GetPropertyName_Result>("GetPropertyName @Guid={0},@category={1}", urn, category)
                        .ToList().Select(c => new GetPropertyName_Result
                        {

                            Id = c.Id,
                            Property_Name = c.Property_Name
                        });
                    return Json(items, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult SaveSupplyChainData(string Urn, string Cat_Name, string Prop_Name,string Prop_Value,string IsGroup,string Report_Id)
        {
            bool IsGroupBool;
            if (IsGroup == "Yes")
            {
                IsGroupBool = true;
            }
            else
                IsGroupBool = false;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

          //  int id = Int32.Parse(Id);

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.NVarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Cat_name",
                SqlDbType = SqlDbType.VarChar,
                Value = Cat_Name,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Prop_Name",
                SqlDbType = SqlDbType.VarChar,
                Value = Prop_Name,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Prop_Value",
                SqlDbType = SqlDbType.VarChar,
                Value = Prop_Value,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@IsGroup",
                SqlDbType = SqlDbType.Bit,
                Value = IsGroupBool,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Report_Id",
                SqlDbType = SqlDbType.VarChar,
                Value = Report_Id,
                Direction = System.Data.ParameterDirection.Input
            });



            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_InsertSupplyChainMaster", parameters1.ToArray());

            string errormessage = dtRuleData.Tables[1].Rows[0]["Action"].ToString();

            // return Json(errormessage, "text/plain");

            return this.Json(new DataSourceResult
            {
                Errors = errormessage
            });
           

        }
        
       public JsonResult AddSetingsData(string Urn, string Category, string Level, string Standard, string GroupQuantity, string ID)
        {
            int id = Convert.ToInt32(ID);
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            //  int id = Int32.Parse(Id);

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.NVarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Category",
                SqlDbType = SqlDbType.VarChar,
                Value = Category,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Level",
                SqlDbType = SqlDbType.VarChar,
                Value = Level,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Standard",
                SqlDbType = SqlDbType.VarChar,
                Value = Standard,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Group",
                SqlDbType = SqlDbType.VarChar,
                Value = GroupQuantity,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ID",
                SqlDbType = SqlDbType.Int,
                Value = id,
                Direction = System.Data.ParameterDirection.Input
            });
           




            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_INSERT_UpdateQuantitySetting", parameters1.ToArray());

            string errormessage = dtRuleData.Tables[0].Rows[0]["Action"].ToString();

            // return Json(errormessage, "text/plain");

            return this.Json(new DataSourceResult
            {
                Errors = errormessage
            });


        }

        public JsonResult AddConsolidatedData(string Urn, string DESCRIPTION, string UOM, string QTY_Needed, string GB_ID, string a123)
        {

            if((Urn=="" || Urn==null || a123 == "" || a123 == null) )
            {
                 return this.Json(new DataSourceResult
                {
                    Errors = ""
                });
            }
           
            else
            {
                //int id = Convert.ToInt32(ID);
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                //  int id = Int32.Parse(Id);

                List<SqlParameter> parameters1 = new List<SqlParameter>();
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = Urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@DESCRIPTION",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = DESCRIPTION,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@UOM",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = UOM,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@QTY_Needed",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = QTY_Needed,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@GBID",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = GB_ID,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@GB_Description",
                    SqlDbType = SqlDbType.NVarChar,
                    Value = a123,
                    Direction = System.Data.ParameterDirection.Input
                });
                //parameters1.Add(new SqlParameter()
                //{
                //    ParameterName = "@ID",
                //    SqlDbType = SqlDbType.Int,
                //    Value = id,
                //    Direction = System.Data.ParameterDirection.Input
                //});





                DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_ADDUPDATE_MaterialGBDescription", parameters1.ToArray());

                string errormessage = dtRuleData.Tables[0].Rows[0]["Action"].ToString();

                // return Json(errormessage, "text/plain");

                return this.Json(new DataSourceResult
                {
                    Errors = errormessage
                });
            }

        }

        public JsonResult AddSetDescriptionData(string Urn, string Cat_Name, string Description,string ID)
        {
            int id = Convert.ToInt32(ID);
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            //  int id = Int32.Parse(Id);

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.NVarChar,
                Value = Urn,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Cat_Name",
                SqlDbType = SqlDbType.VarChar,
                Value = Cat_Name,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@Description",
                SqlDbType = SqlDbType.VarChar,
                Value = Description,
                Direction = System.Data.ParameterDirection.Input
            });
          
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@ID",
                SqlDbType = SqlDbType.Int,
                Value = id,
                Direction = System.Data.ParameterDirection.Input
            });





            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_ADDUPDATE_SettingDataDesc", parameters1.ToArray());

            //string errormessage = dtRuleData.Tables[0].Rows[0]["Action"].ToString();
            string errormessage = "";
            // return Json(errormessage, "text/plain");

            return this.Json(new DataSourceResult
            {
                Errors = errormessage
            });


        }

        public JsonResult GetSetingsData([DataSourceRequest] DataSourceRequest request,string Urn)
        {

            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var result = db.Database.SqlQuery<SP_GetQuantitySetting_Result>("EXEC SP_GetQuantitySetting @Urn={0}", Urn).ToList();
            result1 = result.ToDataSourceResult(request);

            return Json(result1, JsonRequestBehavior.AllowGet);

          

        }

        public JsonResult GetDescriptionData([DataSourceRequest] DataSourceRequest request, string Urn)
        {

            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var result = db.Database.SqlQuery<SP_GET_SettingDataDesc_Result>("EXEC SP_GET_SettingDataDesc @Urn={0}", Urn).ToList();
            result1 = result.ToDataSourceResult(request);

            return Json(result1, JsonRequestBehavior.AllowGet);



        }



        public JsonResult GetConsoliatedData([DataSourceRequest] DataSourceRequest request, string Urn)
        {

            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var result = db.Database.SqlQuery<SP_GetConsolidatedSCMData_Result>("EXEC SP_GetConsolidatedSCMData @Urn={0}", Urn).ToList();
            result1 = result.ToDataSourceResult(request);

            return Json(result1, JsonRequestBehavior.AllowGet);



        }

        public JsonResult GetConsoliatedDataNew([DataSourceRequest] DataSourceRequest request, string Urn)
        {

            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var result = db.Database.SqlQuery<SP_GetConsolidatedGBData_Result > ("EXEC SP_GetConsolidatedGBData @Urn={0}", Urn).ToList();
            result1 = result.ToDataSourceResult(request);

            return Json(result1, JsonRequestBehavior.AllowGet);



        }

        public JsonResult GetPropValues(string Urn, string Category, string Property)
        {

            try
            {
                IEnumerable<GetPropertyValueByPropName_Result> items = db.Database
               .SqlQuery<GetPropertyValueByPropName_Result>("EXEC GetPropertyValueByPropName @Urn={0},@category={1},@propname={2}", Urn, Category, Property)
               .ToList().Select(c => new GetPropertyValueByPropName_Result
               {

                   ID = c.ID,
                   Property_Value = c.Property_Value
               });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        public string GetConsolidatedQuantity(string urn)
        {
            try
            {
                if (urn != "")
                {
                    //comment
                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@Urn",
                        SqlDbType = SqlDbType.VarChar,
                        Value = urn,
                        Direction = System.Data.ParameterDirection.Input
                    });


                    DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_GetConsolidatedSCMData", parameters1.ToArray());
                    string data = JsonConvert.SerializeObject(dtRuleData);
                    return data;

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }


        public string GetScopeBoxElement(string Urn, string VersionNo, string Roomid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@VersionNo",
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
                ParameterName = "@Roomid",
                SqlDbType = SqlDbType.VarChar,
                Value = Roomid,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetRoomElements", parameters1.ToArray());
            DataTable datatable = new DataTable();

            if (dataSet2.Tables.Count > 0)
                datatable = dataSet2.Tables[0];


            string data = JsonConvert.SerializeObject(datatable);
            return data;
        }


       

    }
}