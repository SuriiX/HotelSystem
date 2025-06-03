// File: ~/Eventos/VerReservasEvento.aspx.cs
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
using HotelAuroraDreams.Api_Framework.Models.DTO;
using HotelAuroraDreams.WebApp_Framework.Models;
using Newtonsoft.Json;

namespace HotelAuroraDreams.WebApp_Framework.Eventos
{
    public partial class VerReservasEvento : System.Web.UI.Page
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
                await LoadSalonesDropdownFiltroAsync();
                await BindGridAsync();
                pnlDetallesReservaEvento.Visible = false;
                pnlEditarReservaEvento.Visible = false;
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

        private async Task LoadSalonesDropdownFiltroAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/SalonesEvento");
                if (response.IsSuccessStatusCode)
                {
                    var salones = JsonConvert.DeserializeObject<List<SalonEventoListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlFiltroSalonEvento.DataSource = salones;
                    ddlFiltroSalonEvento.DataTextField = "Nombre";
                    ddlFiltroSalonEvento.DataValueField = "SalonEventoID";
                    ddlFiltroSalonEvento.DataBind();
                    ddlFiltroSalonEvento.Items.Insert(0, new ListItem("-- Todos los Salones --", "0"));
                }
                else { ShowError($"Error cargando salones para filtro: {response.StatusCode}."); }
            }
            catch (Exception ex) { ShowError("Excepción cargando salones para filtro: " + ex.Message); }
        }

        private async Task BindGridAsync(string clienteId = null, string salonId = null, string fechaEvento = null, string estado = null)
        {
            ClearMessages();
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(clienteId) && clienteId != "0") queryParams.Add($"clienteId={clienteId}");
                if (!string.IsNullOrWhiteSpace(salonId) && salonId != "0") queryParams.Add($"salonId={salonId}");
                if (!string.IsNullOrWhiteSpace(fechaEvento)) queryParams.Add($"fecha={HttpUtility.UrlEncode(fechaEvento)}");
                if (!string.IsNullOrWhiteSpace(estado)) queryParams.Add($"estado={HttpUtility.UrlEncode(estado)}");

                string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/ReservasEvento{queryString}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // La API devuelve una lista simplificada para el grid (NombreEvento, Cliente, Salon, Fecha, Estado)
                    var reservas = JsonConvert.DeserializeObject<List<object>>(jsonResponse); // O un DTO específico si la API lo devuelve
                    gvReservasEvento.DataSource = reservas;
                    gvReservasEvento.DataBind();
                }
                else { ShowError($"Error al cargar reservas de evento: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnFiltrarReservasEvento_Click(object sender, EventArgs e)
        {
            gvReservasEvento.PageIndex = 0; // Resetear paginación al filtrar
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteIDEvento.Text.Trim(),
                    ddlFiltroSalonEvento.SelectedValue,
                    txtFiltroFechaEvento.Text.Trim(),
                    ddlFiltroEstadoEvento.SelectedValue
                );
            }));
            pnlDetallesReservaEvento.Visible = false;
            pnlEditarReservaEvento.Visible = false;
        }

        protected void btnLimpiarFiltrosReservasEvento_Click(object sender, EventArgs e)
        {
            txtFiltroClienteIDEvento.Text = "";
            ddlFiltroSalonEvento.ClearSelection();
            if (ddlFiltroSalonEvento.Items.FindByValue("0") != null) ddlFiltroSalonEvento.SelectedValue = "0";
            txtFiltroFechaEvento.Text = "";
            ddlFiltroEstadoEvento.ClearSelection();
            gvReservasEvento.PageIndex = 0;
            RegisterAsyncTask(new PageAsyncTask(async () => { await BindGridAsync(); }));
            pnlDetallesReservaEvento.Visible = false;
            pnlEditarReservaEvento.Visible = false;
        }

        protected void gvReservasEvento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReservasEvento.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteIDEvento.Text.Trim(),
                    ddlFiltroSalonEvento.SelectedValue,
                    txtFiltroFechaEvento.Text.Trim(),
                    ddlFiltroEstadoEvento.SelectedValue
                );
            }));
        }

        protected async void gvReservasEvento_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ClearMessages();
            int reservaEventoID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "VerDetallesEvento")
            {
                await LoadReservaEventoDetailsAsync(reservaEventoID);
            }
            else if (e.CommandName == "EditarReservaEvento")
            {
                await LoadReservaEventoForEditAsync(reservaEventoID);
            }
            else if (e.CommandName == "CancelarReservaEvento")
            {
                await CancelarReservaEventoAsync(reservaEventoID);
            }
        }

        private async Task LoadReservaEventoDetailsAsync(int reservaEventoID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ReservasEvento/{reservaEventoID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<Api_Framework.Models.DTO.ReservaEventoViewModel>(jsonResponse);
                    if (reserva != null)
                    {
                        litReservaEventoIDDetalle.Text = reserva.ReservaEventoID.ToString();
                        litNombreEventoDetalle.Text = HttpUtility.HtmlEncode(reserva.NombreEvento);
                        litClienteEventoDetalle.Text = HttpUtility.HtmlEncode(reserva.NombreCliente);
                        litSalonEventoDetalle.Text = HttpUtility.HtmlEncode(reserva.NombreSalon);
                        litTipoEventoDetalle.Text = HttpUtility.HtmlEncode(reserva.NombreTipoEvento ?? "N/A");
                        litFechaHoraEventoDetalle.Text = $"{reserva.FechaEvento:yyyy-MM-dd} de {reserva.HoraInicio:hh\\:mm} a {reserva.HoraFin:hh\\:mm}";
                        litAsistentesEstDetalle.Text = reserva.NumeroAsistentesEstimado.ToString();
                        litAsistentesConfDetalle.Text = reserva.NumeroAsistentesConfirmado?.ToString() ?? "N/A";
                        litEstadoEventoDetalle.Text = reserva.Estado;
                        litMontoSalonDetalle.Text = reserva.MontoEstimadoSalon?.ToString("C") ?? "N/A";
                        litMontoServiciosDetalle.Text = reserva.MontoEstimadoServicios?.ToString("C") ?? "N/A";
                        litMontoTotalEventoDetalle.Text = reserva.MontoTotalEvento.ToString("C");
                        litNotasEventoDetalle.Text = HttpUtility.HtmlEncode(reserva.NotasGenerales);

                        rptServiciosEventoDetalle.DataSource = reserva.ServiciosAdicionales;
                        rptServiciosEventoDetalle.DataBind();

                        pnlDetallesReservaEvento.Visible = true;
                        pnlEditarReservaEvento.Visible = false;
                    }
                }
                else { ShowError($"Error al cargar detalles de reserva de evento: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnCerrarDetallesEvento_Click(object sender, EventArgs e)
        {
            pnlDetallesReservaEvento.Visible = false;
        }

        private async Task LoadReservaEventoForEditAsync(int reservaEventoID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ReservasEvento/{reservaEventoID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<Api_Framework.Models.DTO.ReservaEventoViewModel>(jsonResponse);
                    if (reserva != null)
                    {
                        if (reserva.Estado == "Cancelada" || reserva.Estado == "Realizada")
                        {
                            ShowError($"La reserva de evento #{reservaEventoID} no se puede editar porque su estado es '{reserva.Estado}'.");
                            pnlEditarReservaEvento.Visible = false;
                            return;
                        }
                        hfReservaEventoIDEditar.Value = reserva.ReservaEventoID.ToString();
                        litReservaEventoIDEditar.Text = reserva.ReservaEventoID.ToString();
                        txtEditNombreEvento.Text = reserva.NombreEvento;
                        txtEditNumAsistentesEst.Text = reserva.NumeroAsistentesEstimado.ToString();
                        txtEditNumAsistentesConf.Text = reserva.NumeroAsistentesConfirmado?.ToString();
                        txtEditNotasEvento.Text = reserva.NotasGenerales;
                        ddlEditEstadoEvento.SelectedValue = reserva.Estado;
                        // La edición de servicios aquí es compleja, se omite por simplicidad en este panel.
                        // La API permite enviar una nueva lista de servicios.
                        litEditServiciosInfo.Visible = true;

                        pnlEditarReservaEvento.Visible = true;
                        pnlDetallesReservaEvento.Visible = false;
                    }
                }
                else { ShowError($"Error al cargar reserva de evento para editar: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected async void btnGuardarEdicionReservaEvento_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (string.IsNullOrEmpty(hfReservaEventoIDEditar.Value)) { ShowError("ID de reserva no válido."); return; }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var updateModel = new ReservaEventoUpdateBindingModel
            {
                NombreEvento = txtEditNombreEvento.Text.Trim(),
                NumeroAsistentesEstimado = Convert.ToInt32(txtEditNumAsistentesEst.Text),
                NumeroAsistentesConfirmado = string.IsNullOrWhiteSpace(txtEditNumAsistentesConf.Text) ? (int?)null : Convert.ToInt32(txtEditNumAsistentesConf.Text),
                NotasGenerales = txtEditNotasEvento.Text.Trim(),
                Estado = ddlEditEstadoEvento.SelectedValue,
                ServiciosAdicionales = new List<Api_Framework.Models.DTO.ReservaEventoServicioInputDto>() // Dejar vacío si no se editan servicios aquí
            };

            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/ReservasEvento/{hfReservaEventoIDEditar.Value}";
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(updateModel);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess("Reserva de evento actualizada exitosamente.");
                    pnlEditarReservaEvento.Visible = false;
                    await BindGridAsync(txtFiltroClienteIDEvento.Text.Trim(), ddlFiltroSalonEvento.SelectedValue, txtFiltroFechaEvento.Text.Trim(), ddlFiltroEstadoEvento.SelectedValue);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowError($"Error al actualizar reserva de evento: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al actualizar: {ex.Message}"); }
        }

        protected void btnCancelarEdicionReservaEvento_Click(object sender, EventArgs e)
        {
            pnlEditarReservaEvento.Visible = false;
        }

        private async Task CancelarReservaEventoAsync(int reservaEventoID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.PostAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ReservasEvento/{reservaEventoID}/Cancelar", null);
                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess($"Reserva de evento #{reservaEventoID} cancelada exitosamente.");
                    await BindGridAsync(txtFiltroClienteIDEvento.Text.Trim(), ddlFiltroSalonEvento.SelectedValue, txtFiltroFechaEvento.Text.Trim(), ddlFiltroEstadoEvento.SelectedValue);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowError($"Error al cancelar reserva de evento: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}");
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