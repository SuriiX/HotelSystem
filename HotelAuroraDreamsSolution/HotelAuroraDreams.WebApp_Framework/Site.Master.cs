// File: Site.Master.cs
using System;
using System.Collections.Generic; // Para IList<string>
using System.Linq; // Para .Contains()
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HotelAuroraDreams.WebApp_Framework
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.Cookies["AuthTokenHotel"] != null && !string.IsNullOrEmpty(Request.Cookies["AuthTokenHotel"].Value) && Session["UserFullName"] != null)
                {
                    phLogin.Visible = false;
                    phUserInfo.Visible = true;
                    lblUserFullNameMaster.Text = $"Hola, {Session["UserFullName"]}";

                    var userRoles = Session["UserRoles"] as IList<string>;
                    if (userRoles != null && userRoles.Contains("Administrador"))
                    {
                        phAdminMenu.Visible = true;
                    }
                    else
                    {
                        phAdminMenu.Visible = false;
                    }
                }
                else
                {
                    phLogin.Visible = true;
                    phUserInfo.Visible = false;
                    phAdminMenu.Visible = false;
                }
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            if (Request.Cookies["AuthTokenHotel"] != null)
            {
                HttpCookie cookie = new HttpCookie("AuthTokenHotel");
                cookie.Expires = DateTime.Now.AddDays(-1d); // Hacer que expire
                Response.Cookies.Add(cookie);
            }

            Response.Redirect("~/Login.aspx");
        }
    }
}