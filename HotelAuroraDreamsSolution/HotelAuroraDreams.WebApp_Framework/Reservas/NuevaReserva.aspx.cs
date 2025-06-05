// File: ~/Reservas/NuevaReserva.aspx.cs
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

namespace HotelAuroraDreams.WebApp_Framework.Reservas
{
    public partial class NuevaReserva : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();
        protected List<HabitacionDisponibleParaSeleccion> HabitacionesParaSeleccion { get; set; }

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
                await LoadHotelesDropdownAsync();
                await LoadTiposHabitacionDropdownAsync();
                pnlResultadosDisponibilidad.Visible = false;
                pnlDetallesReserva.Visible = false;
            }
        }

        private async Task<bool> IsUserAuthorizedAsync() // Asegúrate que esta lógica sea correcta
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

        private async Task LoadHotelesDropdownAsync()
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
                    ddlHotelBusqueda.DataSource = hoteles;
                    ddlHotelBusqueda.DataTextField = "Nombre";
                    ddlHotelBusqueda.DataValueField = "HotelID";
                    ddlHotelBusqueda.DataBind();
                    ddlHotelBusqueda.Items.Insert(0, new ListItem("-- Seleccione Hotel --", "0"));
                }
                else { ShowError($"Error cargando hoteles (DDL): {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando hoteles (DDL): " + ex.Message); }
        }

        private async Task LoadTiposHabitacionDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            SetAuthorizationHeader(authTokenCookie.Value);
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/TiposHabitacion");
                if (response.IsSuccessStatusCode)
                {
                    var tipos = JsonConvert.DeserializeObject<List<TipoHabitacionSimpleViewModel>>(await response.Content.ReadAsStringAsync());
                    ddlTipoHabitacionBusqueda.DataSource = tipos;
                    ddlTipoHabitacionBusqueda.DataTextField = "nombre";
                    ddlTipoHabitacionBusqueda.DataValueField = "tipo_habitacion_id";
                    ddlTipoHabitacionBusqueda.DataBind();
                    ddlTipoHabitacionBusqueda.Items.Insert(0, new ListItem("-- Todos los Tipos --", "0"));
                }
                else { ShowError($"Error cargando tipos de habitación (DDL): {response.StatusCode}"); }
            }
            catch (Exception ex) { ShowError("Excepción cargando tipos habitación (DDL): " + ex.Message); }
        }

        // ***** NUEVO MÉTODO PARA BUSCAR CLIENTES *****
        protected async void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            ClearMessages();
            string terminoBusqueda = txtBusquedaCliente.Text.Trim();
            if (string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                ShowError("Ingrese un término para buscar el cliente.");
                return;
            }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Clientes/Buscar?terminoBusqueda={HttpUtility.UrlEncode(terminoBusqueda)}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var clientesEncontrados = JsonConvert.DeserializeObject<List<ClienteListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlResultadosCliente.DataSource = clientesEncontrados;
                    ddlResultadosCliente.DataTextField = "NombreCompleto"; // Asegúrate que ClienteListItemDto tenga esta propiedad
                    ddlResultadosCliente.DataValueField = "ClienteID";
                    ddlResultadosCliente.DataBind();
                    if (!clientesEncontrados.Any())
                    {
                        ddlResultadosCliente.Items.Insert(0, new ListItem("-- No se encontraron clientes --", "0"));
                        ShowError("No se encontraron clientes con ese criterio. Puede crearlo en la sección de Gestión de Clientes.");
                    }
                    else
                    {
                        ddlResultadosCliente.Items.Insert(0, new ListItem("-- Seleccione un Cliente --", "0"));
                    }
                }
                else
                {
                    ShowError($"Error al buscar clientes: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error de conexión al buscar clientes: {ex.Message}");
            }
        }
        // **********************************************

        protected async void btnVerificarDisponibilidad_Click(object sender, EventArgs e)
        {
            // ... (código existente, sin cambios) ...
            // Solo asegúrate de llamar a ClearMessages() al inicio
            ClearMessages();
            pnlResultadosDisponibilidad.Visible = false;
            pnlDetallesReserva.Visible = false; // Ocultar esto también hasta que se seleccionen habitaciones
            cblHabitacionesDisponibles.Items.Clear();
            lblNoDisponibilidad.Visible = false;

            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                ShowError("No autenticado o sesión expirada.");
                return;
            }
            SetAuthorizationHeader(authTokenCookie.Value);

            var requestDto = new DisponibilidadRequestDto
            {
                HotelID = Convert.ToInt32(ddlHotelBusqueda.SelectedValue),
                FechaEntrada = Convert.ToDateTime(txtFechaEntrada.Text),
                FechaSalida = Convert.ToDateTime(txtFechaSalida.Text),
                NumeroHuespedes = Convert.ToInt32(txtNumeroHuespedesBusqueda.Text),
                TipoHabitacionID = ddlTipoHabitacionBusqueda.SelectedValue == "0" ? (int?)null : Convert.ToInt32(ddlTipoHabitacionBusqueda.SelectedValue)
            };

            ViewState["DisponibilidadRequest"] = requestDto;

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/Reservas/Disponibilidad";
                string jsonPayload = JsonConvert.SerializeObject(requestDto);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var habitaciones = JsonConvert.DeserializeObject<List<HabitacionDisponibleDto>>(responseContent);
                    if (habitaciones != null && habitaciones.Any())
                    {
                        HabitacionesParaSeleccion = habitaciones.Select(h => new HabitacionDisponibleParaSeleccion
                        {
                            HabitacionID = h.HabitacionID,
                            DisplayText = $"N°: {h.NumeroHabitacion} ({h.NombreTipoHabitacion}) - Cap: {h.Capacidad} - Precio: {h.PrecioNoche:C}"
                        }).ToList();

                        cblHabitacionesDisponibles.DataSource = HabitacionesParaSeleccion;
                        cblHabitacionesDisponibles.DataTextField = "DisplayText";
                        cblHabitacionesDisponibles.DataValueField = "HabitacionID";
                        cblHabitacionesDisponibles.DataBind();

                        pnlResultadosDisponibilidad.Visible = true;
                        pnlDetallesReserva.Visible = true;
                    }
                    else
                    {
                        lblNoDisponibilidad.Visible = true;
                        pnlResultadosDisponibilidad.Visible = true;
                    }
                }
                else { ShowError($"Error al verificar disponibilidad: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 200))}"); }
            }
            catch (Exception ex) { ShowError($"Error de conexión al verificar disponibilidad: {ex.Message}"); }
        }

        protected async void btnConfirmarReserva_Click(object sender, EventArgs e)
        {
            ClearMessages();
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                ShowError("No autenticado o sesión expirada.");
                return;
            }
            SetAuthorizationHeader(authTokenCookie.Value);

            var disponibilidadRequest = ViewState["DisponibilidadRequest"] as DisponibilidadRequestDto;
            if (disponibilidadRequest == null)
            {
                ShowError("Criterios de búsqueda no encontrados. Verifique disponibilidad de nuevo.");
                return;
            }

            List<int> habitacionesSeleccionadasIDs = new List<int>();
            foreach (ListItem item in cblHabitacionesDisponibles.Items)
            {
                if (item.Selected)
                {
                    habitacionesSeleccionadasIDs.Add(Convert.ToInt32(item.Value));
                }
            }

            if (!habitacionesSeleccionadasIDs.Any())
            {
                ShowError("Debe seleccionar al menos una habitación.");
                pnlResultadosDisponibilidad.Visible = true;
                pnlDetallesReserva.Visible = true;
                return;
            }
            // ***** MODIFICACIÓN: Obtener ClienteID del DropDownList *****
            if (ddlResultadosCliente.SelectedValue == "0" || string.IsNullOrWhiteSpace(ddlResultadosCliente.SelectedValue))
            {
                ShowError("Debe buscar y seleccionar un cliente.");
                pnlResultadosDisponibilidad.Visible = true;
                pnlDetallesReserva.Visible = true;
                return;
            }
            int clienteIdSeleccionado = Convert.ToInt32(ddlResultadosCliente.SelectedValue);
            // **********************************************************

            var reservaData = new ReservaCreacionBindingModel
            {
                ClienteID = clienteIdSeleccionado, // <--- Usar el ID del cliente seleccionado
                HotelID = disponibilidadRequest.HotelID,
                FechaEntrada = disponibilidadRequest.FechaEntrada,
                FechaSalida = disponibilidadRequest.FechaSalida,
                NumeroHuespedes = disponibilidadRequest.NumeroHuespedes,
                HabitacionIDsSeleccionadas = habitacionesSeleccionadasIDs,
                Notas = txtNotasReserva.Text.Trim()
            };

            try
            {
                string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/Reservas";
                string jsonPayload = JsonConvert.SerializeObject(reservaData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var reservaCreada = JsonConvert.DeserializeObject<ReservaViewModel>(responseContent);
                    ShowSuccess($"¡Reserva #{reservaCreada.ReservaID} creada exitosamente para {reservaCreada.NombreCliente}!");
                    pnlResultadosDisponibilidad.Visible = false;
                    pnlDetallesReserva.Visible = false;
                    cblHabitacionesDisponibles.Items.Clear();
                    txtBusquedaCliente.Text = ""; // Limpiar campo de búsqueda de cliente
                    ddlResultadosCliente.Items.Clear();
                    ddlResultadosCliente.Items.Insert(0, new ListItem("-- Busque y seleccione un cliente --", "0"));
                    txtNotasReserva.Text = "";
                    ViewState.Remove("DisponibilidadRequest"); // Limpiar el estado
                }
                else
                {
                    ShowError($"Error al crear la reserva: {response.StatusCode} - {responseContent.Substring(0, Math.Min(responseContent.Length, 300))}");
                    pnlResultadosDisponibilidad.Visible = true;
                    pnlDetallesReserva.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error de conexión al crear reserva: {ex.Message}");
                pnlResultadosDisponibilidad.Visible = true;
                pnlDetallesReserva.Visible = true;
            }
        }

        protected void btnCancelarProceso_Click(object sender, EventArgs e)
        {
            // ... (código existente, sin cambios) ...
            pnlResultadosDisponibilidad.Visible = false;
            pnlDetallesReserva.Visible = false;
            cblHabitacionesDisponibles.Items.Clear();
            txtBusquedaCliente.Text = "";
            ddlResultadosCliente.Items.Clear();
            ddlResultadosCliente.Items.Insert(0, new ListItem("-- Busque y seleccione un cliente --", "0"));
            txtNotasReserva.Text = "";
            ClearMessages();
            ViewState.Remove("DisponibilidadRequest");
        }

        private void ClearMessages() { lblMessage.Text = ""; lblSuccessMessage.Text = ""; }
        private void ShowError(string message) { lblMessage.Text = message; lblSuccessMessage.Text = ""; }
        private void ShowSuccess(string message) { lblSuccessMessage.Text = message; lblMessage.Text = ""; }
    }

    // Clase auxiliar para el CheckBoxList (ya la tenías, asegúrate que esté definida)
    // public class HabitacionDisponibleParaSeleccion { public int HabitacionID { get; set; } public string DisplayText { get; set; } }
}