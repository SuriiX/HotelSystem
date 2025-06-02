
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
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/jquery-3.7.1.min.js",
                    DebugPath = "~/Scripts/jquery-3.7.1.js",  
                    CdnPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.1.min.js",
                    CdnSupportsSecureConnection = true,
                    LoadSuccessExpression = "window.jQuery"
                });
        }

        void Application_End(object sender, EventArgs e)
        {
        }

        void Application_Error(object sender, EventArgs e)
        {
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {
        }
    }
}