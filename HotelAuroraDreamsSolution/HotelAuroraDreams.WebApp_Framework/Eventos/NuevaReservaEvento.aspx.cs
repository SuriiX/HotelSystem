// File: ~/Eventos/NuevaReservaEvento.aspx.cs
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

namespace HotelAuroraDreams.WebApp_Framework.Eventos
{
    public partial class NuevaReservaEvento : System.Web.UI.Page
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
                await LoadSalonesDropdownAsync();
                await LoadTiposEventoDropdownAsync();
                await LoadServiciosAdicionalesGridAsync(); // Cargar todos los servicios disponibles

                pnlServiciosAdicionales.Visible = false;
                pnlConfirmacionReservaEvento.Visible = false;
                lblDisponibilidadSalon.Text = "";
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
                    // Crear un DTO más simple para el dropdown
                    var clienteListItems = clientes.Select(c => new ClienteListItemDto
                    {
                        ClienteID = c.ClienteID,
                        NombreCompleto = $"{c.Apellido}, {c.Nombre} ({c.NumeroDocumento})",
                        NumeroDocumento = c.NumeroDocumento // No se usa para DisplayText pero podría ser útil
                    }).OrderBy(c => c.NombreCompleto).ToList();

                    ddlClienteEvento.DataSource = clienteListItems;
                    ddlClienteEvento.DataTextField = "NombreCompleto";
                    ddlClienteEvento.DataValueField = "ClienteID";
                    ddlClienteEvento.DataBind();
                    ddlClienteEvento.Items.Insert(0, new ListItem("-- Seleccione Cliente --", "0"));
                }
                else { ShowError($"Error cargando clientes: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando clientes: " + ex.Message); }
        }

        private async Task LoadSalonesDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                // Asumimos que /api/SalonesEvento devuelve SalonEventoListItemDto o similar
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/SalonesEvento");
                if (response.IsSuccessStatusCode)
                {
                    // La API SalonesEventoController->GetSalonesEvento ya devuelve SalonEventoViewModel,
                    // que tiene Nombre y SalonEventoID. Podemos usar eso directamente o un DTO más simple.
                    // Para este ejemplo, usaré SalonEventoListItemDto asumiendo que la API lo devuelve
                    // o que lo mapeamos desde SalonEventoViewModel.
                    var salones = JsonConvert.DeserializeObject<List<SalonEventoListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlSalonEvento.DataSource = salones;
                    ddlSalonEvento.DataTextField = "Nombre";
                    ddlSalonEvento.DataValueField = "SalonEventoID";
                    ddlSalonEvento.DataBind();
                    ddlSalonEvento.Items.Insert(0, new ListItem("-- Seleccione Salón --", "0"));
                }
                else { ShowError($"Error cargando salones: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando salones: " + ex.Message); }
        }

        private async Task LoadTiposEventoDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/TiposEvento");
                if (response.IsSuccessStatusCode)
                {
                    var tipos = JsonConvert.DeserializeObject<List<TipoEventoListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlTipoEvento.DataSource = tipos;
                    ddlTipoEvento.DataTextField = "NombreTipo";
                    ddlTipoEvento.DataValueField = "TipoEventoID";
                    ddlTipoEvento.DataBind();
                    ddlTipoEvento.Items.Insert(0, new ListItem("-- Seleccione Tipo de Evento --", "0"));
                }
                else { ShowError($"Error cargando tipos de evento: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando tipos de evento: " + ex.Message); }
        }

        private async Task LoadServiciosAdicionalesGridAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/ServiciosAdicionalesEvento");
                if (response.IsSuccessStatusCode)
                {
                    var servicios = JsonConvert.DeserializeObject<List<ServicioAdicionalEventoListItemDto>>(await response.Content.ReadAsStringAsync());
                    gvServiciosAdicionales.DataSource = servicios;
                    gvServiciosAdicionales.DataBind();
                }
                else { ShowError($"Error cargando servicios adicionales: {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando servicios adicionales: " + ex.Message); }
        }

        protected void ddlSalonEvento_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Si cambia el salón, la disponibilidad debe re-verificarse
            lblDisponibilidadSalon.Text = "";
            pnlServiciosAdicionales.Visible = false;
            pnlConfirmacionReservaEvento.Visible = false;
        }

        protected void txtFechaHora_TextChanged(object sender, EventArgs e)
        {
            // Si cambian fecha u horas, la disponibilidad debe re-verificarse
            lblDisponibilidadSalon.Text = "";
            pnlServiciosAdicionales.Visible = false;
            pnlConfirmacionReservaEvento.Visible = false;
        }

        protected async void btnVerificarDisponibilidadSalon_Click(object sender, EventArgs e)
        {
            ClearMessages();
            lblDisponibilidadSalon.Text = "";
            pnlServiciosAdicionales.Visible = false;
            pnlConfirmacionReservaEvento.Visible = false;

            if (ddlSalonEvento.SelectedValue == "0" ||
                string.IsNullOrWhiteSpace(txtFechaEvento.Text) ||
                string.IsNullOrWhiteSpace(txtHoraInicioEvento.Text) ||
                string.IsNullOrWhiteSpace(txtHoraFinEvento.Text))
            {
                ShowError("Debe seleccionar un salón, fecha y horas para verificar disponibilidad.");
                Page.Validate("EventoValFechasSalon"); // Disparar validadores de este grupo si existen
                return;
            }
            if (!Page.IsValid) return; // Para validadores del grupo "EventoValFechasSalon"

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            TimeSpan horaInicio, horaFin;
            if (!TimeSpan.TryParse(txtHoraInicioEvento.Text, out horaInicio) || !TimeSpan.TryParse(txtHoraFinEvento.Text, out horaFin))
            {
                ShowError("Formato de hora inválido."); return;
            }

            var requestDto = new SalonDisponibilidadRequestDto
            {
                SalonEventoID = Convert.ToInt32(ddlSalonEvento.SelectedValue),
                FechaEvento = Convert.ToDateTime(txtFechaEvento.Text),
                HoraInicio = horaInicio,
                HoraFin = horaFin
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/ReservasEvento/DisponibilidadSalon";
                string jsonPayload = JsonConvert.SerializeObject(requestDto);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var disponibilidad = JsonConvert.DeserializeObject<SalonDisponibleViewModel>(responseContent);
                    if (disponibilidad != null)
                    {
                        lblDisponibilidadSalon.Text = disponibilidad.Mensaje;
                        lblDisponibilidadSalon.ForeColor = disponibilidad.Disponible ? System.Drawing.Color.Green : System.Drawing.Color.Red;

                        if (disponibilidad.Disponible)
                        {
                            pnlServiciosAdicionales.Visible = true;
                            pnlConfirmacionReservaEvento.Visible = true;
                            ViewState["SalonDisponibilidadRequest"] = requestDto; // Guardar para la confirmación
                        }
                    }
                    else { ShowError("Respuesta de disponibilidad inválida."); }
                }
                else { ShowError($"Error al verificar disponibilidad: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión al verificar disponibilidad: {ex.Message}"); }
        }

        protected async void btnConfirmarReservaEvento_Click(object sender, EventArgs e)
        {
            ClearMessages();
            if (!Page.IsValid) return; // Para validadores del grupo "EventoVal"

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            var disponibilidadRequest = ViewState["SalonDisponibilidadRequest"] as SalonDisponibilidadRequestDto;
            if (disponibilidadRequest == null || disponibilidadRequest.SalonEventoID != Convert.ToInt32(ddlSalonEvento.SelectedValue) ||
                disponibilidadRequest.FechaEvento.Date != Convert.ToDateTime(txtFechaEvento.Text).Date ||
                disponibilidadRequest.HoraInicio != TimeSpan.Parse(txtHoraInicioEvento.Text) ||
                disponibilidadRequest.HoraFin != TimeSpan.Parse(txtHoraFinEvento.Text))
            {
                ShowError("Los criterios de disponibilidad han cambiado. Por favor, verifique la disponibilidad del salón nuevamente.");
                pnlServiciosAdicionales.Visible = false;
                pnlConfirmacionReservaEvento.Visible = false;
                lblDisponibilidadSalon.Text = "";
                return;
            }

            var serviciosSeleccionados = new List<ReservaEventoServicioInputDto>();
            foreach (GridViewRow row in gvServiciosAdicionales.Rows)
            {
                CheckBox chkSeleccionar = (CheckBox)row.FindControl("chkSeleccionarServicio");
                if (chkSeleccionar != null && chkSeleccionar.Checked)
                {
                    int servicioID = Convert.ToInt32(gvServiciosAdicionales.DataKeys[row.RowIndex].Value);
                    TextBox txtCantidad = (TextBox)row.FindControl("txtCantidadServicio");
                    TextBox txtPrecio = (TextBox)row.FindControl("txtPrecioAcordadoServicio");
                    TextBox txtNotas = (TextBox)row.FindControl("txtNotasServicio");

                    if (int.TryParse(txtCantidad.Text, out int cantidad) && cantidad > 0 &&
                        decimal.TryParse(txtPrecio.Text, out decimal precioAcordado))
                    {
                        serviciosSeleccionados.Add(new ReservaEventoServicioInputDto
                        {
                            ServicioAdicionalID = servicioID,
                            Cantidad = cantidad,
                            PrecioCobradoPorUnidad = precioAcordado,
                            Notas = txtNotas.Text.Trim()
                        });
                    }
                    else
                    {
                        ShowError($"Datos inválidos para el servicio ID {servicioID} seleccionado.");
                        return;
                    }
                }
            }

            var reservaData = new ReservaEventoCreacionBindingModel
            {
                ClienteID = Convert.ToInt32(ddlClienteEvento.SelectedValue),
                SalonEventoID = disponibilidadRequest.SalonEventoID,
                TipoEventoID = ddlTipoEvento.SelectedValue == "0" ? (int?)null : Convert.ToInt32(ddlTipoEvento.SelectedValue),
                NombreEvento = txtNombreEvento.Text.Trim(),
                FechaEvento = disponibilidadRequest.FechaEvento,
                HoraInicio = disponibilidadRequest.HoraInicio,
                HoraFin = disponibilidadRequest.HoraFin,
                NumeroAsistentesEstimado = Convert.ToInt32(txtNumeroAsistentesEvento.Text),
                NotasGenerales = txtNotasGeneralesEvento.Text.Trim(),
                ServiciosAdicionales = serviciosSeleccionados
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/ReservasEvento";
                string jsonPayload = JsonConvert.SerializeObject(reservaData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var reservaCreada = JsonConvert.DeserializeObject<ReservaEventoViewModel>(responseContent);
                    ShowSuccess($"¡Reserva de Evento '{reservaCreada.NombreEvento}' (ID: {reservaCreada.ReservaEventoID}) creada exitosamente!");
                    pnlServiciosAdicionales.Visible = false;
                    pnlConfirmacionReservaEvento.Visible = false;
                    lblDisponibilidadSalon.Text = "";
                    // Limpiar formulario principal
                    ddlClienteEvento.ClearSelection();
                    txtNombreEvento.Text = "";
                    // ... y otros campos ...
                }
                else
                {
                    ShowError($"Error al crear reserva de evento: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 300))}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión al crear reserva de evento: {ex.Message}"); }
        }

        protected void btnCancelarProcesoEvento_Click(object sender, EventArgs e)
        {
            pnlServiciosAdicionales.Visible = false;
            pnlConfirmacionReservaEvento.Visible = false;
            lblDisponibilidadSalon.Text = "";
            ClearMessages();
            // Aquí podrías redirigir o limpiar más campos si es necesario
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