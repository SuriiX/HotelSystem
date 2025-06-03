// File: ~/Operaciones/CheckOut.aspx.cs
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
    public partial class CheckOutPage : System.Web.UI.Page // Renombrado para evitar conflicto
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
                pnlDetallesReserva_CheckOut.Visible = false;
                pnlFormCheckOut.Visible = false;
                pnlResultadoFactura.Visible = false;
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

        protected async void btnBuscarReserva_CheckOut_Click(object sender, EventArgs e)
        {
            ClearMessages();
            pnlDetallesReserva_CheckOut.Visible = false;
            pnlFormCheckOut.Visible = false;
            pnlResultadoFactura.Visible = false;

            if (string.IsNullOrWhiteSpace(txtBusquedaReservaID_CheckOut.Text) || !int.TryParse(txtBusquedaReservaID_CheckOut.Text, out int reservaId))
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
                        if (reserva.Estado == "Hospedado") // Solo procesar si está hospedado
                        {
                            litReservaID_CheckOut.Text = reserva.ReservaID.ToString();
                            litClienteNombre_CheckOut.Text = HttpUtility.HtmlEncode(reserva.NombreCliente);
                            litHotelNombre_CheckOut.Text = HttpUtility.HtmlEncode(reserva.NombreHotel);
                            litFechas_CheckOut.Text = $"{reserva.FechaEntrada:yyyy-MM-dd} al {reserva.FechaSalida:yyyy-MM-dd}";
                            litEstadoReserva_CheckOut.Text = reserva.Estado;

                            hfReservaID_CheckOut_Form.Value = reserva.ReservaID.ToString();
                            pnlDetallesReserva_CheckOut.Visible = true;
                            pnlFormCheckOut.Visible = true;
                        }
                        else
                        {
                            ShowError($"La reserva #{reservaId} no se puede procesar para Check-Out. Estado actual: {reserva.Estado}.");
                        }
                    }
                    else { ShowError($"Reserva #{reservaId} no encontrada."); }
                }
                else { ShowError($"Error al buscar reserva para check-out: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión al buscar reserva: {ex.Message}"); }
        }

        protected async void btnRealizarCheckOut_Click(object sender, EventArgs e)
        {
            ClearMessages();
            if (string.IsNullOrEmpty(hfReservaID_CheckOut_Form.Value) || !int.TryParse(hfReservaID_CheckOut_Form.Value, out int reservaId))
            {
                ShowError("ID de Reserva no válido para procesar Check-Out.");
                return;
            }
            if (!Page.IsValid) return; // Para validadores del grupo "CheckOutValidation"


            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var checkOutData = new CheckOutBindingModel
            {
                ReservaID = reservaId,
                MetodoPagoFinal = ddlMetodoPagoFinal.SelectedValue,
                Observaciones = txtObservacionesCheckOut.Text.Trim()
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/CheckOut";
                string jsonPayload = JsonConvert.SerializeObject(checkOutData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var checkOutResult = JsonConvert.DeserializeObject<CheckOutViewModel>(responseContent);
                    ShowSuccess($"Check-Out para Reserva #{checkOutResult.ReservaID} realizado por {checkOutResult.NombreEmpleado}.");

                    litCheckOutIDResultado.Text = checkOutResult.CheckOutID.ToString();
                    litClienteFacturaResultado.Text = HttpUtility.HtmlEncode(checkOutResult.NombreCliente);
                    litTotalFacturaResultado.Text = checkOutResult.TotalFactura.ToString("C");
                    litFacturaIDResultado.Text = checkOutResult.FacturaID?.ToString() ?? "N/A";

                    if (checkOutResult.FacturaID.HasValue)
                    {
                        lnkVerFacturaPdf.NavigateUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Facturas/{checkOutResult.FacturaID.Value}/pdf";
                        lnkVerFacturaPdf.Visible = true;
                    }
                    else { lnkVerFacturaPdf.Visible = false; }

                    pnlResultadoFactura.Visible = true;
                    pnlDetallesReserva_CheckOut.Visible = false;
                    pnlFormCheckOut.Visible = false;
                    txtBusquedaReservaID_CheckOut.Text = "";
                }
                else
                {
                    ShowError($"Error al realizar Check-Out: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al realizar Check-Out: {ex.Message}"); }
        }

        private void ClearMessages()
        {
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }
        private void ShowError(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = message;
            lblSuccessMessage.Text = "";
        }
        private void ShowSuccess(string message)
        {
            lblSuccessMessage.ForeColor = System.Drawing.Color.Green;
            lblSuccessMessage.Text = message;
            lblMessage.Text = "";
        }
    }
}