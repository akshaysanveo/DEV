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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Globalization;
using System.Net;

namespace SanveoAIO.Controllers
{
    public class InspireController : Controller
    {
        private static string FORGE_CLIENT_ID = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID") ?? "BmNe4PXIAdDC3DfrWqX4jaaKcUVLHzGs";
        private static string FORGE_CLIENT_SECRET = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET") ?? "FvjXbdJbWzQUjkL0";
        private static string FORGE_CALLBACK_URL = Environment.GetEnvironmentVariable("FORGE_CALLBACK_URL") ?? "http://localhost:60998/api/forge/callback/oauth";
        internal const string FORGE_OAUTH = "ForgeOAuth";
        private static string BUCKET_KEY = "sampledemo";
        private static string FILE_NAME = "my-elephant.obj";
        private static string FILE_PATH = "";

        // Initialize the relevant clients; in this example, the Objects, Buckets and Derivatives clients, which are part of the Data Management API and Model Derivatives API
        private static BucketsApi bucketsApi = new BucketsApi();
        private static ObjectsApi objectsApi = new ObjectsApi();
        private static DerivativesApi derivativesApi = new DerivativesApi();
        private static FoldersApi foldersApi = new FoldersApi();
        private static HubsApi hubsApi = new HubsApi();
        private static ProjectsApi projectsApi = new ProjectsApi();
        private static ItemsApi itemsApi = new ItemsApi();
        private static TwoLeggedApi oauth2TwoLegged;
        private static dynamic twoLeggedCredentials;


        SanveoInspireEntities db = new SanveoInspireEntities();
        UserInfo user;

        // GET: Inspire
        public ActionResult Inspire()
        {
            return View();
        }

        // GET: Admin
        public ActionResult Admin()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
                ViewBag.group = user.G_Id;
                ViewBag.PageName = "Admin";
                ViewBag.compType = user.Comp_Type;
                string val = user.validity;
                ViewBag.validity = val;
                initializeOAuth();
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("Login", "Login");

        }

        // GET: SuperAdmin
        public ActionResult SuperAdmin()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
                ViewBag.group = user.G_Id;
                ViewBag.PageName = "SuperAdmin";
                string val = user.validity;
                ViewBag.validity = val;
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("Login", "Login");
        }

        // GET: Help
        public ActionResult Help()
        {
            return View();
        }

        public ActionResult AdminPopUp()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
                ViewBag.group = user.G_Id;
                ViewBag.PageName = "AdminPopUp";
                ViewBag.compType = user.Comp_Type;
                string val = user.validity;
                ViewBag.validity = val;
                initializeOAuth();
                return View();
            }
            else
                return View();

        }

        public ActionResult SuperAdminPopUp()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
                ViewBag.group = user.G_Id;
                ViewBag.PageName = "SuperAdminPopUp";
                ViewBag.compType = user.Comp_Type;
                string val = user.validity;
                ViewBag.validity = val;
                initializeOAuth();
                return View();
            }
            else
                return View();
        }

        public JsonResult GetCompany([DataSourceRequest] DataSourceRequest request)
        {
            var data = db.Database.SqlQuery<SP_GetCompany_Result>("EXEC SP_GetCompany").ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult SaveCompany(int CompID, string CompName, string Active, string Type)
        //{
        //    var Error = new ObjectParameter("Error", typeof(string));
        //    user = (UserInfo) Session["UserInfo"];
        //    if (Active == "")
        //    {
        //        Active = "false";
        //    }
        //    bool isactive = bool.Parse(Active);
        //    var result = db.SP_SaveCompany(CompID, CompName, isactive, user.U_Id, Type, Error);
        //    var data = Error.Value.ToString();
        //    return this.Json(new DataSourceResult { Errors = data });
        //}

        public JsonResult SaveCompany(int CompID, string CompName, string Active, string Type, string UploadedFile)
        {
            var Error = new ObjectParameter("Error", typeof(string));
            string data = "";
            user = (UserInfo)Session["UserInfo"];
            if (Active == "")
            {
                Active = "false";
            }
            bool isactive = bool.Parse(Active);
            //var result = db.SP_SaveCompany(CompID, CompName, isactive, user.U_Id, Type, Error);
            //var data = Error.Value.ToString();

            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.Int,
                    Value = CompID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = CompName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Active",
                    SqlDbType = SqlDbType.Int,
                    Value = isactive,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CompType",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Type,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@FileName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = UploadedFile,
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
                DataSet dataSet = SqlManager.ExecuteDataSet("SP_SaveCompany", parameters.ToArray());
                data = error.Value.ToString();
                // return Json(data, JsonRequestBehavior.AllowGet);
                return this.Json(new DataSourceResult { Errors = data });
            }
            catch (Exception ex)
            {
                data = "Error Occurred While Saving Data";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult DeleteCompany(int CompID)
        {
            var Error = new ObjectParameter("Error", typeof(string));
            user = (UserInfo)Session["UserInfo"];
            var result = db.SP_DeleteCompany(CompID, user.U_Id, Error);
            var data = Error.Value.ToString();

            //var data = db.SP_DeleteCompany(CompID, user.U_Id);
            //return Json(data, JsonRequestBehavior.AllowGet);
            return this.Json(new DataSourceResult
            {
                Errors = data
            });
        }

        public ActionResult GetUsers([DataSourceRequest] DataSourceRequest request, string compId)
        {

            var result = db.Database.SqlQuery<Sp_GetUsers_Result>("Sp_GetUsers @Compid={0}", compId).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompanyID()
        {
            IEnumerable<Sp_GetCompanyID_Result> items = db.Database.SqlQuery<Sp_GetCompanyID_Result>("Sp_GetCompanyID").ToList().Select(c => new Sp_GetCompanyID_Result
            {
                CompID = c.CompID,
                CompName = c.CompName
            });

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserGroup()
        {
            IEnumerable<Sp_GetUserGroup_Result> items = db.Database.SqlQuery<Sp_GetUserGroup_Result>("Sp_GetUserGroup").ToList().Select(c => new Sp_GetUserGroup_Result
            {
                Group_Id = c.Group_Id,
                Group_Name = c.Group_Name
            });

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAdminUserGroup()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            IEnumerable<Sp_GetAdminUserGroup_Result> items = db.Database.SqlQuery<Sp_GetAdminUserGroup_Result>("Sp_GetAdminUserGroup @Group_Id={0}", user.G_Id).ToList().Select(c => new Sp_GetAdminUserGroup_Result
            {
                Group_Id = c.Group_Id,
                Group_Name = c.Group_Name
            });

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUser(string U_Id, string FirstName, string LastName, string Username, string Password, string Emailid, string MobileNo, int Group_Name, int CompName, string Active, string ExpiredDate)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            int userid = Int32.Parse(U_Id);
            string data = "";
            if (Active == "")
                Active = "false";
            bool isactive = bool.Parse(Active);
            try
            {
                var Error = new ObjectParameter("Error", typeof(string));
                var result = db.sp_SaveUser(userid, FirstName, LastName, Username, Password, Emailid, MobileNo, CompName, Group_Name, isactive, user.U_Id, ExpiredDate, Error, "");
                data = Error.Value.ToString();
                //return Json(data, JsonRequestBehavior.AllowGet);
                return this.Json(new DataSourceResult
                { Errors = data });
            }
            catch (Exception e)
            {
                data = e.Message;
                return this.Json(new DataSourceResult
                {
                    Errors = data
                });
            }
        }

        public JsonResult GetGroup([DataSourceRequest] DataSourceRequest request)
        {
            var data = db.Database.SqlQuery<SP_GetGroup_Result>("EXEC SP_GetGroup").ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveGroup(int Group_Id, string Group_Name)
        {
            user = (UserInfo)Session["UserInfo"];
            var data = db.SP_SaveGroup(Group_Id, Group_Name, user.U_Id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteGroup(int Group_Id)
        {
            var Error = new ObjectParameter("Error", typeof(string));
            user = (UserInfo)Session["UserInfo"];
            var data = db.SP_DeleteGroup(Group_Id, user.U_Id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private static void initializeOAuth()
        {
            Scope[] scopes = new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.BucketCreate, Scope.BucketRead, Scope.BucketDelete };


            oauth2TwoLegged = new TwoLeggedApi();
            twoLeggedCredentials = oauth2TwoLegged.Authenticate(FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, oAuthConstants.CLIENT_CREDENTIALS, scopes);
            bucketsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            objectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            derivativesApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            foldersApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
        }

        public JsonResult SaveFolder()
        {
            string FinalFolderid = Session["FinalFolderid"].ToString();

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];

            string Hub_id = dataSet2.Tables[0].Rows[0]["HubId"].ToString();
            project_id = dataSet2.Tables[0].Rows[0]["ProjectId"].ToString();


            var result = db.SP_SaveFolderDetails(FinalFolderid, project_id, user.Comp_ID);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFile()
        {
            string nodedata = "";
            string storedata = "";

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFolderDetials", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];

            string FolderId = dataSet2.Tables[0].Rows[0]["FolderId"].ToString();
            string ProjectId = dataSet2.Tables[0].Rows[0]["ProjectId"].ToString();

            dynamic AllContents = foldersApi.GetFolderContents(ProjectId, FolderId);
            string json = AllContents.ToString();
            JObject ser = JObject.Parse(json);
            List<JToken> data = ser.Children().ToList();
            string ProjList = "";
            foreach (JProperty item in data)
            {
                item.CreateReader();
                switch (item.Name)
                {
                    case "metadata":
                        break;
                    case "included":
                        foreach (JObject msg in item.Values())
                        {
                            string projid = "";
                            // msg("id").ToString
                            string projname = "";
                            string versionno = "";
                            string lastModifiedTime = "";
                            List<JToken> data2 = msg.Children().ToList();
                            foreach (JProperty item2 in data2)
                            {
                                try
                                {
                                    if (item2.Name.ToString() == "attributes")
                                    {
                                        List<JToken> data3 = item2.Children().ToList();
                                        List<JToken> data4 = data3.Children().ToList();
                                        foreach (JProperty item4 in data4)
                                        {
                                            if (item4.Name.ToString() == "displayName")
                                            {
                                                projname = item4.First.ToString().Replace(".rvt", "");
                                            }
                                            if (item4.Name.ToString() == "lastModifiedTime")
                                            {
                                                lastModifiedTime = item4.First.ToString();
                                            }
                                            if (item4.Name.ToString() == "versionNumber")
                                            {
                                                versionno = item4.First.ToString();
                                            }
                                        }
                                    }

                                    if (item2.Name.ToString() == "relationships")
                                    {
                                        List<JToken> data3 = item2.Children().ToList();
                                        List<JToken> data4 = data3.Children().ToList();
                                        foreach (JProperty item4 in data4)
                                        {
                                            if (item4.Name.ToString() == "derivatives")
                                            {
                                                List<JToken> data5 = item4.Children().ToList();
                                                foreach (var item5 in data5.Values())
                                                {
                                                    //string item5 = item5_loopVariable;

                                                    List<JToken> data6 = item5.Children().ToList();
                                                    foreach (var item6 in data6.Values())
                                                    {
                                                        //item6 = item6_loopVariable;
                                                        if (item6.Previous != null)
                                                        {
                                                            if (item6.Previous.ToString().Contains("derivatives") == true)
                                                            {
                                                                // projname = item6.Item(1).
                                                                projid = item6.First.ToString();
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                { }

                            }

                            ProjList = ProjList + Environment.NewLine + projid + Environment.NewLine + projname;


                            //  storedata += projname + "," + projid + "," + versionno + "," + lastModifiedTime + "|";

                            nodedata += projid + "," + projname + "," + versionno + "," + lastModifiedTime + "|";


                        }



                        break;
                }
            }


            return Json(nodedata, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFoldersDetails([DataSourceRequest] DataSourceRequest request)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var data = db.Database.SqlQuery<SP_GetFoldersDetails_Result>("EXEC SP_GetFoldersDetails @CompId={0},@userId={1}", user.Comp_ID, user.U_Id).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompanyType(string compid)
        {
            string data = "";
            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = Convert.ToInt16(compid),
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("GetCompanyType", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                string type = dataSet2.Tables[0].Rows[0]["Type"].ToString();
                data = type;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //Profile

        public JsonResult SaveProfile(int Id, string Profile, string Color)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string data = "";
            try
            {
                var Error = new ObjectParameter("Error", typeof(string));
                var result = db.sp_SaveProfile(Id, Profile, Color, user.U_Id, user.Comp_ID, Error);
                data = Error.Value.ToString();
                return this.Json(new DataSourceResult { Errors = data });
            }
            catch (Exception e)
            {
                data = e.Message;
                return this.Json(new DataSourceResult { Errors = data });
            }
        }

        public JsonResult GetProfile()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            IEnumerable<SP_GetUserProfile_Result> items = db.Database.SqlQuery<SP_GetUserProfile_Result>("SP_GetUserProfile @CompId={0}", user.Comp_ID).ToList().Select(c => new SP_GetUserProfile_Result
            {
                Id = c.Id,
                Profile = c.Profile
            });
            return Json(items, JsonRequestBehavior.AllowGet);
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

        public JsonResult GetUserProfile([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            var data = db.Database.SqlQuery<SP_GetUserProfile_Result>("EXEC SP_GetUserProfile @CompId={0}", user.Comp_ID).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteProfile(int Id)
        {
            var Error = new ObjectParameter("Error", typeof(string));
            user = (UserInfo)Session["UserInfo"];
            var data = db.SP_DeleteProfile(Id, user.U_Id, Error);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // User Tab of Admin

        public ActionResult GetCompanyUsers([DataSourceRequest] DataSourceRequest request, string compid, string usertype)
        {
            DataSourceResult result1 = new DataSourceResult();
            int type = 0;
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            if (!(int.TryParse(usertype, out type)))
            {
                type = 0;
            }

            if (string.IsNullOrWhiteSpace(compid))
            {
                user = (UserInfo)Session["UserInfo"];
                var result = db.Database.SqlQuery<Sp_GetCompanyUsers_Result>("Sp_GetCompanyUsers  @C_ID={0},@UserId={1},@User_type={2}", user.Comp_ID, user.U_Id, type).ToList();
                result1 = result.ToDataSourceResult(request);

            }
            else
            {
                var result = db.Database.SqlQuery<Sp_GetCompanyUsers_Result>("Sp_GetCompanyUsers  @C_ID={0},@UserId={1},@User_type={2}", compid, user.U_Id, type).ToList();
                result1 = result.ToDataSourceResult(request);
            }
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveCompanyUser(string U_Id, string FirstName, string LastName, string Username, string Password, string Emailid, string MobileNo, int Group_Name, string Profile, string Active, string compid, string Trade)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (Active == "")
                Active = "false";

            int userid = Int32.Parse(U_Id);
            string data = "";
            bool isactive = bool.Parse(Active);
            try
            {
                if (compid == "")
                {
                    var Error = new ObjectParameter("Error", typeof(string));
                    var result = db.sp_SaveCompanyUser(userid, FirstName, LastName, Username, Password, Emailid, MobileNo, Group_Name, isactive, user.U_Id, Error, 0, Convert.ToInt16(Trade));
                    data = Error.Value.ToString();

                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@U_ID",
                        SqlDbType = SqlDbType.Int,
                        Value = userid,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@ProfileId",
                        SqlDbType = SqlDbType.VarChar,
                        Value = Profile,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet ds1 = SqlManager.ExecuteDataSet("SP_SaveUserProfiles", parameters1.ToArray());
                    //return Json(data, JsonRequestBehavior.AllowGet);
                    return this.Json(new DataSourceResult
                    {
                        Errors = data
                    });
                }
                else
                {
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = Convert.ToInt16(compid),
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet ds = SqlManager.ExecuteDataSet("SP_GetAdminId", parameters.ToArray());
                    DataTable dt = ds.Tables[0];
                    int uID = 0;
                    if (dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            uID = Convert.ToInt16(dt.Rows[0]["U_ID"]);
                        }
                    }

                    var Error = new ObjectParameter("Error", typeof(string));
                    var result = db.sp_SaveCompanyUser(userid, FirstName, LastName, Username, Password, Emailid, MobileNo, Group_Name, isactive, uID, Error, 0, Convert.ToInt16(Trade));
                    data = Error.Value.ToString();

                    // For profile Mapping

                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@U_ID",
                        SqlDbType = SqlDbType.Int,
                        Value = userid,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@ProfileId",
                        SqlDbType = SqlDbType.VarChar,
                        Value = Profile,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet ds1 = SqlManager.ExecuteDataSet("SP_SaveUserProfiles", parameters1.ToArray());

                    //return Json(data, JsonRequestBehavior.AllowGet);
                    return this.Json(new DataSourceResult
                    {
                        Errors = data
                    });
                }

            }
            catch (Exception e)
            {
                data = e.Message;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteUser(string U_Id)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            int UserId = Int32.Parse(U_Id);
            string data = "";

            var Error = new ObjectParameter("Error", typeof(string));
            var result = db.sp_DeleteUser(UserId, user.U_Id, Error);
            data = Error.Value.ToString();
            //return Json(data, JsonRequestBehavior.AllowGet);
            return this.Json(new DataSourceResult
            {
                Errors = data
            });

        }


        //Project Details Tab of Admin
        public string[] GetProjects(string HubID)
        {

            string[] ProjectNameFinal = new string[100000];

            string[] Projectid = new string[1000];
            string[] ProjectName = new string[1000];
            string[] Folderid = new string[100];
            string[] FolderName = new string[100];
            int pi = 0;
            int pn = 0;
            int fi = 0;
            int Fname = 0;


            projectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            dynamic AllProperties = projectsApi.GetHubProjects(HubID);
            string Projectjson = AllProperties.ToString();

            JObject project = JObject.Parse(Projectjson);
            List<JToken> projectdata = project.Children().ToList();
            foreach (JProperty item in projectdata)
            {
                item.CreateReader();
                switch (item.Name)
                {
                    case "jsonapi":
                        break;
                    case "data":
                        List<JToken> projectdata1 = item.Values().Children().ToList();

                        foreach (JProperty msg in projectdata1)
                        {
                            if (msg.Name.ToString() == "id")
                            {
                                Projectid[pi] += msg.Value.ToString();
                                pi++;
                            }

                            if (msg.Name.ToString() == "attributes")
                            {
                                List<JToken> data3 = msg.Children().ToList();
                                foreach (JProperty item4 in data3.Values())
                                {
                                    if (item4.Name.ToString() == "name")
                                    {
                                        ProjectName[pn] += item4.Value.ToString().Replace(".rvt", "");
                                        pn++;
                                    }
                                }
                            }

                            if (msg.Name.ToString() == "relationships")
                            {
                                List<JToken> data3 = msg.Children().ToList();
                                foreach (JProperty item4 in data3.Values())
                                {
                                    if (item4.Name.ToString() == "rootFolder")
                                    {
                                        List<JToken> data34 = item4.Children().ToList();
                                        foreach (JProperty item44 in data34.Values())
                                        {
                                            if (item44.Name.ToString() == "data")
                                            {
                                                List<JToken> data344 = item44.Children().ToList();
                                                foreach (JProperty item444 in data344.Values())
                                                {
                                                    if (item444.Name.ToString() == "type")
                                                    {
                                                        FolderName[Fname] += item444.Value.ToString();
                                                        Fname++;
                                                    }
                                                    if (item444.Name.ToString() == "id")
                                                    {
                                                        Folderid[fi] += item444.Value.ToString();
                                                        fi++;
                                                    }
                                                }

                                            }
                                        }

                                    }
                                }
                            }
                        }
                        break;
                }
            }

            Array.Resize(ref Projectid, pi);
            Array.Resize(ref ProjectName, pn);
            Array.Resize(ref Folderid, fi);
            Array.Resize(ref FolderName, Fname);

            int k = 0;
            if (Projectid.Count() > 0)
            {
                for (int i = 0; i < Projectid.Count(); i++)
                {
                    string pname = ProjectName[i].ToString();
                    string pid = Projectid[i].ToString();

                    //if (pname == folderid)
                    //{
                    ProjectNameFinal[k] += Projectid[i] + "~" + ProjectName[i] + "~" + Folderid[i] + "~" + FolderName[i];
                    k++;
                    //break;
                    // }
                }
            }


            Array.Resize(ref ProjectNameFinal, k);
            return ProjectNameFinal;
        }

        public JsonResult GetNodes([DataSourceRequest] DataSourceRequest request, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            DataSourceResult result = new DataSourceResult();
            if (string.IsNullOrWhiteSpace(compid))
            {
                var getdata = "";
                //  var result = db.SP_GetNodeDetils(user.U_Id);
                // var getdata = db.Database.SqlQuery<SP_GetNodeDetils_Result>("SP_GetNodeDetils  @userid={0}", user.Comp_ID).ToList();
                result = getdata.ToDataSourceResult(request);
            }
            else
            {

                var getdata = db.Database.SqlQuery<SP_GetNodeDetils_Result>("SP_GetNodeDetils  @userid={0}", Convert.ToInt16(compid)).ToList();
                result = getdata.ToDataSourceResult(request);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveHub(string Hubid, string compid)
        {
            string data = "";

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (compid == "")
            {
                // var result = db.SP_SaveNodeDetails(Hubid, user.Comp_ID);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.SP_SaveNodeDetails(Hubid, Convert.ToInt16(compid), user.U_Id);
                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetddlProjects(string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string data = "";

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            if (compid == "")
            {
                //parameters1.Add(new SqlParameter()
                //{
                //    ParameterName = "@userid",
                //    SqlDbType = SqlDbType.Int,
                //    Value = user.Comp_ID,
                //    Direction = System.Data.ParameterDirection.Input
                //});
            }
            else
            {
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@userid",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt16(compid),
                    Direction = System.Data.ParameterDirection.Input
                });
            }

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {

                string Hub_id = dataSet2.Tables[0].Rows[0]["HubId"].ToString();
                // string   projectid = dataSet2.Tables[0].Rows[0]["ProjectId"].ToString();
                // string  projectname = dataSet2.Tables[0].Rows[0]["ProjectName"].ToString();

                var items = GetProjects(Hub_id);
                string projName = "";
                String projID = "";
                String FolderName = "";
                String folderID = "";

                for (int j = 0; j < items.Length; j++)
                {
                    var Totalitems = items[j].Split('~');


                    projID = Totalitems[0].ToString();
                    projName = Totalitems[1].ToString();
                    folderID = Totalitems[2].ToString();
                    FolderName = Totalitems[3].ToString();


                    data += projID + "~" + projName + "~" + folderID + "~" + FolderName + "|";
                    // var result = db.SaveProjectid(projName, projID);


                }

            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveNode(string projectid, string projectname, string folderid, string compid)
        {
            string data = "";

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            if (compid == "")
            {
                // var result = db.SP_SaveNodeFolderDetails(projectid, projectname, folderid, user.Comp_ID);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = db.SP_SaveNodeFolderDetails(projectid, projectname, folderid, Convert.ToInt16(compid), user.U_Id);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }


        //Project Tree Tab of Admin
        public string[] GetFolders(string ProjectID, string FolderID)
        {

            string[] FoldersFinal = new string[100000];

            try
            {
                foldersApi.Configuration.AccessToken = twoLeggedCredentials.access_token;

                dynamic AllContents1 = foldersApi.GetFolderContents(ProjectID, FolderID);
                string json333 = AllContents1.ToString();
                JObject set = JObject.Parse(json333);
                List<JToken> dataw = set.Children().ToList();

                string[] folderid = new string[100];
                string[] foldername = new string[100];

                int foi = 0;
                int fon = 0;

                string[] itemid = new string[100];
                string[] itemname = new string[100];
                int itn = 0;
                int iti = 0;


                int itemflag = 0;
                int Subfolderflag = 0;

                foreach (JProperty item in dataw)
                {
                    item.CreateReader();
                    switch (item.Name)
                    {
                        case "jsonapi":
                            break;
                        case "data":

                            foreach (JObject msg in item.Values())
                            {
                                List<JToken> data2 = msg.Children().ToList();
                                foreach (JProperty item2 in data2)
                                {
                                    if (item2.Name.ToString() == "id")
                                    {
                                        folderid[foi] = item2.Value.ToString();
                                        foi++;
                                    }

                                    if (item2.Name.ToString() == "attributes")
                                    {
                                        List<JToken> data3 = item2.Children().ToList();
                                        List<JToken> data4 = data3.Children().ToList();
                                        foreach (JProperty item4 in data4)
                                        {
                                            if (item4.Name.ToString() == "displayName")
                                            {
                                                foldername[fon] = item4.Value.ToString().Replace(".rvt", "");
                                                fon++;

                                            }
                                        }
                                    }

                                }

                            }

                            break;
                    }
                }

                Array.Resize(ref folderid, foi);
                Array.Resize(ref foldername, fon);

                int k = 0;
                for (int j = 0; j < folderid.Count(); j++)
                {
                    FoldersFinal[k] += folderid[j] + "~" + foldername[j];
                    k++;
                }


                Array.Resize(ref FoldersFinal, k);

            }

            catch (Exception ex)
            {
                string projid = "";
                string projname = "";
                string versionno = "";
                string lastModifiedTime = "";
                string storesize = "";
                dynamic AllContents = itemsApi.GetItem(ProjectID, FolderID);
                string json = AllContents.ToString();
                JObject ser = JObject.Parse(json);
                List<JToken> data = ser.Children().ToList();
                foreach (JProperty item in data)
                {
                    item.CreateReader();
                    switch (item.Name)
                    {
                        case "included":
                            foreach (JObject msg in item.Values())
                            {

                                List<JToken> data2 = msg.Children().ToList();
                                foreach (JProperty item2 in data2)
                                {
                                    try
                                    {
                                        if (item2.Name.ToString() == "attributes")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "displayName")
                                                {
                                                    projname = item4.First.ToString().Replace(".rvt", "");
                                                }
                                                if (item4.Name.ToString() == "lastModifiedTime")
                                                {
                                                    lastModifiedTime = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "versionNumber")
                                                {
                                                    versionno = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "storageSize")
                                                {
                                                    storesize = item4.First.ToString();
                                                }
                                            }
                                        }

                                        if (item2.Name.ToString() == "relationships")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "derivatives")
                                                {
                                                    List<JToken> data5 = item4.Children().ToList();
                                                    foreach (var item5 in data5.Values())
                                                    {
                                                        //string item5 = item5_loopVariable;

                                                        List<JToken> data6 = item5.Children().ToList();
                                                        foreach (var item6 in data6.Values())
                                                        {
                                                            //item6 = item6_loopVariable;
                                                            if (item6.Previous != null)
                                                            {
                                                                if (item6.Previous.ToString().Contains("derivatives") == true)
                                                                {
                                                                    // projname = item6.Item(1).
                                                                    projid = item6.First.ToString();
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex1)
                                    { }

                                }

                                Session["projid"] = projid;
                                Session["projname"] = projname;
                                Session["versionno"] = versionno;
                                Session["lastModifiedTime"] = lastModifiedTime;
                                Session["FileSize"] = storesize;
                                Session["ItemId"] = FolderID;
                            }


                            break;
                    }
                }


            }


            return FoldersFinal;
        }

        string project_id = "";
        string projectname = "";
        public ActionResult Nodes(string node, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            Session["FinalFolderid"] = node;
            var list = new List<SanveoAIO.lib.Node>();

            try
            {
                List<SqlParameter> parameters1 = new List<SqlParameter>();

                if (string.IsNullOrWhiteSpace(compid))
                {
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@userid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                }
                else
                {
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@userid",
                        SqlDbType = SqlDbType.Int,
                        Value = compid,
                        Direction = System.Data.ParameterDirection.Input
                    });
                }

                DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());

                DataTable datatable = dataSet2.Tables[0];


                if (datatable.Rows.Count == 0)
                {
                    list.Add(new SanveoAIO.lib.Node("No Project Found", "1", "", true));
                }
                else
                {
                    string Hub_id = dataSet2.Tables[0].Rows[0]["HubId"].ToString();
                    project_id = dataSet2.Tables[0].Rows[0]["ProjectId"].ToString();
                    projectname = dataSet2.Tables[0].Rows[0]["ProjectName"].ToString();
                    string folderid = dataSet2.Tables[0].Rows[0]["FolderId"].ToString();

                    if (node == null)
                    {
                        list.Add(new SanveoAIO.lib.Node(projectname, folderid, "", true));
                    }
                    else
                    {

                        var items = GetFolders(project_id, node);
                        string projName = "";
                        String projID = "";
                        String FolderName = "";
                        String folderID = "";

                        for (int j = 0; j < items.Length; j++)
                        {
                            if (items.Length == 100000)
                            {
                                break;
                            }
                            else
                            {
                                var Totalitems = items[j].Split('~');

                                folderID = Totalitems[0].ToString();
                                FolderName = Totalitems[1].ToString();

                                list.Add(new SanveoAIO.lib.Node(FolderName, folderID, "", true));
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {


            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FinalNodes(string node, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var list = new List<SanveoAIO.lib.Node>();

            if (node == null)
            {
                list.Add(new SanveoAIO.lib.Node("EnterPrise Files", "1", "", true));
            }
            else
            {
                if (node == "1")
                {
                    List<SqlParameter> parameters1 = new List<SqlParameter>();

                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetEnterPriseDetails", parameters1.ToArray());
                    DataTable datatable = dataSet2.Tables[0];
                    if (datatable.Rows.Count > 0)
                    {
                        for (int j = 0; j < datatable.Rows.Count; j++)
                        {
                            string FileName = dataSet2.Tables[0].Rows[j]["FileName"].ToString();
                            string Urn = dataSet2.Tables[0].Rows[j]["Urn"].ToString();
                            string FolderId = dataSet2.Tables[0].Rows[j]["FolderId"].ToString();
                            string Size = dataSet2.Tables[0].Rows[j]["Size"].ToString();
                            string ModifiedData = dataSet2.Tables[0].Rows[j]["ModifiedData"].ToString();
                            string Version = dataSet2.Tables[0].Rows[j]["Version"].ToString();

                            list.Add(new SanveoAIO.lib.Node(FileName, Urn, "", true));
                        }

                    }
                }
                else
                {


                }


            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEnterPriseFolderDetail(string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string nodedata = "";

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetEnterPriseDetails", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                for (int j = 0; j < datatable.Rows.Count; j++)
                {
                    string FileName = dataSet2.Tables[0].Rows[j]["FileName"].ToString();
                    string Urn = dataSet2.Tables[0].Rows[j]["Urn"].ToString();
                    string FolderId = dataSet2.Tables[0].Rows[j]["FolderId"].ToString();
                    string Size = dataSet2.Tables[0].Rows[j]["Size"].ToString();
                    string ModifiedData = dataSet2.Tables[0].Rows[j]["ModifiedData"].ToString();
                    string Version = dataSet2.Tables[0].Rows[j]["Version"].ToString();

                    List<SqlParameter> parameters2 = new List<SqlParameter>();

                    parameters2.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters2.Add(new SqlParameter()
                    {
                        ParameterName = "@urn",
                        SqlDbType = SqlDbType.VarChar,
                        Value = Urn,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters2.Add(new SqlParameter()
                    {
                        ParameterName = "@filename",
                        SqlDbType = SqlDbType.VarChar,
                        Value = FileName,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dataSet3 = SqlManager.ExecuteDataSet("SP_CheckEnterPriseFiles", parameters2.ToArray());
                    if (dataSet3.Tables[0].Rows.Count > 0)
                        nodedata += Urn + "," + FileName + ",checked |";
                    else
                        nodedata += Urn + "," + FileName + ",|";
                }

            }


            return Json(nodedata, JsonRequestBehavior.AllowGet);
        }


        public string getFilePath(string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("FileName");
            dt.Columns.Add("ModifiedDate");
            dt.Columns.Add("Version");
            dt.Columns.Add("FilePath");
            DataRow dr;

            if (user.Comp_Type == "Personal Account" || user.Comp_Type == "All" || user.Comp_Type == "")
            {
                if (user.G_Id == 1)
                {
                    string data1 = "";
                    List<SqlParameter> parameters1 = new List<SqlParameter>();
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = Convert.ToInt16(compid),
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dataSet2 = SqlManager.ExecuteDataSet("GetCompanyType", parameters1.ToArray());

                    DataTable datatable = dataSet2.Tables[0];
                    if (datatable.Rows.Count > 0)
                    {
                        string type = dataSet2.Tables[0].Rows[0]["Type"].ToString();
                        data1 = type;
                    }

                    if (data1 == "Personal Account" || data1 == "All")
                    {
                        string nodedata = "";
                        //ObjectsApi objects = new ObjectsApi();
                        //objects.Configuration.AccessToken = twoLeggedCredentials.access_token;
                        initializeOAuth();
                        var objectsList = objectsApi.GetObjects(BUCKET_KEY, 100);
                        foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                        {
                            string urn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(((string)objInfo.Value.objectId)));
                            //if (urn.Contains("=="))
                            //{
                            //    urn = urn.Substring(0, urn.Length - 2);
                            //}
                            if (urn.Contains("=="))
                            {
                                urn = urn.Substring(0, urn.Length - 2);
                            }
                            if (urn.Contains("="))
                            {
                                urn = urn.Substring(0, urn.Length - 1);
                            }

                            nodedata += urn + "," + (string)objInfo.Value.objectKey + "|";

                            dr = dt.NewRow();
                            dr["FileName"] = (string)objInfo.Value.objectKey;
                            dr["ModifiedDate"] = "";
                            dr["Version"] = 1;
                            dr["FilePath"] = BUCKET_KEY;
                            dt.Rows.Add(dr);
                        }
                    }


                }
                else
                {
                    string nodedata = "";
                    //ObjectsApi objects = new ObjectsApi();
                    //objects.Configuration.AccessToken = twoLeggedCredentials.access_token;
                    initializeOAuth();
                    var objectsList = objectsApi.GetObjects(BUCKET_KEY, 100);
                    foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                    {
                        string urn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(((string)objInfo.Value.objectId)));
                        //if (urn.Contains("=="))
                        //{
                        //    urn = urn.Substring(0, urn.Length - 2);
                        //}
                        if (urn.Contains("=="))
                        {
                            urn = urn.Substring(0, urn.Length - 2);
                        }
                        if (urn.Contains("="))
                        {
                            urn = urn.Substring(0, urn.Length - 1);
                        }

                        nodedata += urn + "," + (string)objInfo.Value.objectKey + "|";

                        dr = dt.NewRow();
                        dr["FileName"] = (string)objInfo.Value.objectKey;
                        dr["ModifiedDate"] = "";
                        dr["Version"] = 1;
                        dr["FilePath"] = BUCKET_KEY;
                        dt.Rows.Add(dr);
                    }

                }

            }

            if (user.Comp_Type == "Enterprise 360 Account" || user.Comp_Type == "All" || user.Comp_Type == "")
            {
                if (user.G_Id == 1)
                {
                    string data1 = "";
                    List<SqlParameter> parameters2 = new List<SqlParameter>();
                    parameters2.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = Convert.ToInt16(compid),
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dataSet2 = SqlManager.ExecuteDataSet("GetCompanyType", parameters2.ToArray());

                    DataTable datatable = dataSet2.Tables[0];
                    if (datatable.Rows.Count > 0)
                    {
                        string type = dataSet2.Tables[0].Rows[0]["Type"].ToString();
                        data1 = type;
                    }

                    if (data1 == "Enterprise 360 Account" || data1 == "All")
                    {

                        var list = new List<SanveoAIO.lib.Node>();
                        List<SqlParameter> parameters1 = new List<SqlParameter>();

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@userid",
                            SqlDbType = SqlDbType.Int,
                            Value = Convert.ToInt16(compid),
                            Direction = System.Data.ParameterDirection.Input
                        });


                        DataSet dataSet23 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());
                        DataTable datatable1 = dataSet23.Tables[0];

                        if (datatable1.Rows.Count > 0)
                        {
                            string Hub_id = dataSet23.Tables[0].Rows[0]["HubId"].ToString();
                            project_id = dataSet23.Tables[0].Rows[0]["ProjectId"].ToString();
                            projectname = dataSet23.Tables[0].Rows[0]["ProjectName"].ToString();
                            string folderid = dataSet23.Tables[0].Rows[0]["FolderId"].ToString();
                            Scope[] scopes = new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.BucketCreate, Scope.BucketRead };

                            oauth2TwoLegged = new TwoLeggedApi();
                            twoLeggedCredentials = oauth2TwoLegged.Authenticate(FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, oAuthConstants.CLIENT_CREDENTIALS, scopes);
                            bucketsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                            objectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                            derivativesApi.Configuration.AccessToken = twoLeggedCredentials.access_token;

                            //string ProjID = "b.f99c8a05-e163-44fa-98df-3be91077084e";
                            //string FoldID = "urn:adsk.wipprod:fs.folder:co.doflr9o2S0GFwKEK91pTLQ";
                            string ProjID = project_id;
                            string FoldID = folderid;

                            foldersApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                            dynamic AllProperties = foldersApi.GetFolder(ProjID, FoldID);

                            var folderContents = foldersApi.GetFolderContents(ProjID, FoldID);
                            string items = "", item1 = "", item2 = "", item3 = "", item4 = "";
                            string lastModifiedTime = "";
                            string version = "";
                            List<string> Itemslist = new List<string>();
                            foreach (KeyValuePair<string, dynamic> folderContentItem in new DynamicDictionaryItems(folderContents.data))
                            {
                                if (folderContentItem.Value.type == "folders")
                                {
                                    string displayName = folderContentItem.Value.attributes.displayName;
                                    string fid = folderContentItem.Value.id;
                                    item1 = displayName;
                                    var folderContents1 = foldersApi.GetFolderContents(ProjID, fid);
                                    foreach (KeyValuePair<string, dynamic> folderContentItem1 in new DynamicDictionaryItems(folderContents1.data))
                                    {
                                        //second                        
                                        if (folderContentItem1.Value.type == "folders")
                                        {
                                            string displayName1 = folderContentItem1.Value.attributes.displayName;
                                            string fid1 = folderContentItem1.Value.id;
                                            item2 = displayName1;
                                            var folderContents2 = foldersApi.GetFolderContents(ProjID, fid1);
                                            foreach (KeyValuePair<string, dynamic> folderContentItem2 in new DynamicDictionaryItems(folderContents2.data))
                                            {
                                                //third                                
                                                if (folderContentItem2.Value.type == "folders")
                                                {
                                                    string displayName2 = folderContentItem2.Value.attributes.displayName;
                                                    string fid2 = folderContentItem2.Value.id;
                                                    item3 = displayName2;
                                                    var folderContents3 = foldersApi.GetFolderContents(ProjID, fid2);
                                                    foreach (KeyValuePair<string, dynamic> folderContentItem3 in new DynamicDictionaryItems(folderContents3.data))
                                                    {
                                                        //fourth                                       
                                                        if (folderContentItem3.Value.type == "folders")
                                                        {

                                                        }
                                                        else
                                                        {
                                                            //4
                                                            //string displayName3 = folderContentItem3.Value.attributes.displayName;
                                                            ItemsApi itemsApi = new ItemsApi();
                                                            itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                                            dynamic item = itemsApi.GetItem(ProjID, folderContentItem3.Value.id);
                                                            displayName = item.included[0].attributes.displayName;
                                                            lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                                            version = Convert.ToString(item.included[0].attributes.versionNumber);

                                                            dr = dt.NewRow();
                                                            dr["FileName"] = displayName;
                                                            dr["ModifiedDate"] = lastModifiedTime;
                                                            dr["Version"] = version;
                                                            dr["FilePath"] = item1 + "\\" + item2 + "\\" + item3;
                                                            dt.Rows.Add(dr);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //3
                                                    //string displayName2 = folderContentItem2.Value.attributes.displayName;
                                                    ItemsApi itemsApi = new ItemsApi();
                                                    itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                                    dynamic item = itemsApi.GetItem(ProjID, folderContentItem2.Value.id);
                                                    displayName = item.included[0].attributes.displayName;
                                                    lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                                    version = Convert.ToString(item.included[0].attributes.versionNumber);
                                                    dr = dt.NewRow();
                                                    dr["FileName"] = displayName;
                                                    dr["ModifiedDate"] = lastModifiedTime;
                                                    dr["Version"] = version;
                                                    dr["FilePath"] = item1 + "\\" + item2;
                                                    dt.Rows.Add(dr);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //2
                                            string displayName1 = folderContentItem1.Value.attributes.displayName;
                                            ItemsApi itemsApi = new ItemsApi();
                                            itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                            dynamic item = itemsApi.GetItem(ProjID, folderContentItem1.Value.id);
                                            displayName = item.included[0].attributes.displayName;
                                            lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                            version = Convert.ToString(item.included[0].attributes.versionNumber);
                                            dr = dt.NewRow();
                                            dr["FileName"] = displayName;
                                            dr["ModifiedDate"] = lastModifiedTime;
                                            dr["Version"] = version;
                                            dr["FilePath"] = item1;
                                            dt.Rows.Add(dr);
                                        }
                                    }
                                }

                                else
                                {
                                    //1
                                    string displayName = folderContentItem.Value.attributes.displayName;
                                    ItemsApi itemsApi = new ItemsApi();
                                    itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                    dynamic item = itemsApi.GetItem(ProjID, folderContentItem.Value.id);
                                    displayName = item.included[0].attributes.displayName;
                                    lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                    version = Convert.ToString(item.included[0].attributes.versionNumber);
                                    dr = dt.NewRow();
                                    dr["FileName"] = displayName;
                                    dr["ModifiedDate"] = lastModifiedTime;
                                    dr["Version"] = version;
                                    dr["FilePath"] = item1;
                                    dt.Rows.Add(dr);
                                }
                                //string items1 = items.Split(',')[0];
                                //items = items.Replace(",", items1);
                                //Itemslist.Add(items);
                                //items = "";

                            }
                        }
                    }
                }
                else
                {
                    var list = new List<SanveoAIO.lib.Node>();
                    List<SqlParameter> parameters1 = new List<SqlParameter>();

                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@userid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });


                    DataSet dataSet24 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());
                    DataTable datatable4 = dataSet24.Tables[0];
                    if (datatable4.Rows.Count > 0)
                    {
                        string Hub_id = dataSet24.Tables[0].Rows[0]["HubId"].ToString();
                        project_id = dataSet24.Tables[0].Rows[0]["ProjectId"].ToString();
                        projectname = dataSet24.Tables[0].Rows[0]["ProjectName"].ToString();
                        string folderid = dataSet24.Tables[0].Rows[0]["FolderId"].ToString();
                        Scope[] scopes = new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.BucketCreate, Scope.BucketRead };

                        oauth2TwoLegged = new TwoLeggedApi();
                        twoLeggedCredentials = oauth2TwoLegged.Authenticate(FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, oAuthConstants.CLIENT_CREDENTIALS, scopes);
                        bucketsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                        objectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                        derivativesApi.Configuration.AccessToken = twoLeggedCredentials.access_token;

                        //string ProjID = "b.f99c8a05-e163-44fa-98df-3be91077084e";
                        //string FoldID = "urn:adsk.wipprod:fs.folder:co.doflr9o2S0GFwKEK91pTLQ";
                        string ProjID = project_id;
                        string FoldID = folderid;

                        foldersApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                        dynamic AllProperties = foldersApi.GetFolder(ProjID, FoldID);

                        var folderContents = foldersApi.GetFolderContents(ProjID, FoldID);
                        string items = "", item1 = "", item2 = "", item3 = "", item4 = "";
                        string lastModifiedTime = "";
                        string version = "";
                        List<string> Itemslist = new List<string>();
                        foreach (KeyValuePair<string, dynamic> folderContentItem in new DynamicDictionaryItems(folderContents.data))
                        {
                            if (folderContentItem.Value.type == "folders")
                            {
                                string displayName = folderContentItem.Value.attributes.displayName;
                                string fid = folderContentItem.Value.id;
                                item1 = displayName;
                                var folderContents1 = foldersApi.GetFolderContents(ProjID, fid);
                                foreach (KeyValuePair<string, dynamic> folderContentItem1 in new DynamicDictionaryItems(folderContents1.data))
                                {
                                    //second                        
                                    if (folderContentItem1.Value.type == "folders")
                                    {
                                        string displayName1 = folderContentItem1.Value.attributes.displayName;
                                        string fid1 = folderContentItem1.Value.id;
                                        item2 = displayName1;
                                        var folderContents2 = foldersApi.GetFolderContents(ProjID, fid1);
                                        foreach (KeyValuePair<string, dynamic> folderContentItem2 in new DynamicDictionaryItems(folderContents2.data))
                                        {
                                            //third                                
                                            if (folderContentItem2.Value.type == "folders")
                                            {
                                                string displayName2 = folderContentItem2.Value.attributes.displayName;
                                                string fid2 = folderContentItem2.Value.id;
                                                item3 = displayName2;
                                                var folderContents3 = foldersApi.GetFolderContents(ProjID, fid2);
                                                foreach (KeyValuePair<string, dynamic> folderContentItem3 in new DynamicDictionaryItems(folderContents3.data))
                                                {
                                                    //fourth                                       
                                                    if (folderContentItem3.Value.type == "folders")
                                                    {

                                                    }
                                                    else
                                                    {
                                                        //4
                                                        //string displayName3 = folderContentItem3.Value.attributes.displayName;
                                                        ItemsApi itemsApi = new ItemsApi();
                                                        itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                                        dynamic item = itemsApi.GetItem(ProjID, folderContentItem3.Value.id);
                                                        displayName = item.included[0].attributes.displayName;
                                                        lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                                        version = Convert.ToString(item.included[0].attributes.versionNumber);

                                                        dr = dt.NewRow();
                                                        dr["FileName"] = displayName;
                                                        dr["ModifiedDate"] = lastModifiedTime;
                                                        dr["Version"] = version;
                                                        dr["FilePath"] = item1 + "\\" + item2 + "\\" + item3;
                                                        dt.Rows.Add(dr);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //3
                                                //string displayName2 = folderContentItem2.Value.attributes.displayName;
                                                ItemsApi itemsApi = new ItemsApi();
                                                itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                                dynamic item = itemsApi.GetItem(ProjID, folderContentItem2.Value.id);
                                                displayName = item.included[0].attributes.displayName;
                                                lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                                version = Convert.ToString(item.included[0].attributes.versionNumber);
                                                dr = dt.NewRow();
                                                dr["FileName"] = displayName;
                                                dr["ModifiedDate"] = lastModifiedTime;
                                                dr["Version"] = version;
                                                dr["FilePath"] = item1 + "\\" + item2;
                                                dt.Rows.Add(dr);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //2
                                        string displayName1 = folderContentItem1.Value.attributes.displayName;
                                        ItemsApi itemsApi = new ItemsApi();
                                        itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                        dynamic item = itemsApi.GetItem(ProjID, folderContentItem1.Value.id);
                                        displayName = item.included[0].attributes.displayName;
                                        lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                        version = Convert.ToString(item.included[0].attributes.versionNumber);
                                        dr = dt.NewRow();
                                        dr["FileName"] = displayName;
                                        dr["ModifiedDate"] = lastModifiedTime;
                                        dr["Version"] = version;
                                        dr["FilePath"] = item1;
                                        dt.Rows.Add(dr);
                                    }
                                }
                            }

                            else
                            {
                                //1
                                string displayName = folderContentItem.Value.attributes.displayName;
                                ItemsApi itemsApi = new ItemsApi();
                                itemsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                                dynamic item = itemsApi.GetItem(ProjID, folderContentItem.Value.id);
                                displayName = item.included[0].attributes.displayName;
                                lastModifiedTime = Convert.ToString(item.included[0].attributes.lastModifiedTime);
                                version = Convert.ToString(item.included[0].attributes.versionNumber);
                                dr = dt.NewRow();
                                dr["FileName"] = displayName;
                                dr["ModifiedDate"] = lastModifiedTime;
                                dr["Version"] = version;
                                dr["FilePath"] = item1;
                                dt.Rows.Add(dr);
                            }
                            //string items1 = items.Split(',')[0];
                            //items = items.Replace(",", items1);
                            //Itemslist.Add(items);
                            //items = "";

                        }
                    }

                }


            }

            string data = JsonConvert.SerializeObject(dt);
            return data;
        }

        public JsonResult SaveFile(string modelname, string URN, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters2 = new List<SqlParameter>();
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@Urn",
                SqlDbType = SqlDbType.VarChar,
                Value = URN,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet3 = SqlManager.ExecuteDataSet("SP_GetModelEnterpriseFile", parameters2.ToArray());
            DataTable datatable = dataSet3.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                string FolderId = "";
                string ProjectId = "";

                for (int j = 0; j < datatable.Rows.Count; j++)
                {
                     FolderId = dataSet3.Tables[0].Rows[j]["ItemId"].ToString();
                     ProjectId = dataSet3.Tables[0].Rows[j]["ProjectId"].ToString();
                }

                string projid = "";
                string projname = "";
                string versionno = "";
                string lastModifiedTime = "";
                string storesize = "";
                dynamic AllContents = itemsApi.GetItemVersions(ProjectId, FolderId);
                string json = AllContents.ToString();
                JObject ser = JObject.Parse(json);
                List<JToken> data = ser.Children().ToList();
                foreach (JProperty item in data)
                {
                    item.CreateReader();
                    switch (item.Name)
                    {
                        case "data":
                            int loop = item.Values().Count();
                            int cnt = 1;
                            foreach (JObject msg in item.Values())
                            {
                                List<JToken> data2 = msg.Children().ToList();
                                foreach (JProperty item2 in data2)
                                {
                                    try
                                    {
                                        if (item2.Name.ToString() == "attributes")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "displayName")
                                                {
                                                    projname = item4.First.ToString().Replace(".rvt", "");
                                                }
                                                if (item4.Name.ToString() == "lastModifiedTime")
                                                {
                                                    lastModifiedTime = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "versionNumber")
                                                {
                                                    versionno = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "storageSize")
                                                {
                                                    storesize = item4.First.ToString();
                                                }
                                            }
                                        }

                                        if (item2.Name.ToString() == "relationships")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "derivatives")
                                                {
                                                    List<JToken> data5 = item4.Children().ToList();
                                                    foreach (var item5 in data5.Values())
                                                    {
                                                        //string item5 = item5_loopVariable;

                                                        List<JToken> data6 = item5.Children().ToList();
                                                        foreach (var item6 in data6.Values())
                                                        {
                                                            //item6 = item6_loopVariable;
                                                            if (item6.Previous != null)
                                                            {
                                                                if (item6.Previous.ToString().Contains("derivatives") == true)
                                                                {
                                                                    // projname = item6.Item(1).
                                                                    projid = item6.First.ToString();
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    { }
                                }

                                //*for version save *//
                                var finaldata = db.SP_SaveModelUrnDetails(projname, projid, versionno, Convert.ToDateTime(lastModifiedTime), storesize, user.Comp_ID, FolderId, 0, user.U_Id);
                                cnt++;
                            }
                            break;
                    }
                }
            }

            //List<SqlParameter> parameters1 = new List<SqlParameter>();
            //if (compid == "")
            //{
            //    parameters1.Add(new SqlParameter()
            //    {
            //        ParameterName = "@compid",
            //        SqlDbType = SqlDbType.Int,
            //        Value = user.Comp_ID,
            //        Direction = System.Data.ParameterDirection.Input
            //    });
            //}
            //else
            //{
            //    parameters1.Add(new SqlParameter()
            //    {
            //        ParameterName = "@compid",
            //        SqlDbType = SqlDbType.Int,
            //        Value = Convert.ToInt16(compid),
            //        Direction = System.Data.ParameterDirection.Input
            //    });

            //}

            //parameters1.Add(new SqlParameter()
            //{
            //    ParameterName = "@Urn",
            //    SqlDbType = SqlDbType.VarChar,
            //    Value = URN,
            //    Direction = System.Data.ParameterDirection.Input
            //});

            //DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveModelEnterpriseFile", parameters1.ToArray());

            //string Filename = Session["projname"].ToString();
            //string Fileurn = Session["projid"].ToString();
            //string version = Session["versionno"].ToString();
            //string lastmodifiedDate = Session["lastModifiedTime"].ToString();
            //string ItemId = Session["ItemId"].ToString();
            //string filesize = "";
            //if (Session["FileSize"] != null)
            //{
            //    filesize = Session["FileSize"].ToString();
            //}

            //if (compid == "")
            //{
            //    var finaldata = db.SP_SaveModelUrnDetails(Filename, Fileurn, version, Convert.ToDateTime(lastmodifiedDate), filesize, user.Comp_ID, ItemId, 1, user.U_Id);
            //}
            //else
            //{
            //    var finaldata = db.SP_SaveModelUrnDetails(Filename, Fileurn, version, Convert.ToDateTime(lastmodifiedDate), filesize, Convert.ToInt16(compid), ItemId, 1, user.U_Id);
            //}



            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePersonalFile(string modelname, string URN, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string filesize = "";
            DateTime? lastmodifiedDate = null;

            //string date1 = "01/11/2015";
            //DateTime? Lastdate = Convert.ToDateTime(date1);


            //ObjectsApi objects = new ObjectsApi();
            //objects.Configuration.AccessToken = twoLeggedCredentials.access_token;

            //var objectsList = objects.GetObjectDetails(BUCKET_KEY, modelname, Lastdate, "lastModifiedDate");
            //string json = objectsList.ToString();
            //JObject set = JObject.Parse(json);
            //List<JToken> data = set.Children().ToList();
            //foreach (JProperty item in data)
            //{
            //    if (item.Name.ToString() == "objectKey")
            //    {
            //        Filename = item.Value.ToString();
            //    }

            //    if (item.Name.ToString() == "size")
            //    {
            //        filesize = item.Value.ToString();
            //    }
            //    if (item.Name.ToString() == "lastModifiedDate")
            //    {
            //        string s1 = item.Value.ToString();
            //        string s = "\"/Date(" + s1 + ")/\"";
            //        DateTimeOffset dto = JsonConvert.DeserializeObject<DateTimeOffset>(s);
            //        lastmodifiedDate = dto.UtcDateTime;

            //        //string s = item.Value.ToString();
            //        //var date = new Date(s);

            //        // DateTimeOffset dto = JsonConvert.DeserializeObject<DateTimeOffset>(s);
            //        // DateTime utc = dto.UtcDateTime;

            //    }

            //}

            if (compid == "")
            {
                var finaldata = db.SP_SavePersonalModelUrnDetails(modelname, URN, "1", lastmodifiedDate, filesize, user.Comp_ID, user.U_Id);
            }
            else { var finaldata = db.SP_SavePersonalModelUrnDetails(modelname, URN, "1", lastmodifiedDate, filesize, Convert.ToInt16(compid), user.U_Id); }

            return Json("", JsonRequestBehavior.AllowGet);
        }


        public JsonResult ScanFolder(string Folderid, string Foldername)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeDetils", parameters1.ToArray());
            string Hubid = dataSet2.Tables[0].Rows[0]["HubId"].ToString();
            string projectid = dataSet2.Tables[0].Rows[0]["ProjectId"].ToString();
            string projectname = dataSet2.Tables[0].Rows[0]["ProjectName"].ToString();
            //string folderid = dataSet2.Tables[0].Rows[0]["FolderId"].ToString();

            string[] FoldersFinal = new string[100000];
            foldersApi.Configuration.AccessToken = twoLeggedCredentials.access_token;




            dynamic AllContents1 = foldersApi.GetFolderContents(projectid, Folderid);
            // dynamic AllContents1 = foldersApi.GetBasePath();
            string json333 = AllContents1.ToString();
            JObject set = JObject.Parse(json333);
            List<JToken> dataw = set.Children().ToList();

            string[] folderid = new string[100];
            string[] foldername = new string[100];

            int foi = 0;
            int fon = 0;

            string[] itemid = new string[100];
            string[] itemname = new string[100];
            int itn = 0;
            int iti = 0;


            int itemflag = 0;
            int Subfolderflag = 0;

            foreach (JProperty item in dataw)
            {
                item.CreateReader();
                switch (item.Name)
                {
                    case "jsonapi":
                        break;
                    case "data":

                        foreach (JObject msg in item.Values())
                        {
                            List<JToken> data2 = msg.Children().ToList();
                            foreach (JProperty item2 in data2)
                            {
                                if (item2.Name.ToString() == "id")
                                {
                                    folderid[foi] = item2.Value.ToString();
                                    foi++;
                                }

                                if (item2.Name.ToString() == "attributes")
                                {
                                    List<JToken> data3 = item2.Children().ToList();
                                    List<JToken> data4 = data3.Children().ToList();
                                    foreach (JProperty item4 in data4)
                                    {
                                        if (item4.Name.ToString() == "displayName")
                                        {
                                            foldername[fon] = item4.Value.ToString().Replace(".rvt", "");
                                            fon++;

                                        }
                                    }
                                }

                            }

                        }

                        break;
                }
            }

            Array.Resize(ref folderid, foi);
            Array.Resize(ref foldername, fon);

            int k = 0;
            for (int j = 0; j < folderid.Count(); j++)
            {
                FoldersFinal[k] += folderid[j] + "~" + foldername[j];

                string projid = "";
                string projname = "";
                string versionno = "";
                string lastModifiedTime = "";
                string storesize = "";
                dynamic AllContents = itemsApi.GetItem(projectid, folderid[j]);
                string json = AllContents.ToString();
                JObject ser = JObject.Parse(json);
                List<JToken> data = ser.Children().ToList();
                foreach (JProperty item in data)
                {
                    item.CreateReader();
                    switch (item.Name)
                    {
                        case "included":
                            foreach (JObject msg in item.Values())
                            {

                                List<JToken> data2 = msg.Children().ToList();
                                foreach (JProperty item2 in data2)
                                {
                                    try
                                    {
                                        if (item2.Name.ToString() == "attributes")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "displayName")
                                                {
                                                    projname = item4.First.ToString().Replace(".rvt", "");
                                                }
                                                if (item4.Name.ToString() == "lastModifiedTime")
                                                {
                                                    lastModifiedTime = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "versionNumber")
                                                {
                                                    versionno = item4.First.ToString();
                                                }
                                                if (item4.Name.ToString() == "storageSize")
                                                {
                                                    storesize = item4.First.ToString();
                                                }
                                            }
                                        }

                                        if (item2.Name.ToString() == "relationships")
                                        {
                                            List<JToken> data3 = item2.Children().ToList();
                                            List<JToken> data4 = data3.Children().ToList();
                                            foreach (JProperty item4 in data4)
                                            {
                                                if (item4.Name.ToString() == "derivatives")
                                                {
                                                    List<JToken> data5 = item4.Children().ToList();
                                                    foreach (var item5 in data5.Values())
                                                    {
                                                        //string item5 = item5_loopVariable;

                                                        List<JToken> data6 = item5.Children().ToList();
                                                        foreach (var item6 in data6.Values())
                                                        {
                                                            //item6 = item6_loopVariable;
                                                            if (item6.Previous != null)
                                                            {
                                                                if (item6.Previous.ToString().Contains("derivatives") == true)
                                                                {
                                                                    // projname = item6.Item(1).
                                                                    projid = item6.First.ToString();
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex1)
                                    { }

                                }

                                //Session["projid"] = projid;
                                //Session["projname"] = projname;
                                //Session["versionno"] = versionno;
                                //Session["lastModifiedTime"] = lastModifiedTime;
                                //Session["FileSize"] = storesize;
                                ////Session["ItemId"] = Folderid;

                                List<SqlParameter> parameters = new List<SqlParameter>();
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@FileName",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = projname,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@Urn",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = projid,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@VersionNo",
                                    SqlDbType = SqlDbType.Int,
                                    Value = versionno,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@ModifiedDate",
                                    SqlDbType = SqlDbType.DateTime,
                                    Value = lastModifiedTime,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@compid",
                                    SqlDbType = SqlDbType.Int,
                                    Value = user.Comp_ID,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@FolderId",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = Folderid,
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                parameters.Add(new SqlParameter()
                                {
                                    ParameterName = "@ItemId",
                                    SqlDbType = SqlDbType.VarChar,
                                    Value = folderid[j],
                                    Direction = System.Data.ParameterDirection.Input
                                });
                                DataSet dataSet = SqlManager.ExecuteDataSet("SP_SaveEnterpriseFolderDetails", parameters.ToArray());
                            }


                            break;
                    }
                }

            }

            string FolderPath = GetFolderParent(projectid, Folderid);

            List<SqlParameter> parameters2 = new List<SqlParameter>();

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@FolderName",
                SqlDbType = SqlDbType.VarChar,
                Value = Foldername,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@FolderPath",
                SqlDbType = SqlDbType.VarChar,
                Value = FolderPath,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@CompID",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet3 = SqlManager.ExecuteDataSet("Sp_SavePersonalFolderPath", parameters2.ToArray());

            return Json("", JsonRequestBehavior.AllowGet);
        }

        string folderpath = "";
        public string GetFolderParent(string projectid, string Folderid)
        {
            if (Folderid != "" && Folderid != null)
            {
                dynamic AllContents1 = foldersApi.GetFolderParent(projectid, Folderid);
                // dynamic AllContents1 = foldersApi.GetBasePath();
                string json333 = AllContents1.ToString();
                JObject set = JObject.Parse(json333);
                List<JToken> dataw = set.Children().ToList();

                string folderid = "";
                string foldername = "";


                int foi = 0;
                int fon = 0;

                string[] itemid = new string[100];
                string[] itemname = new string[100];
                int itn = 0;
                int iti = 0;


                int itemflag = 0;
                int Subfolderflag = 0;

                foreach (JProperty item in dataw)
                {
                    item.CreateReader();
                    switch (item.Name)
                    {
                        case "jsonapi":
                            break;
                        case "data":
                            string json1 = item.Value.ToString();
                            JObject set1 = JObject.Parse(json1);
                            // foreach (JObject msg in json1)
                            // {
                            List<JToken> data2 = set1.Children().ToList();
                            foreach (JProperty item2 in data2)
                            {
                                if (item2.Name.ToString() == "id")
                                {
                                    folderid = item2.Value.ToString();
                                    foi++;
                                }

                                if (item2.Name.ToString() == "attributes")
                                {
                                    List<JToken> data3 = item2.Children().ToList();
                                    List<JToken> data4 = data3.Children().ToList();
                                    foreach (JProperty item4 in data4)
                                    {
                                        if (item4.Name.ToString() == "displayName")
                                        {
                                            foldername = item4.Value.ToString().Replace(".rvt", "");
                                            fon++;

                                        }
                                    }
                                }

                            }

                            // }

                            break;
                    }
                }

                if (folderpath == "")
                {
                    folderpath += foldername;
                }
                else
                {
                    if (foldername.Contains("root-folder"))
                    {
                        folderpath += "/" + foldername;
                        return folderpath;
                    }
                    else
                    {
                        folderpath += "/" + foldername;
                    }

                }


                if (folderid != "")
                {
                    GetFolderParent(projectid, folderid);
                }
            }

            string[] ff = folderpath.Split('/');
            int gg = ff.Length;
            string folderpath1 = "";
            for (int i = ff.Length; i-- > 0;)
            {
                if (folderpath1 == "")
                {
                    folderpath1 += ff[i];
                }
                else
                {
                    folderpath1 += "/" + ff[i];
                }

            }


            return folderpath1;
        }

        public JsonResult DeleteFile(string modelname)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            //int UserId = Int32.Parse(Id);
            string data = "";
            //try
            //{
            var Error = new ObjectParameter("Error", typeof(string));
            var result = db.sp_DeleteFile(modelname, user.U_Id, user.Comp_ID, Error);
            data = Error.Value.ToString();
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetPersonalModelUrnDetails()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string ActiveProject = "";
            string nodedata = "";

            List<SqlParameter> parameters2 = new List<SqlParameter>();

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet3 = SqlManager.ExecuteDataSet("SP_GetPersonalActivePrj", parameters2.ToArray());
            if (dataSet3.Tables[0].Rows.Count > 0)
            {
                ActiveProject = dataSet3.Tables[0].Rows[0]["projectName"].ToString();

               
                initializeOAuth();

                var objectsList = objectsApi.GetObjects(ActiveProject, 100);
                nodedata += ActiveProject + "#";
                foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                {
                    string urn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(((string)objInfo.Value.objectId)));

                    if (urn.Contains("=="))
                    {
                        urn = urn.Substring(0, urn.Length - 2);
                    }
                    if (urn.Contains("="))
                    {
                        urn = urn.Substring(0, urn.Length - 1);
                    }
                    string filename = (string)objInfo.Value.objectKey;
                    string[] file;
                    DateTime temp;
                    int flag = 1;
                    if (filename.Contains('_'))
                    {
                        file = filename.Split('_');
                        file = file[file.Length - 1].Split('.');
                        if (DateTime.TryParseExact(file[0], "dd-M-yyyy-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp))
                        {
                            //File with timestamp
                            flag = 0;
                        }
                        else
                        {
                            //File without timestamp
                            flag = 1;
                        }
                    }
                    if (flag == 1)
                    {
                        List<SqlParameter> parameters1 = new List<SqlParameter>();

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@compid",
                            SqlDbType = SqlDbType.Int,
                            Value = user.Comp_ID,
                            Direction = System.Data.ParameterDirection.Input
                        });
                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@urn",
                            SqlDbType = SqlDbType.VarChar,
                            Value = urn,
                            Direction = System.Data.ParameterDirection.Input
                        });
                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@filename",
                            SqlDbType = SqlDbType.VarChar,
                            Value = (string)objInfo.Value.objectKey,
                            Direction = System.Data.ParameterDirection.Input
                        });

                        DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_CheckPersonalFiles", parameters1.ToArray());
                       
                        if (dataSet2.Tables[0].Rows.Count > 0)
                            nodedata += urn + "," + (string)objInfo.Value.objectKey + ",checked |";
                        else
                            nodedata += urn + "," + (string)objInfo.Value.objectKey + ",|";
                    }

                }
               
            }
            return Json(nodedata, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetModelUrnGridData([DataSourceRequest] DataSourceRequest request, string compid)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (compid == "")
            {
                var data = db.Database.SqlQuery<GetModelUrnDetails_Result>("EXEC GetModelUrnDetails @compid={0}", user.Comp_ID).ToList();
                DataSourceResult result = data.ToDataSourceResult(request);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {

                var data = db.Database.SqlQuery<GetModelUrnDetails_Result>("EXEC GetModelUrnDetails @compid={0}", compid).ToList();
                DataSourceResult result = data.ToDataSourceResult(request);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPersonalFoldePath([DataSourceRequest] DataSourceRequest request)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            var data = db.Database.SqlQuery<SP_GetPersonalFolderPath_Result>("EXEC SP_GetPersonalFolderPath @compid={0}", user.Comp_ID).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        //Folder Management Tab of Admin
        public ActionResult AddNodes(string node, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var list = new List<SanveoAIO.lib.Node>();



            // string foldername = dataSet2.Tables[0].Rows[0]["FolderName"].ToString();

            try
            {

                if (node == null)
                {
                    List<SqlParameter> parameters1 = new List<SqlParameter>();

                    if (string.IsNullOrWhiteSpace(compid))
                    {
                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@CompId",
                            SqlDbType = SqlDbType.Int,
                            Value = user.Comp_ID,
                            Direction = System.Data.ParameterDirection.Input
                        });

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@userId",
                            SqlDbType = SqlDbType.Int,
                            Value = user.U_Id,
                            Direction = System.Data.ParameterDirection.Input
                        });
                    }
                    else
                    {

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@CompId",
                            SqlDbType = SqlDbType.Int,
                            Value = compid.ToString(),
                            Direction = System.Data.ParameterDirection.Input
                        });

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@userId",
                            SqlDbType = SqlDbType.Int,
                            Value = user.U_Id,
                            Direction = System.Data.ParameterDirection.Input
                        });
                    }

                    DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFoldersDetails", parameters1.ToArray());

                    DataTable datatable = dataSet2.Tables[0];
                    if (datatable.Rows.Count > 0)
                    {
                        for (int i = 0; i < datatable.Rows.Count; i++)
                        {
                            string foldername = dataSet2.Tables[0].Rows[i]["FolderName"].ToString();
                            string folderid = dataSet2.Tables[0].Rows[i]["Id"].ToString();

                            list.Add(new SanveoAIO.lib.Node(foldername, folderid, "", true));

                        }
                    }
                    else
                    {
                        list.Add(new SanveoAIO.lib.Node("No Folders Found", "1", "", true));
                    }




                }
                else
                {
                    List<SqlParameter> parameters1 = new List<SqlParameter>();

                    if (string.IsNullOrWhiteSpace(compid))
                    {
                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@CompId",
                            SqlDbType = SqlDbType.Int,
                            Value = user.Comp_ID,
                            Direction = System.Data.ParameterDirection.Input
                        });
                    }
                    else
                    {

                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@CompId",
                            SqlDbType = SqlDbType.Int,
                            Value = compid.ToString(),
                            Direction = System.Data.ParameterDirection.Input
                        });

                    }

                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@Folderid",
                        SqlDbType = SqlDbType.Int,
                        Value = node,
                        Direction = System.Data.ParameterDirection.Input
                    });

                    DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFolderNodes", parameters1.ToArray());

                    DataTable datatable = dataSet2.Tables[0];
                    if (datatable.Rows.Count > 0)
                    {
                        for (int i = 0; i < datatable.Rows.Count; i++)
                        {
                            string nodename = dataSet2.Tables[0].Rows[i]["NodeName"].ToString();
                            string nodeid = dataSet2.Tables[0].Rows[i]["Id"].ToString();

                            list.Add(new SanveoAIO.lib.Node(nodename, nodeid, "", true));

                        }
                    }
                    //list.Add(new SanveoAIO.lib.Node(FolderName, folderID, true));




                }


            }
            catch (Exception ex)
            {


            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getfolderddl(string compid)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (compid == "")
            {
                IEnumerable<SP_GetFoldersDetails_Result> items = db.Database.SqlQuery<SP_GetFoldersDetails_Result>("SP_GetFoldersDetails @CompId={0},@userId={1}", user.Comp_ID, user.U_Id).ToList().Select(c => new SP_GetFoldersDetails_Result
                {

                    Id = c.Id,
                    FolderName = c.FolderName
                });

                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {

                IEnumerable<SP_GetFoldersDetails_Result> items = db.Database.SqlQuery<SP_GetFoldersDetails_Result>("SP_GetFoldersDetails @CompId={0},@userId={1}", compid, user.U_Id).ToList().Select(c => new SP_GetFoldersDetails_Result
                {

                    Id = c.Id,
                    FolderName = c.FolderName
                });

                return Json(items, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult Savefoldername(string Foldername, string compid)
        {
            string data = "";

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            if (compid == "")
            {
                var result = db.SP_SaveFolderName(Foldername, user.Comp_ID, user.U_Id);
            }
            else
            {
                var result = db.SP_SaveFolderName(Foldername, Convert.ToInt16(compid), user.U_Id);
            }


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFolderFiles(string folderid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string filename = "";
            if (folderid != "")
            {
                List<SqlParameter> parameters1 = new List<SqlParameter>();

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@compid",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Folderid",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt16(folderid),
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetFolderNodes", parameters1.ToArray());

                DataTable datatable = dataSet2.Tables[0];

                if (datatable.Rows.Count > 0)
                {
                    for (int i = 0; i < datatable.Rows.Count; i++)
                    {
                        string nodename = dataSet2.Tables[0].Rows[i]["NodeName"].ToString();
                        string nodeid = dataSet2.Tables[0].Rows[i]["Id"].ToString();

                        filename += nodename + "|";
                    }
                }

            }

            return Json(filename, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getfilesnames(string compid)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (compid == "")
            {
                IEnumerable<SP_GetFiles_Result> items = db.Database.SqlQuery<SP_GetFiles_Result>("SP_GetFiles @CompId={0}, @userid={1},@groupid={2}", user.Comp_ID, user.U_Id, user.G_Id).ToList().Select(c => new SP_GetFiles_Result
                {

                    CompId = c.CompId,
                    FileName = c.FileName
                });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<SP_GetFiles_Result> items = db.Database.SqlQuery<SP_GetFiles_Result>("SP_GetFiles @CompId={0},@userid={1},@groupid={2}", compid, user.U_Id, user.G_Id).ToList().Select(c => new SP_GetFiles_Result
                {

                    CompId = c.CompId,
                    FileName = c.FileName
                });
                return Json(items, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult SaveFolderFiles(string Folderid, string FilesNames, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }


            List<SqlParameter> parameters1 = new List<SqlParameter>();
            if ((Folderid == "" || Folderid == null) && (FilesNames == "" || FilesNames == null))
            {
                string data3 = "Please select the folder & files";
                return Json(data3, JsonRequestBehavior.AllowGet);
            }
            else if (Folderid == "" || Folderid == null)
            {
                string data = "Please select the folder";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else if ((FilesNames == "" || FilesNames == null))
            {
                string data2 = "Please select the files";
                return Json(data2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string data1 = "File saved Successfully";

                int folderid = 0;
                if (!int.TryParse(Folderid, out folderid))
                {
                    folderid = 0;
                }


                if (compid == "")
                {
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                }
                else
                {
                    parameters1.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = Convert.ToInt16(compid),
                        Direction = System.Data.ParameterDirection.Input
                    });

                }

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Folderid",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt16(Folderid),
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@FilesName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = FilesNames,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });



                DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveFolderFiles", parameters1.ToArray());


                return Json(data1, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteFolder(string Folderid, string compid)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();

            if (compid == "")
            {
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@compid",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });
            }
            else
            {
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@compid",
                    SqlDbType = SqlDbType.Int,
                    Value = Convert.ToInt16(compid),
                    Direction = System.Data.ParameterDirection.Input
                });
            }


            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@folderid",
                SqlDbType = SqlDbType.Int,
                Value = Folderid,
                Direction = System.Data.ParameterDirection.Input
            });

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CurrentUserId",
                SqlDbType = SqlDbType.Int,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });


            DataSet dataSet2 = SqlManager.ExecuteDataSet("Deletefolder", parameters1.ToArray());


            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFolderaccess([DataSourceRequest] DataSourceRequest request, string folderId, string compId)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            else
            {
                throw new Exception("session expired");
            }
            int f_Id = 0;
            int C_Id = 0;
            if (!(int.TryParse(folderId, out f_Id)))
            {
                f_Id = 0;
            }
            if (compId == "0")
            {
                C_Id = user.Comp_ID;
            }
            else
            {
                if (!(int.TryParse(compId, out C_Id)))
                {
                    C_Id = 0;
                }
            }

            var data = db.Database.SqlQuery<SP_GetFolderAccess_Result>("EXEC SP_GetFolderAccess  @CompId={0},@FolderId={1}", C_Id, f_Id).ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateFolderaccessUser([DataSourceRequest] DataSourceRequest request, string U_ID, string Id, string IsAssigned, string IsViewable)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            else
            {
                throw new Exception("session expired");
            }
            var Error = new ObjectParameter("Error", typeof(string));
            string data = "Data updated successfully";
            int f_Id = 0;
            int UId = 0;
            int assign = 0;
            int viewAsign = 0;
            if (!(int.TryParse(Id, out f_Id)))
            {
                f_Id = 0;
            }
            if (!(int.TryParse(U_ID, out UId)))
            {
                UId = 0;
            }
            if (IsAssigned == "true")
            {
                assign = 1;
            }
            else
            {
                assign = 0;
            }
            if (IsViewable == "true")
            {
                viewAsign = 1;
            }
            else
            {
                viewAsign = 0;
            }
            var result = db.SP_UpdateFolderAccess(user.Comp_ID, f_Id, UId, assign, viewAsign, user.U_Id, Error);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPersonalModelUrnDetails_Database()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string nodedata = "";


            List<SqlParameter> parameters1 = new List<SqlParameter>();

            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetAllPersonalFiles", parameters1.ToArray());

            DataTable datatable = dataSet2.Tables[0];

            if (datatable.Rows.Count > 0)
            {
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    string Filename = dataSet2.Tables[0].Rows[i]["OrignalName"].ToString();
                    string Urn = dataSet2.Tables[0].Rows[i]["Urn"].ToString();
                    nodedata += Urn + "," + Filename + "|";
                }
            }
            return Json(nodedata, JsonRequestBehavior.AllowGet);
        }
        public JsonResult InsertPersonalModelUrnDetails()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string ActiveProject = "";
            List<SqlParameter> parameters2 = new List<SqlParameter>();

            parameters2.Add(new SqlParameter()
            {
                ParameterName = "@compid",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet3 = SqlManager.ExecuteDataSet("SP_GetPersonalActivePrj", parameters2.ToArray());
            if (dataSet3.Tables[0].Rows.Count > 0)
            {
                try
                {
                    ActiveProject = dataSet3.Tables[0].Rows[0]["projectName"].ToString();
                    initializeOAuth();
                    DateTime? lastmodifiedDate = null;
                    string date1 = "01/11/2015";
                    DateTime? Lastdate = Convert.ToDateTime(date1);
                    ObjectsApi objects = new ObjectsApi();
                    objects.Configuration.AccessToken = twoLeggedCredentials.access_token;
                    var objectsList = objectsApi.GetObjects(ActiveProject, 100);

                    foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                    {
                        string urn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(((string)objInfo.Value.objectId)));

                        if (urn.Contains("=="))
                        {
                            urn = urn.Substring(0, urn.Length - 2);
                        }
                        if (urn.Contains("="))
                        {
                            urn = urn.Substring(0, urn.Length - 1);
                        }
                        string filename = (string)objInfo.Value.objectKey;
                        string filesize = Convert.ToString(objInfo.Value.size);
                        var jsondata = objects.GetObjectDetails(ActiveProject, filename, Lastdate, "lastModifiedDate");
                        string json = jsondata.ToString();
                        JObject set = JObject.Parse(json);
                        List<JToken> data = set.Children().ToList();
                        foreach (JProperty item in data)
                        {
                            if (item.Name.ToString() == "lastModifiedDate")
                            {
                                string s1 = item.Value.ToString();
                                string s = "\"/Date(" + s1 + ")/\"";
                                DateTimeOffset dto = JsonConvert.DeserializeObject<DateTimeOffset>(s);
                                lastmodifiedDate = dto.UtcDateTime;
                            }
                        }
                        string[] Originalname;
                        DateTime dateValue;
                        string existingFName = "";
                        string ext = "";
                        string f = "";
                        if (filename.Contains('_'))
                        {
                            Originalname = filename.Split('_');
                            f = filename.Split('_').Last();
                            ext = Originalname[Originalname.Length - 1].Split('.').Last();
                            existingFName = filename.Replace('_' + f, "") + '.' + ext;
                            if (DateTime.TryParseExact(Originalname[Originalname.Length - 1].Replace("." + ext, ""), "dd-M-yyyy-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                            {
                                var finaldata = db.SP_SaveAllPersonalModelUrn(filename, existingFName, urn, filesize, lastmodifiedDate, ActiveProject, user.Comp_ID);
                            }
                            else
                            {
                                var finaldata = db.SP_SaveAllPersonalModelUrn(filename, filename, urn, filesize, lastmodifiedDate, ActiveProject, user.Comp_ID);
                            }
                        }
                        else
                        {
                            var finaldata = db.SP_SaveAllPersonalModelUrn(filename, filename, urn, filesize, lastmodifiedDate, ActiveProject, user.Comp_ID);
                        }
                    }
                    return Json("Data Saved Successfully", JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Set Active Project", JsonRequestBehavior.AllowGet);
            }
        }




        //public ActionResult UploadLogo(IEnumerable<HttpPostedFileBase> attachments, int uploadID)
        //{
        //    string data = "";
        //    string msg = "";
        //    foreach (var file in attachments)
        //    {

        //        string fileExtension = Path.GetExtension(file.FileName);
        //        string FileName = Path.GetFileName(file.FileName);

        //        int index = FileName.IndexOf(".");
        //        if (index > 0)
        //            FileName = FileName.Substring(0, index);

        //        if (fileExtension.ToLower() != ".png" || fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".gif")
        //        {

        //            msg = "Please upload .png files..!!!!";
        //            data = "";
        //        }
        //        if (fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".gif")
        //        {

        //            try
        //            {



        //                //String FilePath = "";
        //                //String Filename = "";

        //                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //                var random = new Random();
        //                var randomcharresult = new string(
        //                    Enumerable.Repeat(chars, 8)
        //                              .Select(s => s[random.Next(s.Length)])
        //                              .ToArray());

        //                string filename = randomcharresult + "_" + FileName + fileExtension;

        //                file.SaveAs(Server.MapPath("~/Content/") + filename);

        //                data = filename;
        //                msg = "File uploaded";



        //                //  return this.Json(new DataSourceResult { Errors = data});
        //                return this.Json(new { data = data, Message = msg });
        //            }
        //            catch (Exception ex)
        //            {
        //                var data2 = "";
        //                return Json(data2, JsonRequestBehavior.AllowGet);
        //            }
        //        }

        //        else
        //        {

        //            msg = "Please upload .png files..!!!!";
        //            data = "";

        //            return this.Json(new { data = data, Message = msg });
        //        }

        //    }

        //    return Content("");
        //}

        public ActionResult UploadLogo(IEnumerable<HttpPostedFileBase> attachments, int uploadID)
        {
            string data = "";
            string msg = "";
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            else
            {
                throw new Exception("session expired");
            }
            foreach (var file in attachments)
            {

                string fileExtension = Path.GetExtension(file.FileName);
                string FileName = Path.GetFileName(file.FileName);

                int index = FileName.IndexOf(".");
                if (index > 0)
                    FileName = FileName.Substring(0, index);

                if (fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".gif")
                {

                    try
                    {


                        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        var random = new Random();
                        var randomcharresult = new string(
                            Enumerable.Repeat(chars, 3)
                                      .Select(s => s[random.Next(s.Length)])
                                      .ToArray());
                        string exactFilename = FileName + "_" + randomcharresult + fileExtension;
                        string filename = randomcharresult + fileExtension;
                        string path = Server.MapPath("~/Content/");
                        file.SaveAs(Server.MapPath("~/Content/") + filename);
                        string originalfile = Server.MapPath("~/Content/") + filename;

                        System.Drawing.Image img = System.Drawing.Image.FromFile(originalfile);
                        int h = img.Height;
                        int w = img.Width;
                        img.Dispose();
                        //string filenewname = user.CompanyName + "_" + exactFilename;
                        string filenewname = user.CompanyName + "_" + exactFilename;
                        file.SaveAs(Server.MapPath("~/Content/") + filenewname);
                        data = filenewname;
                        //  data = resizeImage(path, filename, exactFilename, 300, 70, w, h);
                        // img.Dispose();

                        if (System.IO.File.Exists(originalfile))
                        {
                            System.GC.Collect();
                            System.GC.WaitForPendingFinalizers();
                            System.IO.File.Delete(originalfile);
                        }


                        return this.Json(new { data = data, Message = msg });
                    }
                    catch (Exception ex)
                    {
                        var data2 = "";
                        return Json(data2, JsonRequestBehavior.AllowGet);
                    }
                }

                else
                {

                    msg = "Please upload .png files..!!!!";
                    data = "";

                    return this.Json(new { data = data, Message = msg });
                }

            }

            return Content("");
        }



        public string resizeImage(string path, string originalFilename, string exactFilename,
                     int canvasWidth, int canvasHeight,
                     int originalWidth, int originalHeight)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            Image image = Image.FromFile(path + originalFilename);

            System.Drawing.Image thumbnail =
                new Bitmap(canvasWidth, canvasHeight); // changed parm names
            System.Drawing.Graphics graphic =
                         System.Drawing.Graphics.FromImage(thumbnail);

            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            /* ------------------ new code --------------- */

            // Figure out the ratio
            double ratioX = (double)canvasWidth / (double)originalWidth;
            double ratioY = (double)canvasHeight / (double)originalHeight;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            // now we can get the new height and width
            int newHeight = Convert.ToInt32(originalHeight * ratio);
            int newWidth = Convert.ToInt32(originalWidth * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            int posX = Convert.ToInt32((canvasWidth - (originalWidth * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (originalHeight * ratio)) / 2);

            graphic.Clear(System.Drawing.Color.White); // white padding
            graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            /* ------------- end new code ---------------- */

            System.Drawing.Imaging.ImageCodecInfo[] info =
                             ImageCodecInfo.GetImageEncoders();
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality,
                             100L);
            string Filename = newWidth + "." + originalFilename;
            string filenewname = user.CompanyName + "_" + exactFilename;
            thumbnail.Save(path + filenewname, info[1],
                             encoderParameters);
            thumbnail.Dispose();
            originalFilename = path + originalFilename;
            System.Drawing.Image img = System.Drawing.Image.FromFile(originalFilename);
            img.Dispose();
            return filenewname;
        }

        public JsonResult GetCompanyLogo()
        {
            // DateTime Updated_Date = System.DateTime.Now;
            string data = "";


            //   var data = db.SP_BT_SaveBugTracking(Task_ID, Module_Name, Title, "", Description, 0, Assign_To, Status, Updated_Date, user.U_Id);
            user = (UserInfo)Session["UserInfo"];
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });


                DataSet ds = SqlManager.ExecuteDataSet("SP_GetCompanyLogo", parameters.ToArray());
                if (ds.Tables[0].Rows.Count == 0)
                {

                }
                else
                {
                    data = ds.Tables[0].Rows[0]["UploadedFile"].ToString();

                }
                return Json(data, JsonRequestBehavior.AllowGet);
                // return this.Json(new DataSourceResult { Errors = data });
            }
            catch (Exception ex)
            {
                data = "Error Occurred While Saving Data";
                return Json(data, JsonRequestBehavior.AllowGet);
            }




            return Json(data, JsonRequestBehavior.AllowGet);
        }





        //Trade Tab of Admin

        public ActionResult SaveTradeName(int TradeId, string TradeName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var Error = new ObjectParameter("Error", typeof(string));

            var data = db.SP_SaveTrade(TradeId, TradeName, user.Comp_ID, Error);
            string data1 = Error.Value.ToString();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrade([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetTrade_Result>("SP_GetTrade @CompId={0}", user.Comp_ID).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteTradeName(string Id)
        {

            int UserId = Int32.Parse(Id);
            var data = db.SP_DeleteTradeName(UserId);

            return Json("Data Deleted Successfully", JsonRequestBehavior.AllowGet);


        }

        //End of Trade

        //Email tab of Admin


        public ActionResult SaveEmailName(int EmailId, string EmailName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            var Error = new ObjectParameter("Error", typeof(string));

            var data = db.SP_SaveEmail(EmailId, EmailName, user.Comp_ID, Error);
            string data1 = Error.Value.ToString();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmail([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetEmail_Result>("SP_GetEmail @CompId={0}", user.Comp_ID).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteEmailName(string Id)
        {

            int UserId = Int32.Parse(Id);
            var data = db.SP_DeleteEmailId(UserId);

            return Json("Data Deleted Successfully", JsonRequestBehavior.AllowGet);


        }

        //end of email tab

        //Access Rights

        public ActionResult GetAccessRights([DataSourceRequest] DataSourceRequest request, int? ModuleId)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            var result = db.Database.SqlQuery<GetSecurity_Result>("Exec GetSecurity @M_ID={0},@Compid={1}", ModuleId, user.Comp_ID).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetModules()
        {
            IEnumerable<SP_GetModules_Result> items = db.Database.SqlQuery<SP_GetModules_Result>("SP_GetModules").ToList().Select(c => new SP_GetModules_Result
            {
                M_ID = c.M_ID,
                ModuleName = c.ModuleName
            });

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public string SaveSecurity(string fullid, bool boolvalue)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }
                string[] words = fullid.Split('_');
                string col = words[0] + "_" + words[1];
                int id = int.Parse(words[2]);
                var Error = new ObjectParameter("Error", typeof(string));

                var result = db.SP_UpdateSecurity(id, col, user.U_Id, boolvalue, user.Comp_ID, Error);

                return "success";
            }
            catch (Exception e)
            {
                return "Error";
            }
        }

        public JsonResult GetExcelColor([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            var data = db.Database.SqlQuery<SP_GetExcelColors_Result>("EXEC SP_GetExcelColors").ToList();
            DataSourceResult result = data.ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveExcelGrid(int ID, string Name, string Color)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string data = "";
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.Int,
                    Value = ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@name",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Name,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Color",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Color,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
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
                DataSet dataSet = SqlManager.ExecuteDataSet("SP_SaveExcelColor", parameters.ToArray());
                data = error.Value.ToString();
            }
            catch (Exception e)
            {
                data = e.Message;
                return this.Json(new DataSourceResult { Errors = data });
            }

            return this.Json(new DataSourceResult
            {
                Errors = data
            });
        }



        public JsonResult DeleteExcelGrid(int ID)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            string data = "";
            List<SqlParameter> parameters = new List<SqlParameter>();
            try
            {


                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.Int,
                    Value = ID,
                    Direction = System.Data.ParameterDirection.Input
                });


                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
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
                DataSet dataSet = SqlManager.ExecuteDataSet("SP_DeleteExcelColor", parameters.ToArray());
                data = error.Value.ToString();
            }
            catch (Exception e)
            {
                data = e.Message;
                return this.Json(new DataSourceResult { Errors = data });
            }

            return this.Json(new DataSourceResult
            {
                Errors = data
            });
        }

        public JsonResult GetAllBucket(string compid)
        {
            IEnumerable<SP_GetPersonalCompPrj_Result> items = db.Database.SqlQuery<SP_GetPersonalCompPrj_Result>("SP_GetPersonalCompPrj @compid={0}", compid).ToList().Select(c => new SP_GetPersonalCompPrj_Result
            {
                projectId = c.projectId,
                projectName = c.projectName
            });

            return Json(items, JsonRequestBehavior.AllowGet);


            //var response = "";
            //string bucketKey = "";
            //int id = 0;
            //string AccessToken = bucketsApi.Configuration.AccessToken;
            //using (WebClient client = new WebClient())
            //{
            //    client.Headers.Add("content-type", "application/json");
            //    client.Headers.Add("authorization", "Bearer " + AccessToken);
            //    response = client.DownloadString("https://developer.api.autodesk.com/oss/v2/buckets");
            //}
            //string result = "";
            //JObject set = JObject.Parse(response);
            //List<JToken> data = set.Children().ToList();
            //foreach (JProperty item in data)
            //{
            //    foreach (JObject msg in item.Values())
            //    {
            //        List<JToken> data2 = msg.Children().ToList();
            //        foreach (JProperty item2 in data2)
            //        {
            //            if (item2.Name.ToString() == "bucketKey")
            //            {
            //                bucketKey = item2.Value.ToString();
            //                id = id + 1;
            //                result += id + "," + bucketKey + "|";
            //            }
            //        }
            //    }
            //}

            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveProject(string ProjectName, string CompId)
        {
            string data = "";
            initializeOAuth();
            try
            {
                createBucket(ProjectName.ToLower());
            }
            catch (Exception ex)
            {
                return Json("Project Name Already Exist", JsonRequestBehavior.AllowGet);
            }

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            var Error = new ObjectParameter("Error", typeof(string));
            var result = db.SP_SaveProjectName(ProjectName.ToLower(), Convert.ToInt16(CompId), Error);
            data = Error.Value.ToString();
            return Json(data, JsonRequestBehavior.AllowGet);

        }


        public ActionResult SetActiveProject(string ProjectId,string Compid)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@ProjectId",
                    SqlDbType = SqlDbType.Int,
                    Value = ProjectId,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Compid",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Compid,
                    Direction = System.Data.ParameterDirection.Input
                });          
                
                DataSet dataSet = SqlManager.ExecuteDataSet("SP_SetPersonalCompPrj", parameters.ToArray());
                
                return Json("Project Updated Successfully", JsonRequestBehavior.AllowGet);
        }

        private static void createBucket(string ProjectName)
        {
            PostBucketsPayload payload = new PostBucketsPayload(ProjectName, null, PostBucketsPayload.PolicyKeyEnum.Persistent);
            dynamic response = bucketsApi.CreateBucket(payload, "US");
        }



        //Start of Measurement Unit tab


        public ActionResult SaveMeasurementUnit(int MeasuremenId, string MeasuremenName, string scalevalue)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var Error = new ObjectParameter("Error", typeof(string));

            var data = db.SP_SaveMeasurementUnit(MeasuremenId, MeasuremenName, Convert.ToDouble(scalevalue), user.Comp_ID, Error);
            string data1 = Error.Value.ToString();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMeasurementUnit([DataSourceRequest] DataSourceRequest request)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetMeasurementUnit_Result>("SP_GetMeasurementUnit @CompId={0}", user.Comp_ID).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteMeasurementUnit(string Id)
        {

            int UserId = Int32.Parse(Id);
            var data = db.SP_DeleteMeasurementUnit(UserId);

            return Json("Data Deleted Successfully", JsonRequestBehavior.AllowGet);


        }


        //End of Measurement Unit Tab



        //Access To Button Tab

        public ActionResult SaveAccessName(int UID,bool BtnDextract, bool BtnVersion, bool BtnShow2D, bool BtnQuantity, bool BtnRuleEngine,bool BtnReport, bool BtnClearance, bool BtnProperty,bool BtnADAClearance, bool BtnElectrial,bool BtnAutoSearch)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var Error = new ObjectParameter("Error", typeof(string));

            var data = db.SP_UpdateUSerAccessData( UID, BtnDextract,  BtnVersion,  BtnShow2D,  BtnQuantity,  BtnRuleEngine,  BtnReport,  BtnClearance,  BtnProperty,  BtnADAClearance,  BtnElectrial, BtnAutoSearch);

            string data1 = "Data Updated Sucessfully";

            return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccess([DataSourceRequest] DataSourceRequest request)
        {
        
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetUSerAccessData_Result>("EXEC SP_GetUSerAccessData @Compid={0}", user.Comp_ID).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }


       
    }


}