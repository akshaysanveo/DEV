using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SanveoAIO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using telmvc;

namespace SanveoAIO.Controllers
{
    public class NewDocController : Controller
    {
        // GET: NewDoc
        SanveoInspireEntities db = new SanveoInspireEntities();
        UserInfo user;
        public ActionResult Index()
        {
            return View();
        }

        #region GetAllForgeIdByCategory
        public string GetAllForgeIdByCategory(string urn, string version, string PropertyName)
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
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = urn,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@PropName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = PropertyName,
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
                DataSet ds = SqlManager.ExecuteDataSet("SP_GetAllForgeIdByCategory", parameters.ToArray());
                string data = JsonConvert.SerializeObject(ds);
                return data;

            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion

        public ActionResult NewDoc()
        {
            return View();
        }

        public ActionResult GetCategoryNew([DataSourceRequest] DataSourceRequest request, string Urn, string Version, string PropertyName)
        {


            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }
                var result = db.Database.SqlQuery<SP_GetCategoryNew_Result>("SP_GetCategoryNew @urn={0},@version={1},@CompId={2},@PropertyName={3}", Urn, Version, user.Comp_ID, PropertyName).ToList();
                DataSourceResult result1 = result.ToDataSourceResult(request);
                return Json(result1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }

        }


        #region SaveCatPropVisisbility
        public JsonResult SaveCatPropVisisbility(string Urn, string Version, string PropName)
        {

            try
            {
                string data = "";
                // var Error = new ObjectParameter("Error", typeof(string));

                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
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
                    ParameterName = "@version",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Version,
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
                    ParameterName = "@PropName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = PropName,
                    Direction = System.Data.ParameterDirection.Input
                });

                SqlParameter error = new SqlParameter()
                {
                    ParameterName = "@Error",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 1000,
                    Direction = System.Data.ParameterDirection.Output
                };

                parameters.Add(error);


                DataSet ds = SqlManager.ExecuteDataSet("SP_SaveCatPropVisisbility", parameters.ToArray());
                data = error.Value.ToString();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        #region UpdateVisibility

        public JsonResult UpdateVisibility(string Visible_ID, string Category_Name, string Property_Name, string Property_Value, string Visibility)
        {
            string data = "";
            int VisibilityID = Int32.Parse(Visible_ID);
            bool VisibleActive = bool.Parse(Visibility);

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            if (Visibility == "")
                Visibility = "false";



            try
            {

                var result = db.SP_UpdateVisibility(VisibilityID, Property_Value, VisibleActive);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                data = ex.Message;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        #region GetPropertyNameForVisibility
        public JsonResult GetPropertyNameForVisibility([DataSourceRequest] DataSourceRequest request, string Urn, string Version)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }

            if (Urn != "")
            {
                IEnumerable<SP_GetPropertyNameForVisibility_Result> items = db.Database.SqlQuery<SP_GetPropertyNameForVisibility_Result>("SP_GetPropertyNameForVisibility @urn={0},@version={1},@CompId={2}", Urn, Version, user.Comp_ID).ToList().Select(c => new SP_GetPropertyNameForVisibility_Result
                {

                    ID = c.ID,
                    Property_Name = c.Property_Name
                });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 

        public JsonResult DeleteVisibility(string Visible_ID)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo)Session["UserInfo"];
            }
            int VisibleID = Int32.Parse(Visible_ID);
            string data = "";

            // var Error = new ObjectParameter("Error", typeof(string));
            // var result = db.sp_DeleteUser(UserId, user.U_Id, Error);
            // data = Error.Value.ToString();
            return this.Json(new DataSourceResult
            {
                Errors = data
            });

        }

        #endregion

        #region GetAllForgeIdByCategory
        public string GetAllForgeId(string urn, string version)
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
                DataSet ds = SqlManager.ExecuteDataSet("SP_GetAllForgeId", parameters.ToArray());
                string data = JsonConvert.SerializeObject(ds);
                return data;

            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion
    }

}
