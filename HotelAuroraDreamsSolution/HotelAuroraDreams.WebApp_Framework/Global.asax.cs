// File: Global.asax.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;    
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI; 

namespace HotelAuroraDreams.WebApp_Framework
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles); 
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {

                    Path = "~/Scripts/jquery-3.7.1.min.js",
                    DebugPath = "~/Scripts/jquery-3.7.1.js",

                });

        }

    }
}