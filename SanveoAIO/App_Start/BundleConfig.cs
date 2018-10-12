﻿using System.Web;
using System.Web.Optimization;

namespace SanveoAIO
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
                    
            //          "~/Scripts/respond.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //             "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                 "~/Scripts/bootstrap.js",
                 "~/Content/jqTree-master/tree.jquery.js",
                 "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                                "~/Content/bootstrap.css",
                                "~/Content/jqTree-master/jqtree.css",
                                "~/Content/font-awesome.min.css",
                                "~/Content/jqTree-master/static/bower_components/fontawesome/css/font-awesome.min.css",
                                "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/Content/JavaScript.js").Include(
                 "~/Content/JS/RoomData.js",
                 "~/Content/JS/RunShrader.js",
                 "~/Content/JS/RoomWiseQuantity.js",
                  "~/Content/JS/ADAClearenceCheck.js",
                  "~/Content/JS/DextractAndUpload.js"
                  ));

            ScriptBundle thirdPartyScripts = new ScriptBundle("~/Scripts/ThirdParty");
            thirdPartyScripts.Include("~/Scripts/jquery-{version}.js",
                                "~/Scripts/bootstrap.min.js");

            bundles.Add(thirdPartyScripts);
            BundleTable.EnableOptimizations = true;









        }
    }
}
