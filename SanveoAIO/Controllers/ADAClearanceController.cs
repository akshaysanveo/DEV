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
    public class ADAClearanceController : Controller
    {
        // GET: ADAClearance
        UserInfo user;
        SanveoInspireEntities db = new SanveoInspireEntities();

        public ActionResult ADAClearance()
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];

                return View();
            }
            else
                return RedirectToAction("Login", "Login");
        }


        public string GetADAClearanceSlopeSearch(string Urn, string Categoryname)
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


                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdaclearanceForSlope", parameters.ToArray());

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

        public string GetForgeIds(string Urn, string Categoryname)
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
                    ParameterName = "@Categoryname",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Categoryname,
                    Direction = System.Data.ParameterDirection.Input
                });


                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetForgeIdsForSlope", parameters.ToArray());

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


        public JsonResult GetADACategory(string Urn, string VersionNo)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                IEnumerable<SP_GetCategory_Result> items1 = db.Database.SqlQuery<SP_GetCategory_Result>("EXEC SP_GetCategory @urn={0},@version={1},@CompId={2}", Urn, VersionNo, user.Comp_ID).ToList().Select(c => new SP_GetCategory_Result
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

        public JsonResult GetADAProperty(string Urn, string CategoryName)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                IEnumerable<GetPropertyName_Result> items1 = db.Database.SqlQuery<GetPropertyName_Result>("EXEC GetPropertyName @Guid={0},@category={1}", Urn, CategoryName).ToList().Select(c => new GetPropertyName_Result
                {
                    Id = c.Id,
                    Property_Name = c.Property_Name
                });
                return Json(items1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        public string GetADAClearanceGeometrySearch(string Urn, string Categoryname, string Propertyname, string Propertyvalue, string Operator)
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


                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdaclearanceGeometry", parameters.ToArray());

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

        public string GetADAClearanceSearch(string Urn, string Categoryname, string Propertyname, string Propertyvalue, string Operator)
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

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdaclearance", parameters.ToArray());



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

        public JsonResult SaveSlopePoints(string Urn, string VersionNo, string FloorDataStore)
        {
            if (Session["UserInfo"] != null)
            {
                user = (UserInfo) Session["UserInfo"];
            }


            // For Middle Co-ordiante

            FloorDataStore = FloorDataStore.Substring(0, FloorDataStore.Length - 1);

            var ffMiddle = FloorDataStore.Split('$');

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


            DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveSlopeBoundingPoints", parameters1.ToArray());


            return Json("", "text/plain");
        }



        public JsonResult Getslope(string Slopes, string Forgeid)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                IEnumerable<SP_GetCategory_Result> items1 = db.Database.SqlQuery<SP_GetCategory_Result>("EXEC SP_GetCategory @urn={0},@version={1},@CompId={2}", user.Comp_ID).ToList().Select(c => new SP_GetCategory_Result
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

        public JsonResult SaveForgeIdSlopes(string Urn, string Bulkdata)
        {
            try
            {
                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo) Session["UserInfo"];
                }

                DataTable dt = new DataTable();
                DataColumn DC = new DataColumn("ForgeId", typeof(string));
                dt.Columns.Add(DC);
                DC = new DataColumn("Slopes", typeof(string));
                dt.Columns.Add(DC);
                DC = new DataColumn("SurfaceId", typeof(string));
                dt.Columns.Add(DC);
                DC = new DataColumn("PointCordinates", typeof(string));
                dt.Columns.Add(DC);

                string[] CompleteString = Bulkdata.Split('!');
                string[] ForgeId = new string[CompleteString.Length];
                string[] Slopes;
                string[] SurfaceId;
                string[] PointCordinates;
                for (int i = 0; i < CompleteString.Length; i++)
                {
                    string[] srtMix = CompleteString[i].Split('|');
                    ForgeId[i] = srtMix[0];
                    Slopes = srtMix[1].Split('#');
                    SurfaceId = srtMix[2].Split('#');
                    PointCordinates = srtMix[3].Split('#');
                    for (int j = 0; j < SurfaceId.Length; j++)
                    {
                        DataRow dr = dt.NewRow();
                        dr[0] = ForgeId[i];
                        dr[1] = Slopes[j];
                        dr[2] = SurfaceId[j];
                        dr[3] = PointCordinates[j];
                        dt.Rows.Add(dr);
                    }
                }

                List<SqlParameter> parameters1 = new List<SqlParameter>();

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Urn,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters1.Add(new SqlParameter()
                {
                    ParameterName = "@SlopeTable",
                    SqlDbType = SqlDbType.Structured,
                    Value = dt,
                    Direction = System.Data.ParameterDirection.Input
                });


                DataSet dataSet2 = SqlManager.ExecuteDataSet("SP_SaveForgeIdSlopes", parameters1.ToArray());

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }



            return Json("", "text/plain");

        }

        public string GetPointCoordinates(string SurfaceId)
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
                    ParameterName = "@SurfaceId",
                    SqlDbType = SqlDbType.VarChar,
                    Value = SurfaceId,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_GetAdaclrPointCoordinates", parameters.ToArray());



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

    }
}