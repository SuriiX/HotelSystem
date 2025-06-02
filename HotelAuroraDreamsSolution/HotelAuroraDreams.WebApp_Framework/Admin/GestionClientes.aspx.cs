// File: ~/Admin/GestionClientes.aspx.cs
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
    public partial class GestionClientes : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(Page_Load_Async));
        }

        private async Task Page_Load_Async()
        {
            if (!await IsUserAuthorizedAsync()) // Podrías tener un IsUserAuthorizedAdminAsync si es solo para admin
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true);
                return;
            }

            if (!IsPostBack)
            {
                await BindGridAsync();
                pnlFormCliente.Visible = false;
                btnShowAddClienteForm.Visible = true;
            }
        }

        // Reutiliza o adapta IsUserAuthorizedAdminAsync de Default.aspx.cs o Site.Master.cs
        // o crea una clase base para páginas protegidas.
        private async Task<bool> IsUserAuthorizedAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value)) return false;

            // Simplificado: Asume que cualquier usuario logueado puede ver, roles para CUD se validan en API.
            // Para mayor seguridad, verifica el rol aquí también si es necesario.
            var userRoles = Session["UserRoles"] as IList<string>;
            if (userRoles != null && (userRoles.Contains("Administrador") || userRoles.Contains("Empleado")))
            {
                return true;
            }
            // Si no, intenta obtener info de la API
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
                    if (userInfo != null && userInfo.Roles != null && (userInfo.Roles.Contains("Administrador") || userInfo.Roles.Contains("Empleado")))
                    {
                        Session["UserRoles"] = userInfo.Roles;
                        return true;
                    }
                }
            }
            catch { }
            return false;
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
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Clientes");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var clientes = JsonConvert.DeserializeObject<List<ClienteViewModel>>(jsonResponse);
                    gvClientes.DataSource = clientes;
                    gvClientes.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar clientes: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        protected void btnShowAddClienteForm_Click(object sender, EventArgs e)
        {
            ClearClienteForm();
            litFormTitle.Text = "Añadir Nuevo Cliente";
            hfClienteID.Value = "0";
            pnlFormCliente.Visible = true;
            btnShowAddClienteForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSaveCliente_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var clienteData = new ClienteBindingModel
            {
                Nombre = txtNombreCliente.Text.Trim(),
                Apellido = txtApellidoCliente.Text.Trim(),
                TipoDocumento = ddlTipoDocumento.SelectedValue,
                NumeroDocumento = txtNumeroDocumento.Text.Trim(),
                Email = txtEmailCliente.Text.Trim(),
                Telefono = txtTelefonoCliente.Text.Trim(),
                Direccion = txtDireccionCliente.Text.Trim(),
                CiudadResidenciaID = string.IsNullOrWhiteSpace(txtCiudadResidenciaID.Text) ? (int?)null : int.Parse(txtCiudadResidenciaID.Text),
                FechaNacimiento = string.IsNullOrWhiteSpace(txtFechaNacimiento.Text) ? (DateTime?)null : DateTime.Parse(txtFechaNacimiento.Text)
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/Clientes";
            int clienteID = Convert.ToInt32(hfClienteID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(clienteData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (clienteID == 0) // Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar
                {
                    response = await client.PutAsync($"{apiUrl}/{clienteID}", httpContent);
                }

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
                {
                    lblSuccessMessage.Text = clienteID == 0 ? "Cliente creado exitosamente." : "Cliente actualizado exitosamente.";
                    pnlFormCliente.Visible = false;
                    btnShowAddClienteForm.Visible = true;
                    await BindGridAsync();
                    ClearClienteForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Manejar errores de ModelState o errores generales de la API
                    lblMessage.Text = $"Error al guardar: {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al guardar cliente: {ex.Message}";
            }
        }

        protected void btnCancelCliente_Click(object sender, EventArgs e)
        {
            pnlFormCliente.Visible = false;
            btnShowAddClienteForm.Visible = true;
            ClearClienteForm();
        }

        private void ClearClienteForm()
        {
            hfClienteID.Value = "0";
            txtNombreCliente.Text = "";
            txtApellidoCliente.Text = "";
            ddlTipoDocumento.ClearSelection();
            txtNumeroDocumento.Text = "";
            txtEmailCliente.Text = "";
            txtTelefonoCliente.Text = "";
            txtDireccionCliente.Text = "";
            txtCiudadResidenciaID.Text = "";
            txtFechaNacimiento.Text = "";
            lblMessage.Text = "";
            // lblSuccessMessage.Text = ""; // No limpiar aquí para que se vea el mensaje de éxito
        }

        protected async void gvClientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditCliente")
            {
                int clienteID = Convert.ToInt32(e.CommandArgument);
                await LoadClienteForEditAsync(clienteID);
            }
            else if (e.CommandName == "DeleteCliente")
            {
                int clienteID = Convert.ToInt32(e.CommandArgument);
                await DeleteClienteAsync(clienteID);
            }
        }

        protected void gvClientes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClientes.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync)); // Asegurar que el databind sea async
        }

        private async Task LoadClienteForEditAsync(int clienteID)
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
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/Clientes/{clienteID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var cliente = JsonConvert.DeserializeObject<ClienteViewModel>(jsonResponse);
                    if (cliente != null)
                    {
                        hfClienteID.Value = cliente.ClienteID.ToString();
                        txtNombreCliente.Text = cliente.Nombre;
                        txtApellidoCliente.Text = cliente.Apellido;
                        ddlTipoDocumento.SelectedValue = cliente.TipoDocumento;
                        txtNumeroDocumento.Text = cliente.NumeroDocumento;
                        txtEmailCliente.Text = cliente.Email;
                        txtTelefonoCliente.Text = cliente.Telefono;
                        txtDireccionCliente.Text = cliente.Direccion;
                        txtCiudadResidenciaID.Text = cliente.CiudadResidenciaID?.ToString();
                        txtFechaNacimiento.Text = cliente.FechaNacimiento.HasValue ? cliente.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "";

                        litFormTitle.Text = "Editar Cliente";
                        pnlFormCliente.Visible = true;
                        btnShowAddClienteForm.Visible = false;
                    }
                }
                else
                {
                    lblMessage.Text = "Error al cargar datos del cliente para edición.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        private async Task DeleteClienteAsync(int clienteID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Clientes/{clienteID}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Cliente eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar cliente: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al eliminar cliente: {ex.Message}";
            }

            pnlFormCliente.Visible = false;
            btnShowAddClienteForm.Visible = true;
            await BindGridAsync(); // Recargar el grid
        }
    }
}