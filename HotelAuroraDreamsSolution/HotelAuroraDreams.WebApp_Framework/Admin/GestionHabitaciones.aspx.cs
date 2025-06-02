// File: ~/Admin/GestionHabitaciones.aspx.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HotelAuroraDreams.WebApp_Framework.Models;
using Newtonsoft.Json;

namespace HotelAuroraDreams.WebApp_Framework.Admin
{
    public partial class GestionHabitaciones : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(Page_Load_Async));
        }

        private async Task Page_Load_Async()
        {
            if (!await IsUserAuthorizedAdminAsync())
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true);
                return;
            }

            if (!IsPostBack)
            {
                await LoadHotelesDropdownAsync();
                await LoadTiposHabitacionDropdownAsync();
                await BindGridAsync();
                pnlFormHabitacion.Visible = false;
                btnShowAddHabitacionForm.Visible = true;
            }
        }

        private async Task<bool> IsUserAuthorizedAdminAsync()
        {
            // Reutilizar la lógica de IsUserAuthorizedAdminAsync de Default.aspx.cs o Site.Master.cs
            // o crear una clase base con este método.
            // Por brevedad, asumimos que está implementado similar a como lo hicimos antes.
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value)) return false;

            var userRoles = Session["UserRoles"] as IList<string>;
            if (userRoles != null && userRoles.Contains("Administrador")) return true;

            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                var response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Account/UserInfo");
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = JsonConvert.DeserializeObject<UserInfoViewModel>(await response.Content.ReadAsStringAsync());
                    if (userInfo != null && userInfo.Roles != null && userInfo.Roles.Contains("Administrador"))
                    {
                        Session["UserRoles"] = userInfo.Roles;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private async Task LoadHotelesDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                // Asumimos que tienes un endpoint GET /api/hoteles en tu API que devuelve una lista de HotelSimpleViewModel
                // Si no, necesitas crear ese endpoint o adaptar para usar el endpoint existente y extraer ID/Nombre.
                // Por ahora, asumiré que la API tiene un HotelController con un GetHoteles() que devuelve HotelSimpleViewModel
                // o algo que podamos adaptar aquí. Para este ejemplo, adaptaré la respuesta de un GET /api/hoteles completo.
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Hoteles"); // Necesitas un HotelesController
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // Asumimos que el endpoint /api/Hoteles devuelve objetos con al menos hotel_id y nombre
                    var hoteles = JsonConvert.DeserializeObject<List<HotelSimpleViewModel>>(jsonResponse);
                    ddlHotel.DataSource = hoteles;
                    ddlHotel.DataTextField = "Nombre"; // Nombre de la propiedad en HotelSimpleViewModel
                    ddlHotel.DataValueField = "HotelID"; // Nombre de la propiedad en HotelSimpleViewModel
                    ddlHotel.DataBind();
                    ddlHotel.Items.Insert(0, new ListItem("-- Seleccione Hotel --", "0"));
                }
            }
            catch (Exception ex) { lblMessage.Text = "Error cargando hoteles: " + ex.Message; }
        }

        private async Task LoadTiposHabitacionDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/TiposHabitacion");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tipos = JsonConvert.DeserializeObject<List<TipoHabitacionSimpleViewModel>>(jsonResponse);
                    ddlTipoHabitacion.DataSource = tipos;
                    ddlTipoHabitacion.DataTextField = "nombre";
                    ddlTipoHabitacion.DataValueField = "tipo_habitacion_id";
                    ddlTipoHabitacion.DataBind();
                    ddlTipoHabitacion.Items.Insert(0, new ListItem("-- Seleccione Tipo --", "0"));
                }
            }
            catch (Exception ex) { lblMessage.Text = "Error cargando tipos de habitación: " + ex.Message; }
        }


        private async Task BindGridAsync()
        {
            lblMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { /* Manejar no autenticado */ return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Habitaciones");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var habitaciones = JsonConvert.DeserializeObject<List<HabitacionViewModel>>(jsonResponse);
                    gvHabitaciones.DataSource = habitaciones;
                    gvHabitaciones.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar habitaciones: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        protected void btnShowAddHabitacionForm_Click(object sender, EventArgs e)
        {
            ClearHabitacionForm();
            litFormTitle.Text = "Añadir Nueva Habitación";
            hfHabitacionID.Value = "0";
            pnlFormHabitacion.Visible = true;
            btnShowAddHabitacionForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSaveHabitacion_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var habitacionData = new HabitacionBindingModel
            {
                HotelID = Convert.ToInt32(ddlHotel.SelectedValue),
                TipoHabitacionID = Convert.ToInt32(ddlTipoHabitacion.SelectedValue),
                Numero = txtNumeroHabitacion.Text.Trim(),
                Piso = Convert.ToInt32(txtPisoHabitacion.Text),
                Estado = ddlEstadoHabitacion.SelectedValue,
                Vista = txtVistaHabitacion.Text.Trim(),
                Descripcion = txtDescripcionHabitacion.Text.Trim()
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/Habitaciones";
            int habitacionID = Convert.ToInt32(hfHabitacionID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(habitacionData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (habitacionID == 0) // Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar
                {
                    response = await client.PutAsync($"{apiUrl}/{habitacionID}", httpContent);
                }

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Created)
                {
                    lblSuccessMessage.Text = habitacionID == 0 ? "Habitación creada exitosamente." : "Habitación actualizada exitosamente.";
                    pnlFormHabitacion.Visible = false;
                    btnShowAddHabitacionForm.Visible = true;
                    await BindGridAsync();
                    ClearHabitacionForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al guardar habitación: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al guardar habitación: {ex.Message}";
            }
        }

        protected void btnCancelHabitacion_Click(object sender, EventArgs e)
        {
            pnlFormHabitacion.Visible = false;
            btnShowAddHabitacionForm.Visible = true;
            ClearHabitacionForm();
        }

        private void ClearHabitacionForm()
        {
            hfHabitacionID.Value = "0";
            ddlHotel.ClearSelection();
            ddlTipoHabitacion.ClearSelection();
            txtNumeroHabitacion.Text = "";
            txtPisoHabitacion.Text = "";
            ddlEstadoHabitacion.SelectedValue = "disponible";
            txtVistaHabitacion.Text = "";
            txtDescripcionHabitacion.Text = "";
            lblMessage.Text = "";
        }

        protected async void gvHabitaciones_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int habitacionID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditHabitacion")
            {
                await LoadHabitacionForEditAsync(habitacionID);
            }
            else if (e.CommandName == "DeleteHabitacion")
            {
                await DeleteHabitacionAsync(habitacionID);
            }
        }

        protected void gvHabitaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHabitaciones.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync));
        }

        private async Task LoadHabitacionForEditAsync(int habitacionID)
        {
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Habitaciones/{habitacionID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var habitacion = JsonConvert.DeserializeObject<HabitacionViewModel>(jsonResponse); // Usa el ViewModel completo
                    if (habitacion != null)
                    {
                        hfHabitacionID.Value = habitacion.HabitacionID.ToString();
                        // Asegurar que los DropDownLists se carguen primero si no es PostBack general
                        // await LoadHotelesDropdownAsync(); // Podría ser redundante si ya se cargaron en Page_Load
                        // await LoadTiposHabitacionDropdownAsync();

                        ddlHotel.SelectedValue = habitacion.HotelID.ToString();
                        ddlTipoHabitacion.SelectedValue = habitacion.TipoHabitacionID.ToString();
                        txtNumeroHabitacion.Text = habitacion.Numero;
                        txtPisoHabitacion.Text = habitacion.Piso.ToString();
                        ddlEstadoHabitacion.SelectedValue = habitacion.Estado;
                        txtVistaHabitacion.Text = habitacion.Vista;
                        txtDescripcionHabitacion.Text = habitacion.Descripcion;

                        litFormTitle.Text = "Editar Habitación";
                        pnlFormHabitacion.Visible = true;
                        btnShowAddHabitacionForm.Visible = false;
                    }
                }
                else
                {
                    lblMessage.Text = "Error al cargar datos de la habitación para edición.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        private async Task DeleteHabitacionAsync(int habitacionID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Habitaciones/{habitacionID}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Habitación eliminada exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar habitación: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al eliminar habitación: {ex.Message}";
            }

            pnlFormHabitacion.Visible = false;
            btnShowAddHabitacionForm.Visible = true;
            await BindGridAsync();
        }
    }
}