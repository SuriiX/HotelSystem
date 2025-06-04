// File: Login.aspx.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Configuration;
using HotelAuroraDreams.WebApp_Framework.Models;
using System.Net.Http.Headers; // <-- ¡AÑADE O VERIFICA ESTA LÍNEA!

namespace HotelAuroraDreams.WebApp_Framework
{
    public partial class Login : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                HttpResponseMessage responseToken = await client.PostAsync(tokenUrl, formContent);
                string responseTokenContent = await responseToken.Content.ReadAsStringAsync();

                if (responseToken.IsSuccessStatusCode)
                {
                    var tokenData = JsonConvert.DeserializeObject<TokenResponseModel>(responseTokenContent);

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

                        Session["UserEmail"] = tokenData.userName; // userName de la respuesta del token
                        Session["UserFullName"] = tokenData.nombreCompleto; // nombreCompleto de la respuesta del token

                        // ***** NUEVO: Obtener UserInfo (incluyendo roles) y guardar en Sesión *****
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.access_token);
                        string userInfoUrl = _apiBaseUrl.TrimEnd('/') + "/api/Account/UserInfo";
                        HttpResponseMessage responseUser = await client.GetAsync(userInfoUrl);
                        if (responseUser.IsSuccessStatusCode)
                        {
                            var userInfo = JsonConvert.DeserializeObject<UserInfoViewModel>(await responseUser.Content.ReadAsStringAsync());
                            if (userInfo != null)
                            {
                                Session["UserRoles"] = userInfo.Roles; // Guardar roles
                            }
                        }
                        else
                        {
                            // No pudo obtener roles, pero el login fue exitoso.
                            // Podría ser un problema si Site.Master depende de esto inmediatamente.
                            // Dejar Session["UserRoles"] como null o lista vacía.
                            Session["UserRoles"] = new List<string>();
                            lblMessage.Text = "Login exitoso, pero no se pudieron cargar los roles del usuario.";
                        }
                        // ***** FIN DE NUEVO CÓDIGO *****

                        Response.Redirect("~/Default.aspx", true);
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
                        var errorData = JsonConvert.DeserializeObject<ApiErrorModel>(responseTokenContent);
                        if (errorData != null && !string.IsNullOrEmpty(errorData.error_description))
                        {
                            lblMessage.Text = $"Error: {errorData.error_description}";
                        }
                        // ... (resto del manejo de errores como estaba) ...
                        else
                        {
                            lblMessage.Text = $"Error de la API: {responseToken.StatusCode}";
                        }
                    }
                    catch
                    {
                        lblMessage.Text = $"Error de la API: {responseToken.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException)
            {
                lblMessage.Text = "No se pudo conectar con el servicio de autenticación.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Ocurrió un error inesperado: {ex.Message.Substring(0, Math.Min(ex.Message.Length, 100))}";
            }
            // YA NO DEFINAS TokenResponseModel ni ApiErrorModel AQUÍ DENTRO
        }
    }
}