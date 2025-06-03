// File: ~/Operaciones/CheckIn.aspx.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HotelAuroraDreams.WebApp_Framework.Models;
using Newtonsoft.Json;

namespace HotelAuroraDreams.WebApp_Framework.Operaciones
{
    public partial class CheckInPage : System.Web.UI.Page // Cambiado nombre de clase para evitar conflicto con entidad CheckIn
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(Page_Load_Async));
        }

        private async Task Page_Load_Async()
        {
            if (!await IsUserAuthorizedAsync())
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true);
                return;
            }

            if (!IsPostBack)
            {
                pnlDetallesReserva.Visible = false;
                pnlFormCheckIn.Visible = false;
            }
        }

        private async Task<bool> IsUserAuthorizedAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value)) return false;
            var userRoles = Session["UserRoles"] as IList<string>;
            if (userRoles != null && (userRoles.Contains("Administrador") || userRoles.Contains("Empleado"))) return true;

            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                var response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Account/UserInfo");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = JsonConvert.DeserializeObject<UserInfoViewModel>(await response.Content.ReadAsStringAsync());
                    if (userInfo != null && userInfo.Roles != null && (userInfo.Roles.Contains("Administrador") || userInfo.Roles.Contains("Empleado")))
                    {
                        Session["UserRoles"] = userInfo.Roles; return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void SetAuthorizationHeader(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected async void btnBuscarReserva_Click(object sender, EventArgs e)
        {
            ClearMessages();
            pnlDetallesReserva.Visible = false;
            pnlFormCheckIn.Visible = false;

            if (string.IsNullOrWhiteSpace(txtBusquedaReservaID.Text) || !int.TryParse(txtBusquedaReservaID.Text, out int reservaId))
            {
                ShowError("Por favor, ingrese un ID de Reserva válido.");
                return;
            }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Reservas/{reservaId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<ReservaViewModel>(jsonResponse);
                    if (reserva != null)
                    {
                        if (reserva.Estado == "Confirmada" && reserva.FechaEntrada.Date <= DateTime.Today)
                        {
                            litReservaID.Text = reserva.ReservaID.ToString();
                            litClienteNombre.Text = HttpUtility.HtmlEncode(reserva.NombreCliente);
                            litHotelNombre.Text = HttpUtility.HtmlEncode(reserva.NombreHotel);
                            litFechaEntrada.Text = reserva.FechaEntrada.ToString("yyyy-MM-dd");
                            litFechaSalida.Text = reserva.FechaSalida.ToString("yyyy-MM-dd");
                            litNumeroHuespedes.Text = reserva.NumeroHuespedes.ToString();
                            litEstadoReserva.Text = reserva.Estado;

                            rptHabitacionesCheckIn.DataSource = reserva.HabitacionesReservadas;
                            rptHabitacionesCheckIn.DataBind();

                            hfReservaIDCheckIn.Value = reserva.ReservaID.ToString();
                            pnlDetallesReserva.Visible = true;
                            pnlFormCheckIn.Visible = true;
                        }
                        else
                        {
                            ShowError($"La reserva #{reservaId} no se puede procesar para Check-In. Estado: {reserva.Estado}, Fecha Entrada: {reserva.FechaEntrada:yyyy-MM-dd}.");
                        }
                    }
                    else
                    {
                        ShowError($"Reserva #{reservaId} no encontrada o respuesta inválida.");
                    }
                }
                else
                {
                    ShowError($"Error al buscar reserva: {response.StatusCode}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al buscar reserva: {ex.Message}"); }
        }

        protected async void btnRealizarCheckIn_Click(object sender, EventArgs e)
        {
            ClearMessages();
            if (string.IsNullOrEmpty(hfReservaIDCheckIn.Value) || !int.TryParse(hfReservaIDCheckIn.Value, out int reservaId))
            {
                ShowError("ID de Reserva no válido para procesar Check-In.");
                return;
            }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var checkInData = new CheckInBindingModel
            {
                ReservaID = reservaId,
                MetodoPagoAdelanto = txtMetodoPagoAdelanto.Text.Trim(),
                DocumentosVerificados = chkDocumentosVerificados.Checked,
                Observaciones = txtObservacionesCheckIn.Text.Trim()
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/CheckIn";
                string jsonPayload = JsonConvert.SerializeObject(checkInData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var checkInResult = JsonConvert.DeserializeObject<CheckInViewModel>(responseContent);
                    ShowSuccess($"Check-In para Reserva #{checkInResult.ReservaID} realizado exitosamente por {checkInResult.NombreEmpleado}. ID Check-In: {checkInResult.CheckInID}");
                    pnlDetallesReserva.Visible = false;
                    pnlFormCheckIn.Visible = false;
                    txtBusquedaReservaID.Text = "";
                }
                else
                {
                    ShowError($"Error al realizar Check-In: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al realizar Check-In: {ex.Message}"); }
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }
        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblSuccessMessage.Text = "";
        }
        private void ShowSuccess(string message)
        {
            lblSuccessMessage.Text = message;
            lblMessage.Text = "";
        }
    }
}