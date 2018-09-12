using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SanveoAIO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using telmvc;

namespace SanveoAIO.Controllers
{
    public class PropertyMapController : Controller
    {
        // GET: PropertyMap
        SanveoInspireEntities db = new SanveoInspireEntities();

        UserInfo user;

        public ActionResult PropertyMap()
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
                return View();
            }
            else
            {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
                return RedirectToAction("Login", "Login");
            }
        }

        public JsonResult GetPropertyMapName(string urn, string category)
        {
            if (urn != "")
            {

                IEnumerable<GetPropertyMapName_Result> items = db.Database
                    .SqlQuery<GetPropertyMapName_Result>("GetPropertyMapName @Guid={0},@category={1}", urn, category)
                    .ToList().Select(c => new GetPropertyMapName_Result
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

        public JsonResult GetPropertyvalues(string urn, string category, string PropertyName)
        {
            if (urn != "")
            {

                IEnumerable<SP_GetPropertyNameValue_Result> items = db.Database
                    .SqlQuery<SP_GetPropertyNameValue_Result>(
                        "SP_GetPropertyNameValue @Guid={0},@category={1},@PropertyName={2}", urn, category,
                        PropertyName).ToList().Select(c => new SP_GetPropertyNameValue_Result
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

        public JsonResult UpdatePropertyValue(string URN)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@URN",
                SqlDbType = SqlDbType.VarChar,
                Value = URN,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CategoryName",
                SqlDbType = SqlDbType.VarChar,
                Value = "",
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyName",
                SqlDbType = SqlDbType.VarChar,
                Value = "",
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyValue",
                SqlDbType = SqlDbType.VarChar,
                Value = "",
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_UpdatePropertyValue", parameters1.ToArray());

            return Json("", "text/plain");
        }

        public JsonResult UpdateTextPropertyValue(string URN, string CategoryName, string PropertyName, string PropertyValue)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@URN",
                SqlDbType = SqlDbType.VarChar,
                Value = URN,
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
                ParameterName = "@PropertyValue",
                SqlDbType = SqlDbType.VarChar,
                Value = PropertyValue,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_UpdateTextPropertyValue", parameters1.ToArray());

            return Json("", "text/plain");
        }

        public ActionResult GetPropertyMap([DataSourceRequest] DataSourceRequest request, string Urn)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            var result = db.Database.SqlQuery<SP_GetPropertyMapDetails_Result>("SP_GetPropertyMapDetails @Compid={0},@Urn={1}", user.Comp_ID, Urn).ToList();
            DataSourceResult result1 = result.ToDataSourceResult(request);
            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SavePropertyMap(string Id, string CategoryName, string PropertyFrom, string PropertyTo,string Urn)
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
                ParameterName = "@PropertyForm",
                SqlDbType = SqlDbType.VarChar,
                Value = PropertyFrom,
                Direction = System.Data.ParameterDirection.Input
            });
            parameters1.Add(new SqlParameter()
            {
                ParameterName = "@PropertyTo",
                SqlDbType = SqlDbType.VarChar,
                Value = PropertyTo,
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
            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_SavePropertyMapDetails", parameters1.ToArray());

            return Json("", "text/plain");
         
        }

        public JsonResult DeletePropertyMap(string Id)
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
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.Int,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet dtRuleData = SqlManager.ExecuteDataSet("SP_DeletePropertyMapDetails", parameters1.ToArray());

            return Json("", "text/plain");
        }
    }
}