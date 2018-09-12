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
    public class VersionTagController : Controller
    {
        // GET: VersionTag
        public ActionResult VersionTag()
        {
            return View();
        }

        UserInfo user;

        SanveoInspireEntities db = new SanveoInspireEntities();

        public string GetVersionTagging(string FileName)
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
                ParameterName = "@CompId",
                SqlDbType = SqlDbType.VarChar,
                Value = user.Comp_ID,
                Direction = System.Data.ParameterDirection.Input
            });

            DataSet ds = SqlManager.ExecuteDataSet("SP_GetVersionwiseTag", parameters.ToArray());
            DataTable dt = ds.Tables[0];
            string data = JsonConvert.SerializeObject(dt);
            return data;
        }


        public JsonResult SaveVersionType(string FileName, string Urn, int versionNo, string verText)
        {
            string result = "Error Occurred While Saving data";
            try
            {

                if (Session["UserInfo"] != null)
                {
                    user = (UserInfo)Session["UserInfo"];
                }
                else
                {
                    throw new Exception("session expired");
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
                    ParameterName = "@Urn",
                    SqlDbType = SqlDbType.VarChar,
                    Value = Urn,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@Version",
                    SqlDbType = SqlDbType.Int,
                    Value = versionNo,
                    Direction = System.Data.ParameterDirection.Input
                });
                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@VersionTag",
                    SqlDbType = SqlDbType.VarChar,
                    Value = verText,
                    Direction = System.Data.ParameterDirection.Input
                });

                parameters.Add(new SqlParameter()
                {
                    ParameterName = "@CompId",
                    SqlDbType = SqlDbType.Int,
                    Value = user.Comp_ID,
                    Direction = System.Data.ParameterDirection.Input
                });

                DataSet dataSet = SqlManager.ExecuteDataSet("SP_AddUpdateVersionType", parameters.ToArray());
                result = "Data Saved Successfully";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(msg, JsonRequestBehavior.AllowGet);


            }

        }
    }
}