// File: ~/Admin/GestionEmpleados.aspx.cs
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

namespace HotelAuroraDreams.WebApp_Framework.Admin
{
    public partial class GestionEmpleados : System.Web.UI.Page
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
                await LoadCargosDropdownAsync();
                LoadSystemRolesCheckboxList(); // Para el CheckBoxList de roles
                await BindGridAsync();
                pnlEditEmpleado.Visible = false;
            }
        }

        private async Task<bool> IsUserAuthorizedAdminAsync()
        {
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
                        Session["UserRoles"] = userInfo.Roles; return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private async Task LoadHotelesDropdownAsync()
        {
            // Similar a GestionHabitaciones.aspx.cs
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Hoteles");
                if (response.IsSuccessStatusCode)
                {
                    var hoteles = JsonConvert.DeserializeObject<List<HotelSimpleViewModel>>(await response.Content.ReadAsStringAsync());
                    ddlHotelEmpleado.DataSource = hoteles;
                    ddlHotelEmpleado.DataTextField = "Nombre";
                    ddlHotelEmpleado.DataValueField = "HotelID";
                    ddlHotelEmpleado.DataBind();
                    ddlHotelEmpleado.Items.Insert(0, new ListItem("-- Sin Asignar --", "")); // Usar "" para valor nulo
                }
                else { lblMessage.Text += " Error cargando hoteles."; }
            }
            catch (Exception ex) { lblMessage.Text += " Excepción cargando hoteles: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 50)); }
        }

        private async Task LoadCargosDropdownAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Cargos");
                if (response.IsSuccessStatusCode)
                {
                    var cargos = JsonConvert.DeserializeObject<List<CargoListItemDto>>(await response.Content.ReadAsStringAsync());
                    ddlCargoEmpleado.DataSource = cargos;
                    ddlCargoEmpleado.DataTextField = "NombreCargo";
                    ddlCargoEmpleado.DataValueField = "CargoID";
                    ddlCargoEmpleado.DataBind();
                    ddlCargoEmpleado.Items.Insert(0, new ListItem("-- Sin Asignar --", ""));
                }
                else { lblMessage.Text += " Error cargando cargos."; }
            }
            catch (Exception ex) { lblMessage.Text += " Excepción cargando cargos: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 50)); }
        }
        private void LoadSystemRolesCheckboxList()
        {

            var rolesDelSistema = new List<string> { "Administrador", "Empleado" };

            cblRolesEmpleado.Items.Clear();
            foreach (var roleName in rolesDelSistema)
            {
                cblRolesEmpleado.Items.Add(new ListItem(roleName, roleName));
            }
        }


        private async Task BindGridAsync()
        {
            lblMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) return;
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Empleados");
                if (response.IsSuccessStatusCode)
                {
                    var empleados = JsonConvert.DeserializeObject<List<EmpleadoViewModel>>(await response.Content.ReadAsStringAsync());
                    gvEmpleados.DataSource = empleados;
                    gvEmpleados.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar empleados: {response.StatusCode}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        protected async void btnSaveEmpleado_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (string.IsNullOrEmpty(hfEmpleadoID.Value)) { lblMessage.Text = "ID de empleado no especificado."; return; }

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            List<string> selectedRoles = new List<string>();
            foreach (ListItem item in cblRolesEmpleado.Items)
            {
                if (item.Selected)
                {
                    selectedRoles.Add(item.Value);
                }
            }

            var empleadoData = new EmpleadoUpdateBindingModel
            {
                Nombre = txtNombreEmpleado.Text.Trim(),
                Apellido = txtApellidoEmpleado.Text.Trim(),
                PhoneNumber = txtTelefonoEmpleado.Text.Trim(),
                HotelID = string.IsNullOrEmpty(ddlHotelEmpleado.SelectedValue) ? (int?)null : Convert.ToInt32(ddlHotelEmpleado.SelectedValue),
                CargoID = string.IsNullOrEmpty(ddlCargoEmpleado.SelectedValue) ? (int?)null : Convert.ToInt32(ddlCargoEmpleado.SelectedValue),
                Salario = decimal.Parse(txtSalarioEmpleado.Text), // Añadir validación TryParse
                Estado = ddlEstadoEmpleado.SelectedValue,
                Roles = selectedRoles,
                // Otros campos como TipoDocumento, NumeroDocumento, Direccion, FechaNacimiento, FechaContratacion
                // necesitarían sus propios TextBoxes y ser asignados aquí si se editan.
                // Por ahora, estos son los principales.
                FechaContratacion = DateTime.Now // Asumir que no se edita o pasar el valor original
            };

            string empleadoID = hfEmpleadoID.Value;
            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Empleados/{empleadoID}";

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(empleadoData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Empleado actualizado exitosamente.";
                    pnlEditEmpleado.Visible = false;
                    await BindGridAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al actualizar empleado: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al actualizar: {ex.Message}"; }
        }

        protected void btnCancelEditEmpleado_Click(object sender, EventArgs e)
        {
            pnlEditEmpleado.Visible = false;
            ClearEmpleadoForm();
        }

        private void ClearEmpleadoForm()
        {
            hfEmpleadoID.Value = "";
            lblEmailDisplay.Text = "";
            txtNombreEmpleado.Text = "";
            txtApellidoEmpleado.Text = "";
            txtTelefonoEmpleado.Text = "";
            ddlHotelEmpleado.ClearSelection();
            ddlCargoEmpleado.ClearSelection();
            txtSalarioEmpleado.Text = "";
            ddlEstadoEmpleado.ClearSelection();
            cblRolesEmpleado.ClearSelection();
            lblMessage.Text = "";
            // lblSuccessMessage.Text = "";
        }

        protected async void gvEmpleados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string empleadoID = e.CommandArgument.ToString();
            if (e.CommandName == "EditEmpleado")
            {
                await LoadEmpleadoForEditAsync(empleadoID);
            }
            else if (e.CommandName == "DeleteEmpleado")
            {
                await DeleteEmpleadoAsync(empleadoID);
            }
        }

        protected void gvEmpleados_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEmpleados.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync));
        }

        private async Task LoadEmpleadoForEditAsync(string empleadoID)
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
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Empleados/{empleadoID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var empleado = JsonConvert.DeserializeObject<EmpleadoViewModel>(jsonResponse);
                    if (empleado != null)
                    {
                        hfEmpleadoID.Value = empleado.Id;
                        litFormTitle.Text = $"Editar Empleado: {empleado.Nombre} {empleado.Apellido}";
                        lblEmailDisplay.Text = $"Editando: {empleado.Email}";
                        txtNombreEmpleado.Text = empleado.Nombre;
                        txtApellidoEmpleado.Text = empleado.Apellido;
                        txtTelefonoEmpleado.Text = empleado.PhoneNumber;

                        ddlHotelEmpleado.ClearSelection();
                        if (empleado.HotelID.HasValue && ddlHotelEmpleado.Items.FindByValue(empleado.HotelID.Value.ToString()) != null)
                            ddlHotelEmpleado.SelectedValue = empleado.HotelID.Value.ToString();
                        else if (ddlHotelEmpleado.Items.FindByValue("") != null)
                            ddlHotelEmpleado.SelectedValue = "";


                        ddlCargoEmpleado.ClearSelection();
                        if (empleado.CargoID.HasValue && ddlCargoEmpleado.Items.FindByValue(empleado.CargoID.Value.ToString()) != null)
                            ddlCargoEmpleado.SelectedValue = empleado.CargoID.Value.ToString();
                        else if (ddlCargoEmpleado.Items.FindByValue("") != null)
                            ddlCargoEmpleado.SelectedValue = "";


                        txtSalarioEmpleado.Text = empleado.Salario.ToString("F2");
                        ddlEstadoEmpleado.SelectedValue = empleado.Estado;

                        cblRolesEmpleado.ClearSelection();
                        if (empleado.Roles != null)
                        {
                            foreach (ListItem item in cblRolesEmpleado.Items)
                            {
                                item.Selected = empleado.Roles.Contains(item.Value);
                            }
                        }

                        pnlEditEmpleado.Visible = true;
                    }
                }
                else
                {
                    lblMessage.Text = "Error al cargar datos del empleado para edición.";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        private async Task DeleteEmpleadoAsync(string empleadoID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            try
            {
                HttpResponseMessage response = await client.DeleteAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Empleados/{empleadoID}");
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Empleado eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar empleado: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al eliminar empleado: {ex.Message}"; }

            pnlEditEmpleado.Visible = false;
            await BindGridAsync();
        }
    }
}