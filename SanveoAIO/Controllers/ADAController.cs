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
    public class ADAController : Controller
    {
        // GET: ADA
        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();

        public ActionResult ADA()
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

        public string GetClearanceDetails(string urn, string category)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@urn",
                SqlDbType = SqlDbType.VarChar,
                Value = urn,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@category",
                SqlDbType = SqlDbType.VarChar,
                Value = category,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetIDByCategory", parameters.ToArray());
            DataTable dt = ds.Tables[0];
            string data = JsonConvert.SerializeObject(dt);
            return data;
        }

        public JsonResult SendClearenceEmail(string TableValueClearence)

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


            string Body = TableValueClearence.ToString();
            string Subject = "Clearance Report";

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