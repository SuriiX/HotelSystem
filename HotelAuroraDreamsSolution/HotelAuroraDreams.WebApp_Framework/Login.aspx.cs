// File: Login.aspx.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Configuration;
using HotelAuroraDreams.WebApp_Framework.Models; // <-- ¡AÑADE O VERIFICA ESTA LÍNEA!

namespace HotelAuroraDreams.WebApp_Framework
{
    public partial class Login : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            // ... (sin cambios aquí) ...
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
                lblMessage.Text = "Error: La URL base de la API no está configurada.";
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
                    // Ahora usa TokenResponseModel del namespace Models
                    var tokenData = JsonConvert.DeserializeObject<TokenResponseModel>(responseContent);

                    if (tokenData != null && !string.IsNullOrEmpty(tokenData.access_token))
                    {
                        HttpCookie authTokenCookie = new HttpCookie("AuthTokenHotel")
                        {
                            Value = tokenData.access_token,
                            HttpOnly = true,
                            Secure = Request.IsSecureConnection,
                            Expires = DateTime.Now.AddSeconds(tokenData.expires_in > 60 ? tokenData.expires_in - 60 : tokenData.expires_in)
                        };
                        Response.Cookies.Add(authTokenCookie);

                        Session["UserEmail"] = tokenData.userName;
                        Session["UserFullName"] = tokenData.nombreCompleto;

                        Response.Redirect("~/Default.aspx");
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
                        // Ahora usa ApiErrorModel del namespace Models
                        var errorData = JsonConvert.DeserializeObject<ApiErrorModel>(responseContent);
                        if (errorData != null && !string.IsNullOrEmpty(errorData.error_description))
                        {
                            lblMessage.Text = $"Error: {errorData.error_description}";
                        }
                        else if (errorData != null && !string.IsNullOrEmpty(errorData.Message))
                        {
                            lblMessage.Text = $"Error: {errorData.Message}";
                        }
                        else
                        {
                            lblMessage.Text = $"Error de la API: {response.StatusCode}";
                        }
                    }
                    catch
                    {
                        lblMessage.Text = $"Error de la API: {response.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException)
            {
                lblMessage.Text = "No se pudo conectar con el servicio de autenticación. Verifique que la API esté ejecutándose.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Ocurrió un error inesperado. ({ex.Message.Substring(0, Math.Min(ex.Message.Length, 100))})";
            }
        }
    }
    // YA NO DEFINAS TokenResponseModel ni ApiErrorModel AQUÍ DENTRO
}