using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
                bool isAuthenticated = false;
                bool isAdmin = false;
                bool isEmployee = false;

                if (Request.Cookies["AuthTokenHotel"] != null &&
                    !string.IsNullOrEmpty(Request.Cookies["AuthTokenHotel"].Value) &&
                    Session["UserFullName"] != null)
                {
                    isAuthenticated = true;
                    if (lblUserFullNameMaster != null)
                    {
                        lblUserFullNameMaster.Text = $"Hola, {Session["UserFullName"]}";
                    }

                    var userRoles = Session["UserRoles"] as IList<string>;
                    if (userRoles != null)
                    {
                        if (userRoles.Contains("Administrador"))
                        {
                            isAdmin = true;
                        }
                        if (userRoles.Contains("Empleado") || isAdmin)
                        {
                            isEmployee = true;
                        }
                    }
                }

                if (isAuthenticated)
                {
                    if (phLogin != null) phLogin.Visible = false;
                    if (phUserInfo != null) phUserInfo.Visible = true;

                    if (isAdmin)
                    {
                        if (phAdminMenu != null) phAdminMenu.Visible = true;
                        if (phOperationsMenu != null) phOperationsMenu.Visible = true;
                    }
                    else if (isEmployee)
                    {
                        if (phAdminMenu != null) phAdminMenu.Visible = false;
                        if (phOperationsMenu != null) phOperationsMenu.Visible = true;
                    }
                    else
                    {
                        if (phAdminMenu != null) phAdminMenu.Visible = false;
                        if (phOperationsMenu != null) phOperationsMenu.Visible = false;
                    }
                }
                else
                {
                    if (phLogin != null) phLogin.Visible = true;
                    if (phUserInfo != null) phUserInfo.Visible = false;
                    if (phAdminMenu != null) phAdminMenu.Visible = false;
                    if (phOperationsMenu != null) phOperationsMenu.Visible = false;
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
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);
            }

            Response.Redirect("~/Login.aspx", true);
        }
    }
}