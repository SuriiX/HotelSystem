using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers; // Para MediaTypeWithQualityHeaderValue si es necesario
using System.Threading.Tasks;
using System.Web; // Para HttpCookie, Session, Response.Redirect
using Newtonsoft.Json; // Para deserializar
using System.Configuration; // Para ConfigurationManager

namespace HotelAuroraDreams.WebApp_Framework
{
    public partial class Login : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }

        protected async void btnLogin_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Por favor, ingrese correo y contraseña.";
                return;
            }

            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                lblMessage.Text = "Error: La URL base de la API no está configurada en Web.config.";
                return;
            }

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password)
            });

            try
            {
                string tokenUrl = _apiBaseUrl.TrimEnd('/') + "/api/token";
                HttpResponseMessage response = await client.PostAsync(tokenUrl, formContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokenData = JsonConvert.DeserializeObject<TokenResponseModel>(responseContent);

                    if (tokenData != null && !string.IsNullOrEmpty(tokenData.access_token))
                    {
                        HttpCookie authTokenCookie = new HttpCookie("AuthTokenHotel")
                        {
                            Value = tokenData.access_token,
                            HttpOnly = true, // Protege contra XSS
                            Secure = Request.IsSecureConnection, // Enviar solo sobre HTTPS
                            Expires = DateTime.Now.AddSeconds(tokenData.expires_in - 60) // Un poco antes de que expire el token
                        };
                        Response.Cookies.Add(authTokenCookie);

                        Session["UserEmail"] = tokenData.userName;
                        Session["UserFullName"] = tokenData.nombreCompleto;

                        Response.Redirect("~/Default.aspx"); // Cambia "Default.aspx" por tu página de inicio deseada
                    }
                    else
                    {
                        lblMessage.Text = "Respuesta de token inválida desde la API.";
                    }
                }
                else
                {
                    try
                    {
                        var errorData = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                        lblMessage.Text = $"Error: {errorData?.error_description ?? response.ReasonPhrase}";
                    }
                    catch
                    {
                        lblMessage.Text = $"Error de la API: {response.StatusCode} - {response.ReasonPhrase}";
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                lblMessage.Text = $"No se pudo conectar con el servicio de autenticación. Verifique que la API esté ejecutándose. ({httpEx.Message})";
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Ocurrió un error inesperado: {ex.Message}";
            }
        }
    }

    public class TokenResponseModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string nombreCompleto { get; set; }
    }

    public class ApiErrorModel
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string Message { get; set; }
    }
}