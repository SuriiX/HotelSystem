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
                bool isEmployee = false; // O cualquier rol de empleado general

                if (Request.Cookies["AuthTokenHotel"] != null &&
                    !string.IsNullOrEmpty(Request.Cookies["AuthTokenHotel"].Value) &&
                    Session["UserFullName"] != null)
                {
                    phLogin.Visible = false;
                    phUserInfo.Visible = true;
                    lblUserFullNameMaster.Text = $"Hola, {Session["UserFullName"]}";

                    var userRoles = Session["UserRoles"] as IList<string>;
                    if (userRoles != null)
                    {
                        if (userRoles.Contains("Administrador"))
                        {
                            isAdmin = true;
                        }
                        if (userRoles.Contains("Empleado")) // Asumiendo que "Empleado" es otro rol
                        {
                            isEmployee = true;
                        }
                    }
                }
                else
                {
                    phLogin.Visible = true;
                    phUserInfo.Visible = false;
                }

                // Visibilidad de menús basados en roles
                if (isAdmin)
                {
                    phAdminMenu.Visible = true;
                    phOperationsMenu.Visible = true; // Los administradores también ven operaciones
                }
                else if (isEmployee) // Si es empleado pero no admin
                {
                    phAdminMenu.Visible = false; // Ocultar menú de administración estricta
                    phOperationsMenu.Visible = true; // Mostrar menú de operaciones
                }
                else // No logueado o sin rol reconocido
                {
                    phAdminMenu.Visible = false;
                    phOperationsMenu.Visible = false;
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
            // Opcional: Invalidar Forms Authentication si se usa
            // System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("~/Login.aspx");
        }
    }
}