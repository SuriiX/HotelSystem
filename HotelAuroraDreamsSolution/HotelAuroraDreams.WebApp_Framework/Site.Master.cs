// File: Site.Master.cs
using System;
using System.Collections.Generic; // Para IList<string>
using System.Linq; // Para .Contains()
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

                // Verificar si el usuario está autenticado (cookie Y UserFullName en Sesión)
                if (Request.Cookies["AuthTokenHotel"] != null &&
                    !string.IsNullOrEmpty(Request.Cookies["AuthTokenHotel"].Value) &&
                    Session["UserFullName"] != null)
                {
                    isAuthenticated = true;
                    if (lblUserFullNameMaster != null) // Siempre verificar que el control exista
                    {
                        lblUserFullNameMaster.Text = $"Hola, {Session["UserFullName"]}";
                    }

                    // Obtener roles de la Sesión (deberían haberse establecido después del login)
                    var userRoles = Session["UserRoles"] as IList<string>;
                    if (userRoles != null)
                    {
                        if (userRoles.Contains("Administrador"))
                        {
                            isAdmin = true;
                        }
                        // Un Administrador también puede ser considerado Empleado para ciertos menús
                        if (userRoles.Contains("Empleado") || isAdmin)
                        {
                            isEmployee = true;
                        }
                    }
                }

                // Configurar visibilidad de placeholders de Login/Logout
                if (isAuthenticated)
                {
                    if (phLogin != null) phLogin.Visible = false;
                    if (phUserInfo != null) phUserInfo.Visible = true;
                }
                else
                {
                    if (phLogin != null) phLogin.Visible = true;
                    if (phUserInfo != null) phUserInfo.Visible = false;
                }

                // Configurar visibilidad de placeholders de Menús de Operaciones y Administración
                // Asegurarse de que los placeholders existan antes de intentar acceder a .Visible
                if (phAdminMenu != null) phAdminMenu.Visible = false;
                if (phOperationsMenu != null) phOperationsMenu.Visible = false;

                if (isAdmin) // Si es Administrador, tiene acceso a todo
                {
                    if (phAdminMenu != null) phAdminMenu.Visible = true;
                    if (phOperationsMenu != null) phOperationsMenu.Visible = true;
                }
                else if (isEmployee) // Si es Empleado (pero no Admin), solo ve operaciones
                {
                    if (phAdminMenu != null) phAdminMenu.Visible = false;
                    if (phOperationsMenu != null) phOperationsMenu.Visible = true;
                }
                // Si no es ni Admin ni Empleado (o no está autenticado), ambos menús específicos de rol permanecen ocultos (su valor por defecto o el false que se les asignó arriba).
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