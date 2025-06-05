// File: ~/Restaurante/VerReservasRestaurante.aspx.cs
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

namespace HotelAuroraDreams.WebApp_Framework.Restaurante
{
    public partial class VerReservasRestaurante : System.Web.UI.Page
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
                await LoadRestaurantesDropdownFiltroAsync();
                await BindGridAsync();
                pnlEditarReservaRestaurante.Visible = false;
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

        private async Task LoadRestaurantesDropdownFiltroAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Restaurantes");
                if (response.IsSuccessStatusCode)
                {
                    var restaurantes = JsonConvert.DeserializeObject<List<RestauranteListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlFiltroRestaurante.DataSource = restaurantes;
                    ddlFiltroRestaurante.DataTextField = "Nombre";
                    ddlFiltroRestaurante.DataValueField = "RestauranteID";
                    ddlFiltroRestaurante.DataBind();
                    ddlFiltroRestaurante.Items.Insert(0, new ListItem("-- Todos --", "0"));
                }
                else { ShowError($"Error cargando restaurantes para filtro: {response.StatusCode}."); }
            }
            catch (Exception ex) { ShowError("Excepción cargando restaurantes para filtro: " + ex.Message); }
        }

        private async Task BindGridAsync(string clienteId = null, string restauranteId = null, string fecha = null)
        {
            ClearMessages();
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(clienteId)) queryParams.Add($"clienteId={clienteId}");
                if (!string.IsNullOrWhiteSpace(restauranteId) && restauranteId != "0") queryParams.Add($"restauranteId={restauranteId}");
                if (!string.IsNullOrWhiteSpace(fecha)) queryParams.Add($"fecha={HttpUtility.UrlEncode(fecha)}");

                string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/ReservasRestaurante{queryString}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reservas = JsonConvert.DeserializeObject<List<Api_Framework.Models.DTO.ReservaRestauranteViewModel>>(jsonResponse);
                    gvReservasRestaurante.DataSource = reservas;
                    gvReservasRestaurante.DataBind();
                }
                else { ShowError($"Error al cargar reservas de restaurante: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnFiltrarReservasRest_Click(object sender, EventArgs e)
        {
            gvReservasRestaurante.PageIndex = 0;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteIDRest.Text.Trim(),
                    ddlFiltroRestaurante.SelectedValue,
                    txtFiltroFechaRest.Text.Trim()
                );
            }));
            pnlEditarReservaRestaurante.Visible = false;
        }

        protected void btnLimpiarFiltrosReservasRest_Click(object sender, EventArgs e)
        {
            txtFiltroClienteIDRest.Text = "";
            ddlFiltroRestaurante.SelectedValue = "0";
            txtFiltroFechaRest.Text = "";
            gvReservasRestaurante.PageIndex = 0;
            RegisterAsyncTask(new PageAsyncTask(async () => { await BindGridAsync(); }));
            pnlEditarReservaRestaurante.Visible = false;
        }

        protected void gvReservasRestaurante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReservasRestaurante.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteIDRest.Text.Trim(),
                    ddlFiltroRestaurante.SelectedValue,
                    txtFiltroFechaRest.Text.Trim()
                );
            }));
        }

        protected async void gvReservasRestaurante_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ClearMessages();
            int reservaRestID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditarReservaRest")
            {
                await LoadReservaRestauranteForEditAsync(reservaRestID);
            }
            else if (e.CommandName == "CancelarReservaRest") // O "DeleteReservaRest" si usas el endpoint DELETE
            {
                await CancelarReservaRestauranteAsync(reservaRestID);
            }
        }

        private async Task LoadReservaRestauranteForEditAsync(int reservaRestID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ReservasRestaurante/{reservaRestID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var reserva = JsonConvert.DeserializeObject<Api_Framework.Models.DTO.ReservaRestauranteViewModel>(jsonResponse);
                    if (reserva != null)
                    {
                        if (reserva.Estado == "Cancelada" || reserva.Estado == "Atendida")
                        {
                            ShowError($"La reserva de restaurante #{reservaRestID} no se puede editar porque su estado es '{reserva.Estado}'.");
                            pnlEditarReservaRestaurante.Visible = false;
                            return;
                        }
                        hfReservaRestIDEditar.Value = reserva.ReservaRestauranteID.ToString();
                        litReservaRestIDEditar.Text = reserva.ReservaRestauranteID.ToString();
                        litClienteReservaRestEditar.Text = HttpUtility.HtmlEncode(reserva.NombreCliente);
                        litRestauranteReservaRestEditar.Text = HttpUtility.HtmlEncode(reserva.NombreRestaurante);
                        litFechaHoraReservaRestEditar.Text = $"{reserva.FechaReserva:yyyy-MM-dd} a las {reserva.HoraReserva:hh\\:mm}";

                        txtEditNumComensalesRest.Text = reserva.NumeroComensales.ToString();
                        txtEditNotasRest.Text = reserva.Notas;
                        ddlEditEstadoRest.SelectedValue = reserva.Estado;

                        pnlEditarReservaRestaurante.Visible = true;
                    }
                }
                else { ShowError($"Error al cargar reserva de restaurante para editar: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected async void btnGuardarEdicionReservaRest_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (string.IsNullOrEmpty(hfReservaRestIDEditar.Value)) { ShowError("ID de reserva no válido."); return; }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var updateModel = new ReservaRestauranteUpdateBindingModel
            {
                NumeroComensales = Convert.ToInt32(txtEditNumComensalesRest.Text),
                Notas = txtEditNotasRest.Text.Trim(),
                Estado = ddlEditEstadoRest.SelectedValue
            };

            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/ReservasRestaurante/{hfReservaRestIDEditar.Value}";
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(updateModel);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess("Reserva de restaurante actualizada exitosamente.");
                    pnlEditarReservaRestaurante.Visible = false;
                    await BindGridAsync(txtFiltroClienteIDRest.Text.Trim(), ddlFiltroRestaurante.SelectedValue, txtFiltroFechaRest.Text.Trim());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ShowError($"Error al actualizar reserva: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al actualizar: {ex.Message}"); }
        }

        protected void btnCancelarEdicionReservaRest_Click(object sender, EventArgs e)
        {
            pnlEditarReservaRestaurante.Visible = false;
        }

        private async Task CancelarReservaRestauranteAsync(int reservaRestID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("Sesión expirada."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                // Usamos DELETE como lo definimos en el API controller para cancelar/eliminar
                HttpResponseMessage response = await client.DeleteAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ReservasRestaurante/{reservaRestID}");
                if (response.IsSuccessStatusCode)
                {
                    ShowSuccess($"Reserva de restaurante #{reservaRestID} cancelada/eliminada exitosamente.");
                    await BindGridAsync(txtFiltroClienteIDRest.Text.Trim(), ddlFiltroRestaurante.SelectedValue, txtFiltroFechaRest.Text.Trim());
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