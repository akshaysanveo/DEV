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
using System.Web.Security;
using System.Globalization;

namespace SanveoAIO.Controllers
{
    public class HomeController : Controller
    {
        private static string FORGE_CLIENT_ID = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID") ??
                                                "BmNe4PXIAdDC3DfrWqX4jaaKcUVLHzGs";

        private static string FORGE_CLIENT_SECRET =
            Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET") ?? "FvjXbdJbWzQUjkL0";

        private static string BUCKET_KEY = "sampledemo";
        private static string FILE_NAME = "my-elephant.obj";
        private static string FILE_PATH = "";

        // Initialize the relevant clients; in this example, the Objects, Buckets and Derivatives clients, which are part of the Data Management API and Model Derivatives API
        private static BucketsApi bucketsApi = new BucketsApi();

        private static ObjectsApi objectsApi = new ObjectsApi();
        private static DerivativesApi derivativesApi = new DerivativesApi();
        private static FoldersApi foldersApi = new FoldersApi();
        private static HubsApi hubApi = new HubsApi();
        private static ItemsApi itemsApi = new ItemsApi();
        private static ProjectsApi projectsApi = new ProjectsApi();

        private static TwoLeggedApi oauth2TwoLegged;
        private static dynamic twoLeggedCredentials;

        UserInfo user;


        SanveoInspireEntities db = new SanveoInspireEntities();

        // GET: Home
        public ActionResult Index()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];

                string val = user.validity;
                string username = user.FirstName;
                ViewBag.validity = val;
                ViewBag.username = username;
                ViewBag.PageName = "Index";
                ViewBag.compType = user.Comp_Type;
                ViewBag.userid = "progressBar" + user.U_Id;
                // UploadMainFile();
                initializeOAuth();

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
                    Value = 0,
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
                    ViewBag.View_RuleEngine = dataSet2.Tables[0].AsEnumerable()
                        .First(r => r.Field<string>("ModuleName") == "Rule Engine")["AR_View"].ToString();
                    ViewBag.View_Report = dataSet2.Tables[0].AsEnumerable()
                        .First(r => r.Field<string>("ModuleName") == "Report")["AR_View"].ToString();

                }


                List<SqlParameter> parameters1 = new List<SqlParameter>();
                SqlParameter userid = new SqlParameter()
                {
                    ParameterName = "@Uid",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                };

                parameters1.Add(userid);


                DataSet btnData = SqlManager.ExecuteDataSet("SP_GetUserAccessible_Buttons", parameters1.ToArray());

                ViewBag.BtnDextract = btnData.Tables[0].Rows[0]["BtnDextract"].ToString();
                ViewBag.BtnVersion = btnData.Tables[0].Rows[0]["BtnVersion"].ToString();
                ViewBag.BtnShow2D = btnData.Tables[0].Rows[0]["BtnShow2D"].ToString();
                ViewBag.BtnQuantity = btnData.Tables[0].Rows[0]["BtnQuantity"].ToString();
                ViewBag.BtnRuleEngine = btnData.Tables[0].Rows[0]["BtnRuleEngine"].ToString();
                ViewBag.BtnReport = btnData.Tables[0].Rows[0]["BtnReport"].ToString();
                ViewBag.BtnClearance = btnData.Tables[0].Rows[0]["BtnClearance"].ToString();
                ViewBag.BtnProperty = btnData.Tables[0].Rows[0]["BtnProperty"].ToString();
                ViewBag.BtnADAClearance = btnData.Tables[0].Rows[0]["BtnADAClearance"].ToString();
                ViewBag.BtnElectrial = btnData.Tables[0].Rows[0]["BtnElectrial"].ToString();
                ViewBag.BtnAutoSearch = btnData.Tables[0].Rows[0]["BtnAutoSearch"].ToString();
                return View();
            }
            else
            {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
                return RedirectToAction("Login", "Login");
            }
        }



        public ActionResult Search()
        {
            return View();
        }

        public ActionResult Clearance()
        {
            return View();
        }

        public ActionResult Report()
        {

            return View();
        }

        public ActionResult Quantity()
        {

            return View();
        }

        public ActionResult RuleEngine()
        {

            return View();
        }

        //<------ Start of Dextract Methods--------->

        public JsonResult SaveModelData(string ModelData, string VersionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            try
            {

                String urn = ModelData; //"dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6c2FtcGxlZGVtby9EZW1vX01vZGVsLnJ2dA";
                String metaid = ""; //"e0122435-61fd-d18d-c65e-c4ce929cc7ce";
                string vt = "";
                DerivativesApi derivativeObj = new DerivativesApi();
                derivativeObj.Configuration.AccessToken = twoLeggedCredentials.access_token;

                string mmm = Convert.ToString(derivativeObj.GetMetadata(urn));

                var jObj1 = JObject.Parse(mmm);

                int p = 1;
                foreach (JProperty prop in jObj1["data"])
                {
                    string ff = prop.Name;
                    string hhh = prop.Value.ToString();
                    if (p == 2)
                    {
                        JArray val2 = JArray.Parse(hhh);

                        int lp = 1;
                        foreach (JObject content in val2.Children<JObject>())
                        {
                            if (lp == 1)
                            {
                                int g = 1;
                                foreach (JProperty prop1 in content.Properties())
                                {
                                    if (g == 3)
                                    {
                                        metaid = prop1.Value.ToString();
                                    }
                                    g++;
                                }
                            }
                            lp++;
                        }
                    }

                    p++;
                }



                // string fff= Convert.ToString(derivativeObj.GetModelviewMetadata(urn, metaid));
                dynamic hierarchy = derivativeObj.GetModelviewMetadata(urn, metaid);
                dynamic properties = derivativeObj.GetModelviewProperties(urn, metaid);

                DateTime dt1 = System.DateTime.Now;

                string Forgeid = "";
                int inc1 = 0;
                string[] datarray = new string[5000000];
                string Family_Type = "";
                string Instance_Name = "";
                string Family_Name = "";
                string Revitid = "";
                dynamic data = hierarchy.data.objects[0].objects;

                Int32 Length = data.Count;
                int loop = 1;
                foreach (KeyValuePair<string, dynamic> categoryOfElements in new DynamicDictionaryItems(hierarchy.data
                    .objects[0].objects))
                {
                    int datalength = Length;

                    Single percentage = ((Single) loop / (Single) datalength) * (Single) 50;

                    Int32 hhvf = Convert.ToInt32(percentage);

                    ProgressHub.SendMessage("Modeldata", hhvf);
                    ProgressHub.GetLoop("Modeldata", hhvf, user.U_Id);


                    string categorname = categoryOfElements.Value.name;

                    List<long> ids = GetAllElements(categoryOfElements.Value.objects, categorname);

                    foreach (long id in ids)
                    {
                        Dictionary<string, object> props = GetProperties(id, properties);

                        foreach (KeyValuePair<string, object> prop in props)
                        {
                            string fff = prop.Key.ToString();
                            string fdsf = prop.Value.ToString();

                            Forgeid = id.ToString();

                            if (prop.Key.ToString() == "ID")
                            {
                                Instance_Name = prop.Value.ToString();

                                if (prop.Value.ToString().Contains("["))
                                {

                                    var dd = Instance_Name.Split('[');

                                    Family_Name = dd[0].ToString();

                                    Revitid = dd[1].ToString();
                                    Revitid = Revitid.Substring(0, dd[1].Length - 1);
                                }

                            }

                            if (prop.Key.ToString() == "Type Name")
                            {
                                Family_Type = prop.Value.ToString();
                                Family_Type = Family_Type.Replace(",", ";");
                                if (Family_Type.Contains("{") && Family_Type.Contains("}"))
                                {
                                    var dd = Family_Type.Split('"');
                                    Family_Type = "";
                                    for (int e = 0; e < dd.Length; e++)
                                    {
                                        var v = Regex.Matches(dd[e], @"[a-zA-Z]").Count;
                                        if (v > 0)
                                        {
                                            Family_Type += dd[e].ToString();
                                        }
                                    }


                                }
                            }

                            //Instance_Name = Family_Name + "[" + Revitid + "]";
                        }

                        foreach (KeyValuePair<string, object> prop in props)
                        {

                            if (prop.Key.ToString() != "ID" && prop.Key.ToString() != "Name")
                            {
                                string Property_value = prop.Value.ToString();
                                if (Property_value.Contains("{") && Property_value.Contains("}"))
                                {
                                    var dd = Property_value.Split('"');
                                    Property_value = dd[3].ToString();
                                }

                                datarray[inc1] = urn + "|>|" + Revitid + "|>|" + Forgeid + "|>|" + categorname + "|>|" +
                                                 prop.Key.ToString() + "|>|" + Property_value + "|>|" + Family_Name +
                                                 "|>|" + Family_Type + "|>|" + Instance_Name + "|>| " + VersionNo +
                                                 "|>|" + user.Comp_ID;

                                inc1++;
                            }


                        }


                    }

                    loop++;
                }


                Array.Resize(ref datarray, inc1 - 1);

                DateTime dt2 = System.DateTime.Now;


                DataTable dt = new DataTable();

                dt.Columns.Add("MGuid");
                dt.Columns.Add("Revitid");
                dt.Columns.Add("Forgeid");
                dt.Columns.Add("Category_Name");
                dt.Columns.Add("Property_Name");
                dt.Columns.Add("Property_Value");
                dt.Columns.Add("Family_Name");
                dt.Columns.Add("Family_Type");
                dt.Columns.Add("Instance_Name");
                dt.Columns.Add("Version");
                dt.Columns.Add("CompId");

                int gg = datarray.Length;

                DateTime dt3 = System.DateTime.Now;

                for (int i = 0; i < datarray.Length; i++)
                {
                    Single percentage = ((Single) i / (Single) gg) * (Single) 40;

                    Int32 hhj = Convert.ToInt32(percentage) + 50;

                    ProgressHub.SendMessage("initializing and preparing", hhj);
                    ProgressHub.GetLoop("initializing and preparing", hhj, user.U_Id);

                    string value1 = datarray[i];
                    //var value2 = value1.Split("|>|");

                    var value2 = value1.Split(new string[] { "|>|" }, StringSplitOptions.None);

                    dt.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7],
                        value2[8], value2[9], value2[10]);

                }

                DateTime dt4 = System.DateTime.Now;

                ///-----For main File Delete

                List<SqlParameter> parameters1 = new List<SqlParameter>();
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.VarChar,
                    Value = urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@versionNo",
                    SqlDbType = SqlDbType.VarChar,
                    Value = VersionNo,
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
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet12 = SqlManager.ExecuteDataSet("DeleteModelDetailsData", parameters1.ToArray());

                //var fdsfd = db.DeleteModelDetailsData(urn, VersionNo, user.Comp_ID, user.U_Id);



                ProgressHub.SendMessage("Modeldata", 95);
                ProgressHub.GetLoop("Modeldata", 95, user.U_Id);

                ///---For Main File Save
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@tblmodelDetailsData",
                    SqlDbType = SqlDbType.Structured,
                    Value = dt,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet1 = SqlManager.ExecuteDataSet("SaveModelData", parameters.ToArray());

                ProgressHub.SendMessage("Modeldata", 100);
                ProgressHub.GetLoop("Modeldata", 100, user.U_Id);

                var finaldata = db.SP_SaveModelUrnDetailsCheck(urn, VersionNo, user.Comp_ID, user.U_Id);

                ///---For Data Delete After Dextract(Custom Change for only one module)
                List<SqlParameter> parameters2 = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Mguid",
                    SqlDbType = SqlDbType.Structured,
                    Value = "",
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet delete = SqlManager.ExecuteDataSet("SP_AfterDextract", parameters2.ToArray());
                vt = "Model data Save Successfully";

                return Json(vt, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveModelDataNwd(string ModelData, string VersionNo)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            try
            {

                String urn = ModelData; //"dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6c2FtcGxlZGVtby9EZW1vX01vZGVsLnJ2dA";
                String metaid = ""; //"e0122435-61fd-d18d-c65e-c4ce929cc7ce";
                string vt = "";
                DerivativesApi derivativeObj = new DerivativesApi();
                derivativeObj.Configuration.AccessToken = twoLeggedCredentials.access_token;

                string mmm = Convert.ToString(derivativeObj.GetMetadata(urn));

                var jObj1 = JObject.Parse(mmm);

                int p = 1;
                foreach (JProperty prop in jObj1["data"])
                {
                    string ff = prop.Name;
                    string hhh = prop.Value.ToString();
                    if (p == 2)
                    {
                        JArray val2 = JArray.Parse(hhh);

                        int lp = 1;
                        foreach (JObject content in val2.Children<JObject>())
                        {
                            if (lp == 1)
                            {
                                int g = 1;
                                foreach (JProperty prop1 in content.Properties())
                                {
                                    if (g == 3)
                                    {
                                        metaid = prop1.Value.ToString();
                                    }
                                    g++;
                                }
                            }
                            lp++;
                        }
                    }

                    p++;
                }



                // string fff= Convert.ToString(derivativeObj.GetModelviewMetadata(urn, metaid));
                dynamic hierarchy = derivativeObj.GetModelviewMetadata(urn, metaid);
                dynamic properties = derivativeObj.GetModelviewProperties(urn, metaid);

                DateTime dt1 = System.DateTime.Now;

                string Forgeid = "";
                int inc1 = 0;
                string[] datarray = new string[5000000];
                string Family_Type = "";
                string Instance_Name = "";
                string Family_Name = "";
                string Revitid = "";
                dynamic data = hierarchy.data.objects[0].objects;

                Int32 Length = data.Count;
                int loop = 1;
                foreach (KeyValuePair<string, dynamic> categoryOfElements in new DynamicDictionaryItems(hierarchy.data
                    .objects[0].objects))
                {
                    int datalength = Length;

                    Single percentage = ((Single) loop / (Single) datalength) * (Single) 50;

                    Int32 hhvf = Convert.ToInt32(percentage);

                    ProgressHub.SendMessage("Modeldata", hhvf);
                    ProgressHub.GetLoop("Modeldata", hhvf, user.U_Id);

                    //  dynamic datanwd = categoryOfElements.Value.objects;

                    foreach (KeyValuePair<string, dynamic> categoryOfElementsnwd in new DynamicDictionaryItems(categoryOfElements.Value.objects))
                    {

                        //string categorname = datanwd.name;

                        //List<long> ids = GetAllElements(datanwd.objects);

                        string categorname = categoryOfElementsnwd.Value.name;

                        List<long> ids = GetAllElements(categoryOfElementsnwd.Value.objects, categorname);

                        foreach (long id in ids)
                        {
                            Dictionary<string, object> props = GetProperties(id, properties);

                            foreach (KeyValuePair<string, object> prop in props)
                            {
                                string fff = prop.Key.ToString();
                                string fdsf = prop.Value.ToString();

                                Forgeid = id.ToString();

                                if (prop.Key.ToString() == "ID")
                                {
                                    Instance_Name = prop.Value.ToString();

                                    if (prop.Value.ToString().Contains("["))
                                    {

                                        var dd = Instance_Name.Split('[');

                                        Family_Name = dd[0].ToString();

                                        Revitid = dd[1].ToString();
                                        Revitid = Revitid.Substring(0, dd[1].Length - 1);
                                    }

                                }

                                if (prop.Key.ToString() == "Type Name")
                                {
                                    Family_Type = prop.Value.ToString();
                                    Family_Type = Family_Type.Replace(",", ";");
                                    if (Family_Type.Contains("{") && Family_Type.Contains("}"))
                                    {
                                        var dd = Family_Type.Split('"');
                                        Family_Type = "";
                                        for (int e = 0; e < dd.Length; e++)
                                        {
                                            var v = Regex.Matches(dd[e], @"[a-zA-Z]").Count;
                                            if (v > 0)
                                            {
                                                Family_Type += dd[e].ToString();
                                            }
                                        }
                                    }
                                }

                                //Instance_Name = Family_Name + "[" + Revitid + "]";
                            }

                            foreach (KeyValuePair<string, object> prop in props)
                            {

                                if (prop.Key.ToString() != "ID" && prop.Key.ToString() != "Name")
                                {
                                    string Property_value = prop.Value.ToString();
                                    if (Property_value.Contains("{") && Property_value.Contains("}"))
                                    {
                                        var dd = Property_value.Split('"');
                                        Property_value = dd[3].ToString();
                                    }

                                    datarray[inc1] = urn + "|>|" + Revitid + "|>|" + Forgeid + "|>|" + categorname + "|>|" +
                                                     prop.Key.ToString() + "|>|" + Property_value + "|>|" + Family_Name +
                                                     "|>|" + Family_Type + "|>|" + Instance_Name + "|>| " + VersionNo +
                                                     "|>|" + user.Comp_ID;

                                    inc1++;
                                }


                            }


                        }
                    }

                    loop++;
                }


                Array.Resize(ref datarray, inc1 - 1);

                DateTime dt2 = System.DateTime.Now;


                DataTable dt = new DataTable();

                dt.Columns.Add("MGuid");
                dt.Columns.Add("Revitid");
                dt.Columns.Add("Forgeid");
                dt.Columns.Add("Category_Name");
                dt.Columns.Add("Property_Name");
                dt.Columns.Add("Property_Value");
                dt.Columns.Add("Family_Name");
                dt.Columns.Add("Family_Type");
                dt.Columns.Add("Instance_Name");
                dt.Columns.Add("Version");
                dt.Columns.Add("CompId");

                int gg = datarray.Length;

                DateTime dt3 = System.DateTime.Now;

                for (int i = 0; i < datarray.Length; i++)
                {
                    Single percentage = ((Single) i / (Single) gg) * (Single) 40;

                    Int32 hhj = Convert.ToInt32(percentage) + 50;

                    ProgressHub.SendMessage("initializing and preparing", hhj);
                    ProgressHub.GetLoop("initializing and preparing", hhj, user.U_Id);

                    string value1 = datarray[i];
                    //var value2 = value1.Split("|>|");

                    var value2 = value1.Split(new string[] { "|>|" }, StringSplitOptions.None);

                    dt.Rows.Add(value2[0], value2[1], value2[2], value2[3], value2[4], value2[5], value2[6], value2[7],
                        value2[8], value2[9], value2[10]);

                }

                DateTime dt4 = System.DateTime.Now;

                ///-----For main File Delete

                List<SqlParameter> parameters1 = new List<SqlParameter>();
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = SqlDbType.VarChar,
                    Value = urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@versionNo",
                    SqlDbType = SqlDbType.VarChar,
                    Value = VersionNo,
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
                    ParameterName = "@CurrentUserId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.U_Id,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet12 = SqlManager.ExecuteDataSet("DeleteModelDetailsData", parameters1.ToArray());

                //var fdsfd = db.DeleteModelDetailsData(urn, VersionNo, user.Comp_ID, user.U_Id);



                ProgressHub.SendMessage("Modeldata", 95);
                ProgressHub.GetLoop("Modeldata", 95, user.U_Id);

                ///---For Main File Save
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@tblmodelDetailsData",
                    SqlDbType = SqlDbType.Structured,
                    Value = dt,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet1 = SqlManager.ExecuteDataSet("SaveModelData", parameters.ToArray());

                ProgressHub.SendMessage("Modeldata", 100);
                ProgressHub.GetLoop("Modeldata", 100, user.U_Id);

                var finaldata = db.SP_SaveModelUrnDetailsCheck(urn, VersionNo, user.Comp_ID, user.U_Id);

                vt = "Model data Save Successfully";

                return Json(vt, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        private Dictionary<string, object> GetProperties(long id, dynamic properties)
        {
            string tempstr;
            Dictionary<string, object> returnProps = new Dictionary<string, object>();
            foreach (KeyValuePair<string, dynamic> objectProps in new DynamicDictionaryItems(properties.data.collection)
            )
            {
                if (objectProps.Value.objectid != id) continue;
                string name = objectProps.Value.name;
                try
                {
                    long elementId = long.Parse(Regex.Match(name, @"\d+").Value);
                }
                catch (Exception ex)
                {
                }

                returnProps.Add("ID", name);
                //  returnProps.Add("Name", name.Replace("[" + elementId.ToString() + "]", string.Empty));

                foreach (KeyValuePair<string, dynamic> objectPropsGroup in new DynamicDictionaryItems(objectProps.Value
                    .properties))
                {
                    if (objectPropsGroup.Key.StartsWith("__")) continue;
                    foreach (KeyValuePair<string, dynamic> objectProp in new DynamicDictionaryItems(objectPropsGroup
                        .Value))
                    {
                        if (!returnProps.ContainsKey(objectProp.Key))
                            returnProps.Add(objectProp.Key, objectProp.Value);
                        else
                            tempstr = (objectProp.Key);
                    }
                }
            }
            return returnProps;
        }

        private List<long> GetAllElements(dynamic objects, string category)
        {

            List<long> ids = new List<long>();
            List<long> ids1 = new List<long>();

            foreach (KeyValuePair<string, dynamic> item in new DynamicDictionaryItems(objects))
            {
                foreach (KeyValuePair<string, dynamic> keys in item.Value.Dictionary)
                {
                    if (keys.Key.Equals("objects"))
                    {
                        ids1 = GetAllElements1(item.Value.objects);
                        ids.AddRange(ids1);
                    }
                }

                if (category == "Rooms")
                {
                    foreach (KeyValuePair<string, dynamic> element in objects.Dictionary)
                    {
                        if (!ids.Contains(element.Value.objectid))
                            ids.Add(element.Value.objectid);
                    }
                }


            }
            return ids;
        }

        private List<long> GetAllElements1(dynamic objects)
        {
            List<long> ids = new List<long>();
            List<long> ids1 = new List<long>();
            foreach (KeyValuePair<string, dynamic> item in new DynamicDictionaryItems(objects))
            {
                foreach (KeyValuePair<string, dynamic> keys in item.Value.Dictionary)
                {
                    if (keys.Key.Equals("objects"))
                    {
                        ids1 = GetAllElements2(item.Value.objects);
                        ids.AddRange(ids1);
                    }
                }
                //foreach (KeyValuePair<string, dynamic> element in objects.Dictionary)
                //{
                //    if (!ids.Contains(element.Value.objectid))
                //        ids.Add(element.Value.objectid);
                //}

            }
            return ids;
        }

        private List<long> GetAllElements2(dynamic objects)
        {
            List<long> ids = new List<long>();
            foreach (KeyValuePair<string, dynamic> item in new DynamicDictionaryItems(objects))
            {
                //foreach (KeyValuePair<string, dynamic> keys in item.Value.Dictionary)
                //{
                //    if (keys.Key.Equals("objects"))
                //    {
                //        return GetAllElements2(item.Value.objects);
                //    }
                //}
                foreach (KeyValuePair<string, dynamic> element in objects.Dictionary)
                {
                    if (!ids.Contains(element.Value.objectid))
                        ids.Add(element.Value.objectid);
                }

            }
            return ids;
        }

        //<<------End of Dextract Methods ---------->>


        //<------ Start of PageLoad Methods-------->

        public JsonResult GetFileUploadData()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetAllNodeUrnDetails", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                for (int j = 0; j < datatable.Rows.Count; j++)
                {
                    string foldername = dataSet2.Tables[0].Rows[j]["FolderName"].ToString();
                    string filename = dataSet2.Tables[0].Rows[j]["FileName"].ToString();
                    string fileurn = dataSet2.Tables[0].Rows[j]["Urn"].ToString();
                    string version = dataSet2.Tables[0].Rows[j]["VersionNo"].ToString();
                    string ItemId = dataSet2.Tables[0].Rows[j]["ItemId"].ToString();
                    string ProjectId = dataSet2.Tables[0].Rows[j]["ProjectId"].ToString();
                    DateTime? modified = null;
                    if (dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString() != "")
                        modified = Convert.ToDateTime(dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString());

                    //* For cross check to Dextract or not
                    string errormessage = "";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@FileName",
                        SqlDbType = SqlDbType.VarChar,
                        Value = filename,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@Urn",
                        SqlDbType = SqlDbType.VarChar,
                        Value = fileurn,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@VersionNo",
                        SqlDbType = SqlDbType.VarChar,
                        Value = version,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@ModifiedDate",
                        SqlDbType = SqlDbType.DateTime,
                        Value = modified,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    SqlParameter message = new SqlParameter()
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.VarChar,
                        Size = 1000,
                        Direction = System.Data.ParameterDirection.Output
                    };
                    parameters.Add(message);
                    DataSet dataSet =
                        SqlManager.ExecuteDataSet("SP_SaveModelUrnDetailsCrossCheck", parameters.ToArray());
                    errormessage = message.Value.ToString();
                    if (filename != "")
                    {
                        if (errormessage == "New File")
                        {
                            nodedata += fileurn + "," + filename + " <span style = 'color:skyblue'>(V" + version +
                                        ")</span>" +
                                        " <span style='font-size:15px;color:red' class='fa fa-exclamation-circle' id='" +
                                        fileurn + "'></span>" + "," + foldername + "," + version + "," + filename +
                                        "|";
                        }
                        else
                        {
                            nodedata += fileurn + "," + filename + " <span style = 'color:skyblue'>(V" + version +
                                        ")</span>" + "," + foldername + "," + version + "," + filename + "|";

                        }
                    }

                }
            }

            return Json(nodedata, JsonRequestBehavior.AllowGet);
        }

        //<<------End of PageLoad Methods ------------>>

        //<------ Start of Viewer Methods-------->
        public JsonResult GetAuthentication()
        {
            initializeOAuth();
            string auth = twoLeggedCredentials.access_token;
            Int64 expires_in = twoLeggedCredentials.expires_in;
            return Json(new { auth = auth, expires_in = expires_in }, "text/plain");
        }
        public JsonResult GetAuthentication_Active()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
                ActiveProject = dataSet3.Tables[0].Rows[0]["projectName"].ToString();
            }
            initializeOAuth();
            string auth = twoLeggedCredentials.access_token;
            Int64 expires_in = twoLeggedCredentials.expires_in;

            return Json(new { auth = auth, expires_in = expires_in, ActiveProject = ActiveProject }, "text/plain");
        }
        private static void initializeOAuth()
        {
            Scope[] scopes = new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.BucketCreate, Scope.BucketRead };

            oauth2TwoLegged = new TwoLeggedApi();
            twoLeggedCredentials = oauth2TwoLegged.Authenticate(FORGE_CLIENT_ID, FORGE_CLIENT_SECRET,
                oAuthConstants.CLIENT_CREDENTIALS, scopes);
            bucketsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            objectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
            derivativesApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
        }

        //<<------End of Viewer Methods ------------>>


        //<------ Start of Load File Methods-------->

        public ActionResult SaveMainFile(IEnumerable<HttpPostedFileBase> files)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }
            string ActiveProject = "";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);

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
                            ParameterName = "@filename",
                            SqlDbType = SqlDbType.VarChar,
                            Value = fileName,
                            Direction = System.Data.ParameterDirection.Input
                        });
                        parameters1.Add(new SqlParameter()
                        {
                            ParameterName = "@BucketKey",
                            SqlDbType = SqlDbType.VarChar,
                            Value = ActiveProject,
                            Direction = System.Data.ParameterDirection.Input
                        });


                        DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_CheckPersonalFile", parameters1.ToArray());
                        if (dataSet2.Tables[0].Rows.Count > 0)
                        {
                            fileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + "_" +
                                       DateTime.Now.ToString("dd-M-yyyy-HH-mm-ss") +
                                       System.IO.Path.GetExtension(file.FileName);
                        }
                        var physicalPath = System.IO.Path.Combine(Server.MapPath("~/UploadFiles/") + fileName);

                        FILE_PATH = fileName;

                        if (file != null && file.ContentLength > 0)
                        {
                            Stream stream = file.InputStream;
                            file.SaveAs(physicalPath);
                            Session["ObjID"] = UploadMainFile(ActiveProject);
                        }
                    }

                }
            }
            return Content("");
        }

        public JsonResult CheckPersonalFileExist(string fileName)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
                ActiveProject = dataSet3.Tables[0].Rows[0]["projectName"].ToString();

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
                    ParameterName = "@filename",
                    SqlDbType = SqlDbType.VarChar,
                    Value = fileName,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@BucketKey",
                    SqlDbType = SqlDbType.VarChar,
                    Value = ActiveProject,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_CheckPersonalFile", parameters1.ToArray());
                if (dataSet2.Tables[0].Rows.Count > 0)
                {
                    string[] withoutext = fileName.Split('.');
                    fileName = withoutext[0] + "_" + DateTime.Now.ToString("dd-M-yyyy-HH-mm-ss") + '.' + withoutext[1];
                }
            }
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        public JsonResult translatejob()
        {
            string ObjId = Session["ObjID"].ToString();
            return Json(ObjId, JsonRequestBehavior.AllowGet);
        }



        public JsonResult DeleteModelFile(string ModelName)
        {
            initializeOAuth();

            ApiResponse<object> response = objectsApi.DeleteObjectWithHttpInfo(BUCKET_KEY, ModelName);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public string UploadMainFile(string ActiveProject)
        {
            dynamic uploadedObject = null;

            try
            {
                initializeOAuth();


                uploadedObject = uploadFile(ActiveProject);

                //try
                //{

                //    var job = JObject.Parse(translateToSVF(uploadedObject.objectId));

                //    if (job.result == "success" || job.result == "created")
                //    {
                //        string base64Urn = job.urn;
                //        string manifest = verifyJobComplete(base64Urn);
                //        if (manifest == "success")
                //        {
                //            //ViewBag.HtmlData = openViewer(manifest.urn);
                //        }

                //    }
                //}
                //catch (Exception ex)
                //{

                //}

            }
            catch (Exception ex)
            {

            }

            return uploadedObject.objectId;
            // return Json(uploadedObject.objectId, JsonRequestBehavior.AllowGet);

        }

        private static void createBucket(string BucketName)
        {
            PostBucketsPayload payload =
                new PostBucketsPayload(BucketName, null, PostBucketsPayload.PolicyKeyEnum.Persistent);
            dynamic response = bucketsApi.CreateBucket(payload, "US");
        }

        private dynamic uploadFile(string BucketName)
        {
            string path = FILE_PATH;
            dynamic finalRes = null;
            if (!System.IO.File.Exists(path))
                path = Server.MapPath("~/UploadFiles/") + FILE_PATH;
            try
            {
                using (StreamReader streamReader = new StreamReader(path))
                {

                    long fileSize = new System.IO.FileInfo(path).Length;
                    finalRes = objectsApi.UploadObject(BucketName,
                       FILE_PATH, (int) streamReader.BaseStream.Length, streamReader.BaseStream,
                       "application/octet-stream");
                }
                //long fileSize = new System.IO.FileInfo(path).Length;
                ////size of piece, say 2M    
                //long chunkSize = 2 * 1024 * 1024;
                ////pieces count
                //long nbChunks = (long)Math.Round(0.5 + (double)fileSize / (double)chunkSize);
                //dynamic finalRes = null;
                //using (StreamReader streamReader = new StreamReader(path))
                //{
                //    string sessionId = RandomString(12);
                //    for (int i = 0; i < nbChunks; i++)
                //    {
                //        //start binary position of one certain piece 
                //        long start = i * chunkSize;
                //        //end binary position of one certain piece 
                //        //if the size of last piece is bigger than  total size of the file, end binary 
                //        // position will be the end binary position of the file 
                //        long end = Math.Min(fileSize, (i + 1) * chunkSize) - 1;

                //        //tell Forge about the info of this piece
                //        string range = "bytes " + start + "-" + end + "/" + fileSize;
                //        // length of this piece
                //        long length = end - start + 1;

                //        //read the file stream of this piece
                //        byte[] buffer = new byte[length];
                //        //char[] chars = Encoding.Unicode.GetChars(buffer);
                //        char[] buffer1 = new char[length];
                //        MemoryStream memoryStream = new MemoryStream(buffer);

                //        int nb = streamReader.Read(buffer1, 0, (int)length);
                //        memoryStream.Write(buffer, 0, nb);
                //        memoryStream.Position = 0;

                //        //upload the piece to Forge bucket
                //        dynamic response = objectsApi.UploadChunk(BucketName,
                //                FILE_PATH, (int)length, range, sessionId, memoryStream,
                //                "application/octet-stream");
                //        finalRes = response;
                // }

                return finalRes;
                //  }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            //string path = FILE_PATH;
            //if (!System.IO.File.Exists(path))
            //    path = Server.MapPath("~/UploadFiles/") + FILE_PATH;

            //using (StreamReader streamReader = new StreamReader(path))
            //{
            //    dynamic response = objectsApi.UploadObject(BucketName,
            //        FILE_PATH, (int) streamReader.BaseStream.Length, streamReader.BaseStream,
            //        "application/octet-stream");

            //    return (response);
            //}
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string AccessToken
        {
            get
            {
                //var cookies = Request.Headers.GetCookies();
                //var accessToken = cookies[0].Cookies[0].Value;
                //return accessToken;

                Scope[] scopes = new Scope[] { Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.BucketCreate, Scope.BucketRead, Scope.BucketDelete };
                oauth2TwoLegged = new TwoLeggedApi();
                twoLeggedCredentials = oauth2TwoLegged.Authenticate(FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, oAuthConstants.CLIENT_CREDENTIALS, scopes);


                objectsApi.Configuration.AccessToken = twoLeggedCredentials.access_token;
                return twoLeggedCredentials.access_token;
            }
        }

        private string translateToSVF(string urn)
        {
            //JobPayloadInput jobInput = new JobPayloadInput(
            //    System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(urn))
            //);
            //JobPayloadOutput jobOutput = new JobPayloadOutput(
            //    new List<JobPayloadItem>(
            //        new JobPayloadItem[]
            //        {
            //            new JobPayloadItem(
            //                JobPayloadItem.TypeEnum.Svf,
            //                new List<JobPayloadItem.ViewsEnum>(
            //                    new JobPayloadItem.ViewsEnum[] {JobPayloadItem.ViewsEnum._3d}
            //                ),
            //                null
            //            )
            //        }
            //    )
            //);
            //JobPayload job = new JobPayload(jobInput, jobOutput);
            //dynamic response = derivativesApi.Translate(job, true);
            //return (response);

            string Url = "https://developer.api.autodesk.com/modelderivative/v2/designdata/job";
            string jsonData = "{\"input\": {\"urn\": \"" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(urn)) + "\"},\"output\": {\"formats\": [{\"type\": \"svf\", \"views\": [\"2d\", \"3d\"]}]}}";
            dynamic response = "";
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Authorization", "Bearer " + AccessToken);
                response = client.UploadString(Url, jsonData);
            }
            return response;
        }

        private string verifyJobComplete(string base64Urn)
        {
            while (true)
            {
                //    dynamic response = derivativesApi.GetManifest(base64Urn);
                //    if (hasOwnProperty(response, "progress") && response.progress == "complete")
                //    {
                //        return (response);
                //    }
                //    else
                //    {
                //        Thread.Sleep(1000);
                //    }
                //}
                string response = "";
                string Url = "https://developer.api.autodesk.com/modelderivative/v2/designdata/" + base64Urn + "/manifest";

                using (var client = new WebClient())
                {
                    client.Headers.Add("Authorization", "Bearer " + AccessToken);
                    response = client.DownloadString(Url);
                }
                var obj = JObject.Parse(response);
                string progress = (string) obj["progress"];
                if (progress == "progress")
                {
                    Thread.Sleep(1000);
                    verifyJobComplete(base64Urn);

                }
                else
                {
                    return (progress);
                }
            }

        }

        public static bool hasOwnProperty(dynamic obj, string name)
        {
            try
            {
                var test = obj[name];
                return (true);
            }
            catch (Exception)
            {
                return (false);
            }
        }

        //<<------End of Load File Methods ------------>>




        public JsonResult GetFiles([DataSourceRequest] DataSourceRequest request)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];

                IEnumerable<SP_GetFolderFiles_Result> items = db.Database
                    .SqlQuery<SP_GetFolderFiles_Result>("SP_GetFolderFiles @compid={0},@userid={1},@groupid={2}",
                        user.Comp_ID, user.U_Id, user.G_Id).ToList().Select(c => new SP_GetFolderFiles_Result
                        {
                            id = c.id,
                            NodeName = c.NodeName
                        });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult GetVersion([DataSourceRequest] DataSourceRequest request, string filename)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            if (filename.Trim() != "" && filename.Trim() != "Select File")
            {
                IEnumerable<SP_Getversion_Result> items = db.Database
                    .SqlQuery<SP_Getversion_Result>("SP_Getversion @file={0},@CompId={1}", filename, user.Comp_ID)
                    .ToList().Select(c => new SP_Getversion_Result
                    {
                        Urn = c.Urn,
                        version = c.version
                    });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCategoryName(string urn)
        {
            if (urn != "")
            {

                IEnumerable<GetCategoryName_Result> items = db.Database
                    .SqlQuery<GetCategoryName_Result>("GetCategoryName @Guid={0}", urn).ToList().Select(
                        c => new GetCategoryName_Result
                        {

                            CateCount = c.CateCount,
                            Category_Name = c.Category_Name
                        });
                return Json(items, JsonRequestBehavior.AllowGet);

            }
            else
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPropertyName(string urn, string category)
        {
            if (urn != "")
            {

                IEnumerable<GetPropertyName_Result> items = db.Database
                    .SqlQuery<GetPropertyName_Result>("GetPropertyName @Guid={0},@category={1}", urn, category).ToList()
                    .Select(c => new GetPropertyName_Result
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

        public JsonResult GetVersionList([DataSourceRequest] DataSourceRequest request, string urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }
            IEnumerable<SP_GetVersionList_Result> items = db.Database
                .SqlQuery<SP_GetVersionList_Result>("SP_GetVersionList @urn={0},@compid={1}", urn, user.Comp_ID)
                .ToList().Select(c => new SP_GetVersionList_Result
                {
                    Id = c.Id,
                    Versionname = c.Versionname
                });
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveFeedback(string username, string feedback)

        {
            String Result = "Your Response Recorded Successfully....";
            List<SqlParameter> parameters = new List<SqlParameter>();
            UserInfo user = new UserInfo();
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }
            else
            {
                throw new Exception("session expired");
            }

            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Username",
                SqlDbType = SqlDbType.VarChar,
                Value = username,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@U_Id",
                SqlDbType = SqlDbType.VarChar,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters.Add(new SqlParameter()
            {
                ParameterName = "@Suggestion",
                SqlDbType = SqlDbType.VarChar,
                Value = feedback,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet EmailTable = SqlManager.ExecuteDataSet("SP_GetEmailData", parameters.ToArray());
            DataTable dataTableSetMailConfiguration = EmailTable.Tables[0];
            DataTable dataTableSuggestion = EmailTable.Tables[1];

            string tempFromAddress = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailFromAddress")["Value"].ToString();
            string tempFromPassword = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailFromPassword")["Value"].ToString();
            string tempMailHostName = dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailHostName")["Value"].ToString();
            int tempMailPortNumber = int.Parse(dataTableSetMailConfiguration.AsEnumerable()
                .First(r => r.Field<string>("KeyName") == "MailPortNumber")["Value"].ToString());
            //string tempMailToAddress = dataTableSetMailConfiguration.AsEnumerable().First(r => r.Field<string>("KeyName") == "MailToAddress")["Value"].ToString();


            string Body = EmailTable.Tables[1].Rows[0]["BODY"].ToString();
            string Subject = EmailTable.Tables[1].Rows[0]["SUBJECT"].ToString();
            string tempMailToAddress = EmailTable.Tables[1].Rows[0]["EmailTo"].ToString();

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


        public ActionResult DownloadFile(string Filename)
        {
            //string file = @"E:\SanveoInspire\SanveoAIO\SanveoAIO\UploadFiles\" + Filename;
            string file = Server.MapPath("~/UploadFiles/") + Filename;
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Filename);
        }

        public JsonResult GetFileUploadData_LatestVersion()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeUrnDetails", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                for (int j = 0; j < datatable.Rows.Count; j++)
                {
                    string foldername = dataSet2.Tables[0].Rows[j]["FolderName"].ToString();
                    string filename = dataSet2.Tables[0].Rows[j]["FileName"].ToString();
                    string fileurn = dataSet2.Tables[0].Rows[j]["Urn"].ToString();
                    string version = dataSet2.Tables[0].Rows[j]["VersionNo"].ToString();
                    string ItemId = dataSet2.Tables[0].Rows[j]["ItemId"].ToString();
                    string ProjectId = dataSet2.Tables[0].Rows[j]["ProjectId"].ToString();
                    DateTime? modified = null;
                    if (dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString() != "")
                        modified = Convert.ToDateTime(dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString());

                    //* For cross check to Dextract or not
                    string errormessage = "";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@FileName",
                        SqlDbType = SqlDbType.VarChar,
                        Value = filename,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@Urn",
                        SqlDbType = SqlDbType.VarChar,
                        Value = fileurn,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@VersionNo",
                        SqlDbType = SqlDbType.VarChar,
                        Value = version,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@ModifiedDate",
                        SqlDbType = SqlDbType.DateTime,
                        Value = modified,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@compid",
                        SqlDbType = SqlDbType.Int,
                        Value = user.Comp_ID,
                        Direction = System.Data.ParameterDirection.Input
                    });
                    SqlParameter message = new SqlParameter()
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.VarChar,
                        Size = 1000,
                        Direction = System.Data.ParameterDirection.Output
                    };
                    parameters.Add(message);
                    DataSet dataSet =
                        SqlManager.ExecuteDataSet("SP_SaveModelUrnDetailsCrossCheck", parameters.ToArray());
                    errormessage = message.Value.ToString();
                    if (filename != "")
                    {
                        if (errormessage == "New File")
                        {
                            nodedata += fileurn + "," + filename + " <span style = 'color:skyblue'>(V" + version +
                                        ")</span>" +
                                        " <span style='font-size:15px;color:red' class='fa fa-exclamation-circle' id='" +
                                        fileurn + "'></span>" + "," + foldername + "," + version + "," + filename + "|";
                        }
                        else
                        {
                            nodedata += fileurn + "," + filename + " <span style = 'color:skyblue'>(V" + version +
                                        ")</span>" + "," + foldername + "," + version + "," + filename + "|";

                        }
                    }


                    // }
                }
            }

            return Json(nodedata, JsonRequestBehavior.AllowGet);

        }

        #region

        public string GetSearchByCategoryNProperty(string urn, string AutoCat)
        {
            bool SuccessFlag = true;

            DataTable DetailsTable = new DataTable();

            string AutoCategory = "", AutoFamily = "", AutoPropName = "", AutoPropValue = "";
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                if (AutoCat.Contains("+"))
                {
                    string[] AutoTextArray = AutoCat.Split('+');

                    if (AutoTextArray.Length == 1)
                    {
                        AutoCategory = AutoTextArray[0];
                    }
                    else if (AutoTextArray.Length == 2)
                    {
                        AutoCategory = AutoTextArray[0];
                        AutoFamily = AutoTextArray[1];
                    }

                    else if (AutoTextArray.Length == 3)
                    {
                        AutoCategory = AutoTextArray[0];
                        AutoFamily = AutoTextArray[1];
                        AutoPropName = AutoTextArray[2];
                    }
                    else if (AutoTextArray.Length == 4)
                    {
                        AutoCategory = AutoTextArray[0];
                        AutoFamily = AutoTextArray[1];
                        AutoPropName = AutoTextArray[2];
                        AutoPropValue = AutoTextArray[3];
                    }
                }
                else
                {
                    AutoCategory = AutoCat;
                }



                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = urn,
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
                    ParameterName = "@AutoCat",
                    SqlDbType = SqlDbType.VarChar,
                    Value = AutoCategory,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoFamily",
                    SqlDbType = SqlDbType.VarChar,
                    Value = AutoFamily,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = AutoPropName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropValue",
                    SqlDbType = SqlDbType.VarChar,
                    Value = AutoPropValue,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetSearchByCategoryNProperty", parameters.ToArray());



                if (dataSet.Tables[0].Rows.Count != 0)
                {
                    DetailsTable = dataSet.Tables[0];
                }
            }

            catch (Exception ex)
            {

                SuccessFlag = false;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = SuccessFlag,
                detailsTable = DetailsTable
            });

        }

        #endregion
        public string GetAdvanceSearch(string Urn, string Categoryname, string Propertyname, string Propertyvalue, string Operator)
        {
            bool SuccessFlag = true;

            DataTable DetailsTable = new DataTable();
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Urn,
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
                    ParameterName = "@AutoCat",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Categoryname,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Operator",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Operator,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Propertyname,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropValue",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Propertyvalue,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdvanceSearch", parameters.ToArray());



                if (dataSet.Tables[0].Rows.Count != 0)
                {
                    DetailsTable = dataSet.Tables[0];
                }
            }

            catch (Exception ex)
            {
                SuccessFlag = false;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = SuccessFlag,
                detailsTable = DetailsTable
            });
        }

        public string GetAdvanceSearch2(string Urn, string Categoryname, string Familyname, string Propertyname, string Propertyvalue, string Operator)
        {
            bool SuccessFlag = true;

            DataTable DetailsTable = new DataTable();
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Urn,
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
                    ParameterName = "@familyname",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Familyname,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoCat",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Categoryname,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Operator",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Operator,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Propertyname,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@AutoPropValue",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Propertyvalue,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdvanceSearchtab3", parameters.ToArray());



                if (dataSet.Tables[0].Rows.Count != 0)
                {
                    DetailsTable = dataSet.Tables[0];
                }
            }

            catch (Exception ex)
            {
                SuccessFlag = false;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = SuccessFlag,
                detailsTable = DetailsTable
            });
        }

        public JsonResult GetLatestPersoanl()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetPersonalActivePrj", parameters2.ToArray());

            if (dataSet2.Tables[0].Rows.Count > 0)
            {
                ActiveProject = dataSet2.Tables[0].Rows[0]["projectName"].ToString();

                initializeOAuth();
                DateTime? lastmodifiedDate = null;
                string date1 = "01/11/2015";
                DateTime? Lastdate = Convert.ToDateTime(date1);
                ObjectsApi objects = new ObjectsApi();
                objects.Configuration.AccessToken = twoLeggedCredentials.access_token;
                var objectsList = objectsApi.GetObjects(ActiveProject, 100);

                foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                {
                    string urn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(((string) objInfo.Value.objectId)));

                    if (urn.Contains("=="))
                    {
                        urn = urn.Substring(0, urn.Length - 2);
                    }
                    if (urn.Contains("="))
                    {
                        urn = urn.Substring(0, urn.Length - 1);
                    }
                    string filename = (string) objInfo.Value.objectKey;
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

                List<SqlParameter> parameters3 = new List<SqlParameter>();
                parameters3.Add(new SqlParameter()
                {
                    ParameterName = "@Compid",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet3 = SqlManager.ExecuteDataSet("SP_SyncPersonalFile", parameters3.ToArray());
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetLatestEnterPrise()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
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
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@userid",
                SqlDbType = SqlDbType.Int,
                Value = user.U_Id,
                Direction = System.Data.ParameterDirection.Input
            });
            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_GetNodeUrnDetails", parameters1.ToArray());
            DataTable datatable = dataSet2.Tables[0];
            if (datatable.Rows.Count > 0)
            {
                for (int j = 0; j < datatable.Rows.Count; j++)
                {
                    string foldername = dataSet2.Tables[0].Rows[j]["FolderName"].ToString();
                    string filename = dataSet2.Tables[0].Rows[j]["FileName"].ToString();
                    string fileurn = dataSet2.Tables[0].Rows[j]["Urn"].ToString();
                    string version = dataSet2.Tables[0].Rows[j]["VersionNo"].ToString();
                    string ItemId = dataSet2.Tables[0].Rows[j]["ItemId"].ToString();
                    string ProjectId = dataSet2.Tables[0].Rows[j]["ProjectId"].ToString();
                    DateTime? modified = null;
                    if (dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString() != "")
                        modified = Convert.ToDateTime(dataSet2.Tables[0].Rows[j]["ModifiedDate"].ToString());

                    //* For Version auto tracking *//
                    if (ItemId != "")
                    {
                        string projid = "";
                        string projname = "";
                        string versionno = "";
                        string lastModifiedTime = "";
                        string storesize = "";
                        dynamic AllContents = itemsApi.GetItemVersions(ProjectId, ItemId);
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
                                                                        if (item6.Previous.ToString()
                                                                                .Contains("derivatives") == true)
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
                                            {
                                            }
                                        }

                                        //*for version save *//
                                        var finaldata = db.SP_SaveModelUrnDetails(projname, projid, versionno,
                                            Convert.ToDateTime(lastModifiedTime), storesize, user.Comp_ID, ItemId, 0,
                                            user.U_Id);

                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Json(System.Convert.ToBase64String(plainTextBytes), JsonRequestBehavior.AllowGet);
        }

    }

}