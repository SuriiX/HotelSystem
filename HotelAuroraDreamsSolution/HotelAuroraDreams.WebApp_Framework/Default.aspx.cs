// File: Default.aspx.cs
using System;
using System.Web;
using System.Web.UI;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using HotelAuroraDreams.WebApp_Framework.Models;
using System.Collections.Generic;
using System.Linq;

namespace HotelAuroraDreams.WebApp_Framework
{
    public partial class _Default : System.Web.UI.Page // El nombre de clase aquí es _Default
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        // Controles referenciados desde el markup - el archivo .designer.cs los declarará
        // protected global::System.Web.UI.WebControls.Label lblWelcomeMessage;
        // protected global::System.Web.UI.WebControls.Label lblApiData;
        // protected global::System.Web.UI.WebControls.Label lblMessage;
        // protected global::System.Web.UI.WebControls.Panel pnlAdminOnly;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(LoadUserDataAsync));
            }
        }

        private async Task LoadUserDataAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];

            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            if (Session["UserRoles"] == null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
                try
                {
                    string userInfoUrl = _apiBaseUrl.TrimEnd('/') + "/api/Account/UserInfo";
                    HttpResponseMessage response = await client.GetAsync(userInfoUrl);
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = JsonConvert.DeserializeObject<UserInfoViewModel>(responseContent);
                        if (userInfo != null)
                        {
                            if (lblWelcomeMessage != null) lblWelcomeMessage.Text = $"Bienvenido desde API, {userInfo.Nombre} {userInfo.Apellido}!";
                            if (lblApiData != null) lblApiData.Text = $"Email (API): {userInfo.Email}<br />Roles (API): {string.Join(", ", userInfo.Roles ?? new List<string>())}";

                            Session["UserEmail"] = userInfo.Email;
                            Session["UserFullName"] = $"{userInfo.Nombre} {userInfo.Apellido}";
                            Session["UserRoles"] = userInfo.Roles;

                            ConfigureUIVisibility(userInfo.Roles);
                        }
                        else
                        {
                            if (lblMessage != null) lblMessage.Text = "No se pudo obtener la información del usuario desde la API.";
                            ConfigureUIVisibility(null);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (lblMessage != null) lblMessage.Text = "Tu sesión ha expirado o el token no es válido. Redirigiendo a Login...";
                        if (Response.Cookies["AuthTokenHotel"] != null) Response.Cookies["AuthTokenHotel"].Expires = DateTime.Now.AddDays(-1);
                        Session.Clear();
                        Response.AddHeader("REFRESH", "3;URL=Login.aspx");
                    }
                    else
                    {
                        if (lblApiData != null) lblApiData.Text = $"Error al obtener info del usuario desde API: {response.StatusCode}";
                        if (lblMessage != null) lblMessage.Text = $"Respuesta de la API: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}";
                        ConfigureUIVisibility(null);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    if (lblApiData != null) lblApiData.Text = "Error de conexión con la API al obtener UserInfo.";
                    if (lblMessage != null) lblMessage.Text = httpEx.Message;
                    ConfigureUIVisibility(null);
                }
                catch (Exception ex)
                {
                    if (lblApiData != null) lblApiData.Text = "Ocurrió un error inesperado al cargar datos del usuario.";
                    if (lblMessage != null) lblMessage.Text = ex.Message;
                    ConfigureUIVisibility(null);
                }
            }
            else
            {
                if (Session["UserFullName"] != null && lblWelcomeMessage != null)
                {
                    lblWelcomeMessage.Text = $"Bienvenido, {Session["UserFullName"]}!";
                }
                var userRoles = Session["UserRoles"] as IList<string>;
                if (lblApiData != null) lblApiData.Text = $"Roles (Sesión): {string.Join(", ", userRoles ?? new List<string>())}";
                ConfigureUIVisibility(userRoles);
            }
        }

        private void ConfigureUIVisibility(IList<string> roles)
        {
            if (pnlAdminOnly != null)
            {
                if (roles != null && roles.Contains("Administrador"))
                {
                    pnlAdminOnly.Visible = true;
                }
                else
                {
                    pnlAdminOnly.Visible = false;
                }
            }
        }
    }
}