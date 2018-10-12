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
    public class QuantityController : Controller
    {
        // GET: Quantity
        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();
        public ActionResult Quantity()
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

        public JsonResult GetQuantityGridModelData([DataSourceRequest] DataSourceRequest request, string filename, string version)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }

                if (string.IsNullOrWhiteSpace(filename) == false && string.IsNullOrWhiteSpace(version) == false)
                {
                    var data = db.Database.SqlQuery<GetQuantityGridModelData_Result>("GetQuantityGridModelData @filename={0},@version={1},@CompId={2}", filename, version, user.Comp_ID).ToList();
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

        public string GetCategoryCount(string filename, string version)
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
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetCategoryCount", parameters.ToArray());

            string data = JsonConvert.SerializeObject(ds);
            data = data.Replace("{\"Table\":", "");
            data = data.Remove(data.Length - 1, 1);
            return data;

        }

        public string GetFamilyCount(string filename, string version, string categoryname)
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

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetFamilyCount", parameters.ToArray());

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

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetVersionwiseDiff", parameters.ToArray());
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


            DataSet ds = SqlManager.ExecuteDataSet("SP_GetPropertywiseDiff", parameters.ToArray());
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


       

    }
}