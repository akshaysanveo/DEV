using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SanveoAIO.Models;
using System.Data.SqlClient;
using System.Data;
using telmvc;
using System.DirectoryServices;
using System.Web.Security;

namespace SanveoAIO.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
           
            string PageName = "LOGIN";
            string GroupName = "ADMIN";
            string PageTitle = "LOGIN";
            string u = "";
            string p = "";


            ViewBag.PageName = PageName;
            ViewBag.GroupName = GroupName;
            ViewBag.PageTitle = PageTitle;

            if (Request.Cookies["InspireUserCookie"] != null)
            {
                //uname =  Request.Cookies["SanveoUserCookie"].Value;
                u = Request.Cookies["InspireUserCookie"]["username"];
                p = Request.Cookies["InspireUserCookie"]["password"];
            }

            ViewBag.user = u;
            ViewBag.pass = p;

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(string username, string password, Login login, Login lm, string rememberme)
        {
            lm.Username = username;
            lm.Password = password;

            bool rememberMe = true;
            if (rememberme != "on")
            {
                rememberMe = false;
            }

            FormsAuthentication.SetAuthCookie(username, rememberMe);
            HttpCookie myCookie = new HttpCookie("InspireUserCookie");
            bool IsRemember = rememberMe;
            if (true)
            {
                string errorMessage = "";
                string validityRemaining = "";

                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@UserName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = lm.Username,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Password",
                    SqlDbType = SqlDbType.VarChar,
                    Value = lm.Password,
                    Direction = System.Data.ParameterDirection.Input
                });

                SqlParameter error = new SqlParameter()
                {
                    ParameterName = "@Error",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 1000,
                    Direction = System.Data.ParameterDirection.Output
                };

                SqlParameter validity = new SqlParameter()
                {
                    ParameterName = "@validity",
                    SqlDbType = SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                parameters.Add(error);
                parameters.Add(validity);

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_LoginAIO", parameters.ToArray());
                errorMessage = error.Value.ToString();
                TempData["Message"] = errorMessage;
                ViewBag.errormsg = errorMessage;
                validityRemaining = validity.Value.ToString();

                try
                {
                    if (errorMessage == "Access Granted")
                    {
                        DataTable dt1 = dataSet.Tables[0];
                        string emailid = "";
                        UserInfo user = new UserInfo();
                        user.validity = validityRemaining;
                        user.FirstName = emailid;

                        if (dt1.Rows.Count > 0)
                        {
                            emailid = dt1.Rows[0]["Emailid"].ToString();
                            user.U_Id = Convert.ToInt32(dt1.Rows[0]["U_ID"]);
                            Session["UID"] = user.U_Id;
                            user.G_Id = Convert.ToInt32(dt1.Rows[0]["Group_Id"]);
                            Session["GID"] = user.G_Id;
                            user.Comp_ID = Convert.ToInt32(dt1.Rows[0]["Comp_ID"]);
                            user.EmailId = dt1.Rows[0]["EmailId"].ToString();
                            user.FirstName = dt1.Rows[0]["FirstName"].ToString();
                            user.LastName = dt1.Rows[0]["LastName"].ToString();
                            user.UserName = dt1.Rows[0]["UserName"].ToString();
                            user.Comp_Type = dt1.Rows[0]["Type"].ToString();
                            user.Active = bool.Parse(dt1.Rows[0]["Active"].ToString());
                            user.Admin_Id = Convert.ToInt32(dt1.Rows[0]["Admin_Id"]);
                            user.Trade_ID = Convert.ToInt32(dt1.Rows[0]["Trade"]);
                            user.Profile_ID = dt1.Rows[0]["Profile"].ToString();

                            user.UserGroupName = dt1.Rows[0]["Group_Name"].ToString();
                            user.CompanyName = dt1.Rows[0]["CompName"].ToString();
                            Session["EmailId"] = dt1.Rows[0]["EmailId"].ToString();
                            Session["FullName"] = user.FirstName + " " + user.LastName;
                            Session["GroupName"] = user.UserGroupName;
                            Session["CompanyName"] = user.CompanyName;
                            Session["ProfileId"] = user.Profile_ID;
                            Session["TradeId"] = user.Trade_ID;
 
                        }

                       
                        Session["UserInfo"] = user;
                        if (IsRemember)
                        {
                            myCookie["username"] = username;
                            myCookie["password"] = password;
                            myCookie.Expires = DateTime.Now.AddDays(15);
                        }
                        else
                        {
                            myCookie["username"] = string.Empty;
                            myCookie["password"] = string.Empty;
                            myCookie.Expires = DateTime.Now.AddMinutes(1);
                        }
                        Response.Cookies.Add(myCookie);
                        Response.SetCookie(myCookie);
                        return RedirectToRoute(new { controller = "Inspire", action = "Admin", id = UrlParameter.Optional });
                    }
                    else
                    {
                        return RedirectToRoute(new { controller = "Login", action = "Login", id = UrlParameter.Optional });
                    }
                }
                catch (Exception)
                {
                    return RedirectToRoute(new { controller = "Login", action = "Login", id = UrlParameter.Optional });
                }
            }
            else
            {
                return RedirectToRoute(new { controller = "Login", action = "Login", id = UrlParameter.Optional });
            }

        }


        public ActionResult LogOut()
        {
            Session["UserInfo"] = null;
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            this.Response.Cache.SetNoStore();
            return RedirectToAction("Login", "Login");
        }
    }
}