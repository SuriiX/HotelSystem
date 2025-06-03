// File: ~/Restaurante/NuevaReservaRestaurante.aspx.cs
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

namespace HotelAuroraDreams.WebApp_Framework.Restaurante
{
    public partial class NuevaReservaRestaurante : System.Web.UI.Page
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
                await LoadClientesDropdownAsync();
                await LoadRestaurantesDropdownAsync();
                pnlConfirmacionReservaRestaurante.Visible = false;
                lblDisponibilidadRestaurante.Text = "";
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

        private async Task LoadClientesDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Clientes");
                if (response.IsSuccessStatusCode)
                {
                    var clientes = JsonConvert.DeserializeObject<List<ClienteViewModel>>(await response.Content.ReadAsStringAsync());
                    var clienteListItems = clientes.Select(c => new ClienteListItemDto
                    {
                        ClienteID = c.ClienteID,
                        NombreCompleto = $"{c.Apellido}, {c.Nombre} ({c.NumeroDocumento})"
                    }).OrderBy(c => c.NombreCompleto).ToList();
                    ddlClienteRestaurante.DataSource = clienteListItems;
                    ddlClienteRestaurante.DataTextField = "NombreCompleto";
                    ddlClienteRestaurante.DataValueField = "ClienteID";
                    ddlClienteRestaurante.DataBind();
                    ddlClienteRestaurante.Items.Insert(0, new ListItem("-- Seleccione Cliente --", "0"));
                }
                else { ShowError($"Error cargando clientes: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando clientes: " + ex.Message); }
        }

        private async Task LoadRestaurantesDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                // Necesitarás un endpoint GET /api/restaurantes en tu API
                // que devuelva una lista de RestauranteListItemDto (RestauranteID, Nombre)
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Restaurantes"); // Asegúrate que este endpoint exista
                if (response.IsSuccessStatusCode)
                {
                    var restaurantes = JsonConvert.DeserializeObject<List<RestauranteListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlRestaurante.DataSource = restaurantes;
                    ddlRestaurante.DataTextField = "Nombre";
                    ddlRestaurante.DataValueField = "RestauranteID";
                    ddlRestaurante.DataBind();
                    ddlRestaurante.Items.Insert(0, new ListItem("-- Seleccione Restaurante --", "0"));
                }
                else { ShowError($"Error cargando restaurantes: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando restaurantes: " + ex.Message); }
        }

        protected void ddlRestaurante_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblDisponibilidadRestaurante.Text = "";
            pnlConfirmacionReservaRestaurante.Visible = false;
        }

        protected void txtFechaHoraRestaurante_TextChanged(object sender, EventArgs e)
        {
            lblDisponibilidadRestaurante.Text = "";
            pnlConfirmacionReservaRestaurante.Visible = false;
        }

        protected async void btnVerificarDisponibilidadRestaurante_Click(object sender, EventArgs e)
        {
            ClearMessages();
            lblDisponibilidadRestaurante.Text = "";
            pnlConfirmacionReservaRestaurante.Visible = false;

            if (ddlRestaurante.SelectedValue == "0" ||
                string.IsNullOrWhiteSpace(txtFechaReservaRestaurante.Text) ||
                string.IsNullOrWhiteSpace(txtHoraReservaRestaurante.Text) ||
                string.IsNullOrWhiteSpace(txtNumComensalesRestaurante.Text))
            {
                ShowError("Debe seleccionar restaurante, fecha, hora y número de comensales.");
                Page.Validate("ReservaRestValFechas"); // Asumiendo que tienes un grupo de validación
                return;
            }
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            TimeSpan hora;
            if (!TimeSpan.TryParse(txtHoraReservaRestaurante.Text, out hora))
            {
                ShowError("Formato de hora inválido."); return;
            }

            var requestDto = new RestauranteDisponibilidadRequestDto
            {
                RestauranteID = Convert.ToInt32(ddlRestaurante.SelectedValue),
                Fecha = Convert.ToDateTime(txtFechaReservaRestaurante.Text),
                Hora = hora,
                NumeroComensales = Convert.ToInt32(txtNumComensalesRestaurante.Text)
            };

            ViewState["DisponibilidadRestauranteRequest"] = requestDto;

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/ReservasRestaurante/Disponibilidad";
                string jsonPayload = JsonConvert.SerializeObject(requestDto);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var disponibilidad = JsonConvert.DeserializeObject<RestauranteDisponibleViewModel>(responseContent);
                    if (disponibilidad != null)
                    {
                        lblDisponibilidadRestaurante.Text = disponibilidad.Mensaje;
                        lblDisponibilidadRestaurante.ForeColor = disponibilidad.Disponible ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                        pnlConfirmacionReservaRestaurante.Visible = disponibilidad.Disponible;
                    }
                    else { ShowError("Respuesta de disponibilidad de restaurante inválida."); }
                }
                else { ShowError($"Error al verificar disponibilidad: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected async void btnConfirmarReservaRestaurante_Click(object sender, EventArgs e)
        {
            ClearMessages();
            if (!Page.IsValid) return; // Para validadores del grupo "ReservaRestVal"

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var disponibilidadRequest = ViewState["DisponibilidadRestauranteRequest"] as RestauranteDisponibilidadRequestDto;
            if (disponibilidadRequest == null ||
                disponibilidadRequest.RestauranteID != Convert.ToInt32(ddlRestaurante.SelectedValue) ||
                disponibilidadRequest.Fecha.Date != Convert.ToDateTime(txtFechaReservaRestaurante.Text).Date ||
                disponibilidadRequest.Hora != TimeSpan.Parse(txtHoraReservaRestaurante.Text) ||
                disponibilidadRequest.NumeroComensales != Convert.ToInt32(txtNumComensalesRestaurante.Text)
                )
            {
                ShowError("Los criterios de disponibilidad han cambiado o no son válidos. Por favor, verifique la disponibilidad nuevamente.");
                pnlConfirmacionReservaRestaurante.Visible = false;
                lblDisponibilidadRestaurante.Text = "";
                return;
            }

            if (ddlClienteRestaurante.SelectedValue == "0")
            {
                ShowError("Debe seleccionar un cliente.");
                pnlConfirmacionReservaRestaurante.Visible = true; // Mantener panel visible
                return;
            }

            var reservaData = new ReservaRestauranteBindingModel
            {
                ClienteID = Convert.ToInt32(ddlClienteRestaurante.SelectedValue),
                RestauranteID = disponibilidadRequest.RestauranteID,
                FechaReserva = disponibilidadRequest.Fecha,
                HoraReserva = disponibilidadRequest.Hora,
                NumeroComensales = disponibilidadRequest.NumeroComensales,
                Notas = txtNotasReservaRestaurante.Text.Trim()
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/ReservasRestaurante";
                string jsonPayload = JsonConvert.SerializeObject(reservaData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var reservaCreada = JsonConvert.DeserializeObject<ReservaRestauranteViewModel>(responseContent);
                    ShowSuccess($"¡Reserva en restaurante (ID: {reservaCreada.ReservaRestauranteID}) creada exitosamente para {reservaCreada.NombreCliente}!");
                    pnlConfirmacionReservaRestaurante.Visible = false;
                    lblDisponibilidadRestaurante.Text = "";
                    ddlClienteRestaurante.ClearSelection();
                }
                else
                {
                    ShowError($"Error al crear reserva de restaurante: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 300))}");
                    pnlConfirmacionReservaRestaurante.Visible = true;
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al crear reserva: {ex.Message}"); pnlConfirmacionReservaRestaurante.Visible = true; }
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