// File: ~/Reservas/VerReservas.aspx.cs
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
using System.Globalization; // Para DateTime.TryParseExact

namespace HotelAuroraDreams.WebApp_Framework.Reservas
{
    public partial class VerReservas : System.Web.UI.Page
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
                await LoadHotelesDropdownFiltroAsync();
                await BindGridAsync();
                pnlDetallesReserva.Visible = false;
                pnlEditarReserva.Visible = false;
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

        private async Task LoadHotelesDropdownFiltroAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Hoteles");
                if (response.IsSuccessStatusCode)
                {
                    var hoteles = JsonConvert.DeserializeObject<List<HotelSimpleViewModel>>(await response.Content.ReadAsStringAsync());
                    ddlFiltroHotel.DataSource = hoteles;
                    ddlFiltroHotel.DataTextField = "Nombre";
                    ddlFiltroHotel.DataValueField = "HotelID";
                    ddlFiltroHotel.DataBind();
                    ddlFiltroHotel.Items.Insert(0, new ListItem("-- Todos los Hoteles --", "0"));
                }
                else { ShowError($"Error cargando hoteles para filtro: {response.StatusCode}."); }
            }
            catch (Exception ex) { ShowError("Excepción cargando hoteles para filtro: " + ex.Message); }
        }

        private async Task BindGridAsync(string clienteId = null, string hotelId = null, string fechaDesde = null, string fechaHasta = null, string estado = null)
        {
            lblMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(clienteId) && clienteId != "0") queryParams.Add($"clienteId={clienteId}");
                if (!string.IsNullOrWhiteSpace(hotelId) && hotelId != "0") queryParams.Add($"hotelId={hotelId}");
                if (!string.IsNullOrWhiteSpace(fechaDesde)) queryParams.Add($"fechaDesde={HttpUtility.UrlEncode(fechaDesde)}");
                if (!string.IsNullOrWhiteSpace(fechaHasta)) queryParams.Add($"fechaHasta={HttpUtility.UrlEncode(fechaHasta)}");
                if (!string.IsNullOrWhiteSpace(estado)) queryParams.Add($"estado={HttpUtility.UrlEncode(estado)}");

                string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Reservas{queryString}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // La API ya devuelve ReservaViewModel, pero la lista es simplificada.
                    // Para el GridView, vamos a usar un modelo que coincida con los BoundFields.
                    var reservas = JsonConvert.DeserializeObject<List<ReservaViewModel>>(jsonResponse);
                    gvReservas.DataSource = reservas;
                    gvReservas.DataBind();
                }
                else
                {
                    ShowError($"Error al cargar reservas: {response.StatusCode}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteID.Text.Trim(),
                    ddlFiltroHotel.SelectedValue,
                    txtFiltroFechaDesde.Text.Trim(),
                    txtFiltroFechaHasta.Text.Trim(),
                    ddlFiltroEstado.SelectedValue
                );
            }));
            pnlDetallesReserva.Visible = false; // Ocultar paneles al filtrar
            pnlEditarReserva.Visible = false;
        }

        protected void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtFiltroClienteID.Text = "";
            ddlFiltroHotel.SelectedValue = "0";
            txtFiltroFechaDesde.Text = "";
            txtFiltroFechaHasta.Text = "";
            ddlFiltroEstado.SelectedValue = "";
            RegisterAsyncTask(new PageAsyncTask(async () => { await BindGridAsync(); }));
            pnlDetallesReserva.Visible = false;
            pnlEditarReserva.Visible = false;
        }

        protected void gvReservas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReservas.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                   txtFiltroClienteID.Text.Trim(),
                   ddlFiltroHotel.SelectedValue,
                   txtFiltroFechaDesde.Text.Trim(),
                   txtFiltroFechaHasta.Text.Trim(),
                   ddlFiltroEstado.SelectedValue
               );
            }));
        }

        protected async void gvReservas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ClearMessages();
            int reservaID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "VerDetalles")
            {
                await LoadReservaDetailsAsync(reservaID);
            }
            else if (e.CommandName == "EditarReserva")
            {
                await LoadReservaForEditAsync(reservaID);
            }
            else if (e.CommandName == "CancelarReserva")
            {
                await CancelarReservaAsync(reservaID);
            }
        }

        private async Task LoadReservaDetailsAsync(int reservaID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Reservas/{reservaID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<ReservaViewModel>(jsonResponse);
                    if (reserva != null)
                    {
                        litReservaIDDetalle.Text = reserva.ReservaID.ToString();
                        litClienteDetalle.Text = reserva.NombreCliente;
                        litHotelDetalle.Text = reserva.NombreHotel;
                        litFechasDetalle.Text = $"{reserva.FechaEntrada:yyyy-MM-dd} al {reserva.FechaSalida:yyyy-MM-dd}";
                        litHuespedesDetalle.Text = reserva.NumeroHuespedes.ToString();
                        litEstadoDetalle.Text = reserva.Estado;
                        litMontoTotalDetalle.Text = reserva.MontoTotalReserva.ToString("C");
                        litNotasDetalle.Text = HttpUtility.HtmlEncode(reserva.Notas); // Sanitizar

                        rptHabitacionesReservadas.DataSource = reserva.HabitacionesReservadas;
                        rptHabitacionesReservadas.DataBind();

                        pnlDetallesReserva.Visible = true;
                        pnlEditarReserva.Visible = false;
                    }
                }
                else { ShowError($"Error al cargar detalles de la reserva: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnCerrarDetalles_Click(object sender, EventArgs e)
        {
            pnlDetallesReserva.Visible = false;
        }

        private async Task LoadReservaForEditAsync(int reservaID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Reservas/{reservaID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<ReservaViewModel>(jsonResponse); // Usamos ViewModel para obtener datos actuales
                    if (reserva != null)
                    {
                        if (reserva.Estado == "Cancelada" || reserva.Estado == "Completada")
                        {
                            ShowError($"La reserva #{reservaID} no se puede editar porque su estado es '{reserva.Estado}'.");
                            pnlEditarReserva.Visible = false;
                            return;
                        }
                        hfReservaIDEditar.Value = reserva.ReservaID.ToString();
                        litReservaIDEditar.Text = reserva.ReservaID.ToString();
                        txtEditNumeroHuespedes.Text = reserva.NumeroHuespedes.ToString();
                        txtEditNotas.Text = reserva.Notas;
                        ddlEditEstado.SelectedValue = reserva.Estado;

                        pnlEditarReserva.Visible = true;
                        pnlDetallesReserva.Visible = false;
                    }
                }
                else { ShowError($"Error al cargar reserva para editar: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected async void btnGuardarEdicionReserva_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (string.IsNullOrEmpty(hfReservaIDEditar.Value)) { ShowError("ID de reserva no válido para editar."); return; }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var updateModel = new ReservaUpdateBindingModel
            {
                NumeroHuespedes = Convert.ToInt32(txtEditNumeroHuespedes.Text),
                Notas = txtEditNotas.Text.Trim(),
                Estado = ddlEditEstado.SelectedValue
            };

            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Reservas/{hfReservaIDEditar.Value}";
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(updateModel);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess("Reserva actualizada exitosamente.");
                    pnlEditarReserva.Visible = false;
                    await BindGridAsync(txtFiltroClienteID.Text.Trim(), ddlFiltroHotel.SelectedValue, txtFiltroFechaDesde.Text.Trim(), txtFiltroFechaHasta.Text.Trim(), ddlFiltroEstado.SelectedValue);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowError($"Error al actualizar reserva: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al actualizar: {ex.Message}"); }
        }

        protected void btnCancelarEdicionReserva_Click(object sender, EventArgs e)
        {
            pnlEditarReserva.Visible = false;
        }

        private async Task CancelarReservaAsync(int reservaID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Reservas/{reservaID}/Cancelar", null); // POST sin cuerpo
                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess($"Reserva #{reservaID} cancelada exitosamente.");
                    await BindGridAsync(txtFiltroClienteID.Text.Trim(), ddlFiltroHotel.SelectedValue, txtFiltroFechaDesde.Text.Trim(), txtFiltroFechaHasta.Text.Trim(), ddlFiltroEstado.SelectedValue);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowError($"Error al cancelar reserva: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al cancelar: {ex.Message}"); }
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