using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SanveoAIO.Domain.Models;
using System.Data.Entity.Core.Objects;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace SanveoAIO.Controllers
{
    public class LayoutDataController : Controller
    {
        UserInfo user = null;
        SanveoInspireEntities db = new SanveoInspireEntities();

        // GET: LayoutData
        public ActionResult Index()
        {
            return View();
        }

        [WebMethod]
        public JsonResult SetCurrentUserProfileInfo()
        {

            UserInfo user = null;
            if (Session["UserInfo"] != null)
                user = (UserInfo)Session["UserInfo"];
            else
                Response.Redirect("~/Login/Login");
            int UserId = user.U_Id;

            string data = "";
            try
            {
                var Error = new ObjectParameter("Error", typeof(string));

                var result1 = db.SP_GetProfileInfo(user.U_Id, user.G_Id);
                string username = "";
                string groupname = "";
                string firstname = "";
                string lastname = "";
                string emailid = "";
                string compname = "";
                foreach (var datarow in result1)
                {
                    username = datarow.Username;
                    groupname = datarow.Group_Name;
                    firstname = datarow.FirstName;
                    lastname = datarow.LastName;
                    emailid = datarow.Emailid;
                    compname = datarow.CompName;
                }
                var result2 = new { username = username, groupname = groupname, firstname = firstname, lastname = lastname, emailid = emailid, compname = compname };

                return Json(result2, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                data = "Error";
                return Json(data, JsonRequestBehavior.AllowGet);
            }


        }

        [WebMethod]
        public JsonResult ChangePassword(string oldpass, string newpass)
        {
            UserInfo user = null;
            user = (UserInfo)Session["UserInfo"];
            int UserId = user.U_Id;

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@old",
                SqlDbType = SqlDbType.VarChar,
                Value = oldpass,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@new",
                SqlDbType = SqlDbType.VarChar,
                Value = newpass,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@U_ID",
                SqlDbType = SqlDbType.Int,
                Value = UserId,
                Direction = System.Data.ParameterDirection.Input
            });

            SqlParameter error = new SqlParameter()
            {
                ParameterName = "@Error",
                SqlDbType = SqlDbType.VarChar,
                Size = 100,
                Direction = System.Data.ParameterDirection.Output
            };
            parameters.Add(error);
            DataSet dataSet = telmvc.SqlManager.ExecuteDataSet("SP_ChangePassword", parameters.ToArray());
            if (error.Value.ToString() == "Password updated Successfully")
            {
                //send mail
                string HostAdd = ConfigurationManager.AppSettings["Host"].ToString();
                string FromEmailId = ConfigurationManager.AppSettings["FromMail"].ToString();
                string Pass = ConfigurationManager.AppSettings["Password"].ToString();

                MailMessage mailmessage = new MailMessage();

                mailmessage.From = new MailAddress(FromEmailId);
                mailmessage.Subject = "Password Changed Notification";
                mailmessage.Body = "Dear User,<br/>Your Password has been changed Successfully at " + System.DateTime.Now + "<br/>Thanks & Regards,<br/>Sanveotech";
                mailmessage.IsBodyHtml = true;
                mailmessage.To.Add(new MailAddress(user.EmailId));

                SmtpClient smtp = new SmtpClient();
                smtp.Host = HostAdd;

                smtp.EnableSsl = true;
                NetworkCredential networkcred = new NetworkCredential();
                networkcred.UserName = mailmessage.From.Address;
                networkcred.Password = Pass;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkcred;
                smtp.Port = 587;
                smtp.Send(mailmessage);

            }
            return Json(error.Value, JsonRequestBehavior.AllowGet);

        }

        [WebMethod]
        public string SaveGlobalThemeBasedOnUserId(string themeval)
        {
            UserInfo user = null;
            if (Session["UserInfo"] != null)
                user = (UserInfo)Session["UserInfo"];
            else
                Response.Redirect("~/Login.aspx");

            string Data = "Saved";
            try
            {
                var result = db.SP_SaveGlobalThemeName(themeval, user.U_Id);
            }
            catch (Exception e)
            {
                Data = "Error Occurred While Saving Data.";
            }
            return Data;
        }



        [WebMethod]
        public string SaveKendoThemeBasedOnUserId(string themeval)
        {

            UserInfo user = null;
            if (Session["UserInfo"] != null)
                user = (UserInfo)Session["UserInfo"];
            else
                Response.Redirect("~/Login.aspx");
            int UserId = user.U_Id;


            string Result = "Error Occurred";
            try
            {
                var result = db.SP_SaveKendoThemeName(themeval, user.U_Id);
            }
            catch (Exception e)
            {
                Result = "Error Occurred While Saving Data.";
            }

            return Result;
        }

        public JsonResult GetThemeBasedOnUserId()
        {
            string data = "Error Occurred While Getting Theme.";
            UserInfo user = null;
            if (Session["UserInfo"] != null)
                user = (UserInfo)Session["UserInfo"];
            else
                Response.Redirect("~/Login.aspx");
            int UserId = user.U_Id;
            try
            {

                var result = db.SP_GetBothTheme(user.U_Id);
                string kendo = "";
                string global = "";

                foreach (var datarow in result)
                {
                    kendo = datarow.KendoTheme;
                    global = datarow.GlobalTheme;
                }
                var result2 = new { kendos = kendo, globals = global };

                return Json(result2, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(data, JsonRequestBehavior.AllowGet);

            }
        }

    }
}