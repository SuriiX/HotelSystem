// File: Site.Master.cs
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
                bool isAdmin = false;
                bool isEmployee = false;

                if (Request.Cookies["AuthTokenHotel"] != null &&
                    !string.IsNullOrEmpty(Request.Cookies["AuthTokenHotel"].Value) &&
                    Session["UserFullName"] != null)
                {
                    phLogin.Visible = false;
                    phUserInfo.Visible = true;
                    if (lblUserFullNameMaster != null) // Verificar si el control existe
                    {
                        lblUserFullNameMaster.Text = $"Hola, {Session["UserFullName"]}";
                    }


                    var userRoles = Session["UserRoles"] as IList<string>;
                    if (userRoles != null)
                    {
                        if (userRoles.Contains("Administrador")) isAdmin = true;
                        if (userRoles.Contains("Empleado")) isEmployee = true;
                    }
                }
                else
                {
                    phLogin.Visible = true;
                    phUserInfo.Visible = false;
                }

                // Visibilidad de menús
                // Los PlaceHolders deben existir en el .master para que estas líneas no den error.
                if (phAdminMenu != null) phAdminMenu.Visible = false;
                if (phOperationsMenu != null) phOperationsMenu.Visible = false;

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