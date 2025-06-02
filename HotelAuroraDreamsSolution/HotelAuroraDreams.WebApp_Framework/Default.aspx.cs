// File: Default.aspx.cs
using System;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using HotelAuroraDreams.WebApp_Framework.Models; // <-- ¡AÑADE O VERIFICA ESTA LÍNEA!
// ELIMINA: using HotelAuroraDreams.WebApp_Framework.Login; 

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
                RegisterAsyncTask(new System.Web.UI.PageAsyncTask(LoadUserDataAsync));
            }
        }

        private async Task LoadUserDataAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];

            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (Session["UserFullName"] != null)
            {
                lblWelcomeMessage.Text = $"Bienvenido, {Session["UserFullName"]}!";
            }
            else
            {
                lblWelcomeMessage.Text = "Bienvenido!";
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
                    // Ahora usa UserInfoViewModel del namespace Models
                    var userInfo = JsonConvert.DeserializeObject<UserInfoViewModel>(responseContent);
                    if (userInfo != null)
                    {
                        lblWelcomeMessage.Text = $"Bienvenido desde API, {userInfo.Nombre} {userInfo.Apellido}!";
                        lblApiData.Text = $"Email (API): {userInfo.Email}<br />ID Hotel (API): {userInfo.HotelID?.ToString() ?? "N/A"}";

                        Session["UserEmail"] = userInfo.Email;
                        Session["UserFullName"] = $"{userInfo.Nombre} {userInfo.Apellido}";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    lblMessage.Text = "Tu sesión ha expirado o el token no es válido. Por favor, inicia sesión de nuevo.";
                    Response.Cookies["AuthTokenHotel"].Expires = DateTime.Now.AddDays(-1);
                    Session.Clear();
                    // Considera redirigir a Login.aspx aquí también, quizás después de un breve mensaje o delay.
                    // Response.Redirect("~/Login.aspx"); 
                }
                else
                {
                    lblApiData.Text = $"Error al obtener info del usuario desde API: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                lblApiData.Text = $"Error conectando a la API para UserInfo: {ex.Message}";
            }
        }
    }
}