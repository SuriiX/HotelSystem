// File: Default.aspx.cs
using System;
using System.Web;
using System.Web.UI; // Para PageAsyncTask
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
    public partial class _Default : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

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
                Response.Redirect("~/Login.aspx", true); // true para terminar la respuesta actual
                return;
            }

            string token = authTokenCookie.Value;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                        lblWelcomeMessage.Text = $"Bienvenido desde API, {userInfo.Nombre} {userInfo.Apellido}!";
                        lblApiData.Text = $"Email (API): {userInfo.Email}<br />Roles (API): {string.Join(", ", userInfo.Roles ?? new List<string>())}";

                        Session["UserEmail"] = userInfo.Email;
                        Session["UserFullName"] = $"{userInfo.Nombre} {userInfo.Apellido}";
                        Session["UserRoles"] = userInfo.Roles;

                        ConfigureUIVisibility(userInfo.Roles);
                    }
                    else
                    {
                        lblMessage.Text = "No se pudo obtener la información del usuario desde la API.";
                        ConfigureUIVisibility(null);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    lblMessage.Text = "Tu sesión ha expirado o el token no es válido. Redirigiendo a Login...";
                    Response.Cookies["AuthTokenHotel"].Expires = DateTime.Now.AddDays(-1);
                    Session.Clear();
                    Response.AddHeader("REFRESH", "3;URL=Login.aspx"); // Redirige después de 3 segundos
                }
                else
                {
                    lblApiData.Text = $"Error al obtener info del usuario desde API: {response.StatusCode}";
                    lblMessage.Text = $"Respuesta de la API: {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}"; // Muestra parte de la respuesta
                    ConfigureUIVisibility(null);
                }
            }
            catch (HttpRequestException httpEx)
            {
                lblApiData.Text = "Error de conexión con la API al obtener UserInfo.";
                lblMessage.Text = httpEx.Message;
                ConfigureUIVisibility(null);
            }
            catch (Exception ex)
            {
                lblApiData.Text = "Ocurrió un error inesperado al cargar datos del usuario.";
                lblMessage.Text = ex.Message;
                ConfigureUIVisibility(null);
            }
        }

        private void ConfigureUIVisibility(IList<string> roles)
        {
            if (pnlAdminOnly != null) // Asegurarse que el control exista
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