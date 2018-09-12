using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SanveoAIO.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using telmvc;

namespace SanveoAIO.Controllers
{
    public class COBIEExlController : Controller
    {
        // GET: COBIEExl
        UserInfo user;
        SanveoInspireEntities db = new SanveoInspireEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult COBIEExl()
        {
            return View();
        }


        #region Get Distinct Property
        public string GetProperty(string urn, string version,string TradeId)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }


                //IEnumerable<SP_GetDistinctProperty1_Result> items1 = db.Database.SqlQuery<SP_GetDistinctProperty1_Result>("EXEC SP_GetDistinctProperty @urn={0},@version={1},@CompId={2},@TradeId={3}", urn, version, user.Comp_ID,TradeId).ToList().Select(c => new SP_GetDistinctProperty1_Result
                //{
                //    Property_Name = c.Property_Name
                //});
                //return Json(items1, JsonRequestBehavior.AllowGet);

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

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@TradeId",
                    SqlDbType = SqlDbType.Int,
                    Value = TradeId,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet ds = SqlManager.ExecuteDataSet("SP_GetDistinctProperty", parameters.ToArray());
                DataTable dt = ds.Tables[0];
                string data = JsonConvert.SerializeObject(dt);
                return data;
            }
            catch (Exception ex)
            {
                // return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
                return ex.Message;
            }
        }

        #endregion
        #region GetCheckCategoryPropertyForexcel

        public JsonResult GetCheckCategoryPropertyForexcel(string FileName, string Category, string Property,string Version)
        {
            StringBuilder tabledata =new StringBuilder("");

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
                    Value = FileName,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CategoryName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Category,
                    Direction = System.Data.ParameterDirection.Input
                });


                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@PropertyName",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Property,
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

                DataSet ds = SqlManager.ExecuteDataSet("GetExcelByCatG&PropT", parameters.ToArray());

                DataTable dt = ds.Tables[0];
                int colSpan = dt.Columns.Count;
                string catOld = "";
                string catNew = "";

                tabledata.Append("<table border='2' id='tblCatPropExl' style='font-size:15px'>");
                tabledata.Append("<thead>");
                tabledata.Append("<tr>");


                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    tabledata.Append("<th style='border:1px solid;'>" + dt.Columns[i].ColumnName + "</th>");
                }

                tabledata.Append("</tr>");
                tabledata.Append("</thead>");
                tabledata.Append("<tbody>");

                for (int i = 0; i < dt.Rows.Count; i++)
                {



                    catNew = dt.Rows[i][0].ToString();

                    if (catNew != catOld)
                    {
                        tabledata.Append("<tr>");
                        tabledata.Append("<td style='border:1px solid;font-weight:bold;' colspan=" + colSpan + ">" + dt.Rows[i][0].ToString() + "</td></tr>");
                        tabledata.Append("<tr><td style='border:1px solid ;'></td>");
                        for (int j = 1; j < dt.Columns.Count; j++)
                        {
                            tabledata.Append("<td style='border:1px solid ;'>" + dt.Rows[i][j].ToString() + "</td>");
                        }
                        catOld = dt.Rows[i][0].ToString();
                        tabledata.Append("</tr>");
                    }

                    else
                    {
                        tabledata.Append("<tr><td style='border:1px solid ;'></td>");
                        for (int j = 1; j < dt.Columns.Count; j++)
                        {

                            tabledata.Append("<td style='border:1px solid ;'>" + dt.Rows[i][j].ToString() + "</td>");
                        }
                        catOld = dt.Rows[i][0].ToString();
                        tabledata.Append("</tr>");
                    }


                }




                tabledata.Append("</tbody>");
                tabledata.Append("</table>");

                var jsonResult = Json(tabledata.ToString(), "text/plain");
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
                //return Json(tabledata, "text/plain");


            }




            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
                // return ex.Message;
            }

        }
        #endregion



        #region SaveCOBIEExlData
        public JsonResult SaveCOBIEExlData(string Urn, string VersionNo, string Property, string Category, int TradeId)
        {

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
                    ParameterName = "@version",
                    SqlDbType = SqlDbType.VarChar,
                    Value = VersionNo,
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
                    ParameterName = "@Property",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Property,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@TradeId",
                    SqlDbType = SqlDbType.Int,
                    Value = TradeId,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "Category",
                    SqlDbType = SqlDbType.VarChar,
                    Value = @Category,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet ds = SqlManager.ExecuteDataSet("SP_SaveCOBIEExlData", parameters.ToArray());
                return Json("", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        #region GetCheckCOBIEExlCategory
        public string GetCheckCOBIEExlCategory(string Urn, string VersionNo, int TradeId)
        {
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
                    ParameterName = "@version",
                    SqlDbType = SqlDbType.VarChar,
                    Value = VersionNo,
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
                    ParameterName = "@TradeId",
                    SqlDbType = SqlDbType.Int,
                    Value = TradeId,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet ds = SqlManager.ExecuteDataSet("SP_GetCOBIEExlCategory", parameters.ToArray());
                string data = JsonConvert.SerializeObject(ds);
                return data;

            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion

        #region GetCheckCOBIEExlProperty
        public string GetCheckCOBIEExlProperty(string Urn, string VersionNo, int TradeId)
        {
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
                    ParameterName = "@version",
                    SqlDbType = SqlDbType.VarChar,
                    Value = VersionNo,
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
                    ParameterName = "@TradeId",
                    SqlDbType = SqlDbType.Int,
                    Value = TradeId,
                    Direction = System.Data.ParameterDirection.Input
                });
                DataSet ds = SqlManager.ExecuteDataSet("SP_GetCOBIEExlProperty", parameters.ToArray());
                DataTable dt = ds.Tables[0];
                string data = JsonConvert.SerializeObject(dt);
                return data;

            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        #endregion

        #region GetTrades

        public JsonResult GetTradeTypes([DataSourceRequest] DataSourceRequest request)
        {

            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];

                IEnumerable<SP_GetTradeType_Result> items = db.Database.SqlQuery<SP_GetTradeType_Result>("SP_GetTradeType @compid={0}", user.Comp_ID).ToList().Select(c => new SP_GetTradeType_Result
                {
                    id = c.id,
                    Name = c.Name
                });
                return Json(items, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }


        }
        #endregion
    }
}
