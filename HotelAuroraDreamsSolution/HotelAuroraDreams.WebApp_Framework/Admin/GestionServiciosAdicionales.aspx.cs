// File: ~/Admin/GestionServiciosAdicionales.aspx.cs
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
    public partial class GestionServiciosAdicionales : System.Web.UI.Page
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
                await BindGridAsync();
                pnlFormServicio.Visible = false;
                btnShowAddServicioForm.Visible = true;
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

        private async Task BindGridAsync()
        {
            lblMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/ServiciosAdicionalesEvento");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var servicios = JsonConvert.DeserializeObject<List<ServicioAdicionalEventoViewModel>>(jsonResponse);
                    gvServiciosAdicionales.DataSource = servicios;
                    gvServiciosAdicionales.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar servicios adicionales: {response.StatusCode}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        protected void btnShowAddServicioForm_Click(object sender, EventArgs e)
        {
            ClearServicioForm();
            litFormTitle.Text = "Añadir Nuevo Servicio Adicional";
            hfServicioID.Value = "0";
            pnlFormServicio.Visible = true;
            btnShowAddServicioForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSaveServicio_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var servicioData = new ServicioAdicionalEventoBindingModel
            {
                NombreServicio = txtNombreServicio.Text.Trim(),
                Descripcion = txtDescripcionServicio.Text.Trim(),
                PrecioBase = decimal.Parse(txtPrecioBaseServicio.Text), // Añadir TryParse y validación
                RequierePersonalPago = chkRequierePersonal.Checked
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/ServiciosAdicionalesEvento";
            int servicioID = Convert.ToInt32(hfServicioID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(servicioData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (servicioID == 0) // Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar
                {
                    response = await client.PutAsync($"{apiUrl}/{servicioID}", httpContent);
                }

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    lblSuccessMessage.Text = servicioID == 0 ? "Servicio creado exitosamente." : "Servicio actualizado exitosamente.";
                    pnlFormServicio.Visible = false;
                    btnShowAddServicioForm.Visible = true;
                    await BindGridAsync();
                    ClearServicioForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al guardar servicio: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al guardar: {ex.Message}"; }
        }

        protected void btnCancelServicio_Click(object sender, EventArgs e)
        {
            pnlFormServicio.Visible = false;
            btnShowAddServicioForm.Visible = true;
            ClearServicioForm();
        }

        private void ClearServicioForm()
        {
            hfServicioID.Value = "0";
            txtNombreServicio.Text = "";
            txtDescripcionServicio.Text = "";
            txtPrecioBaseServicio.Text = "";
            chkRequierePersonal.Checked = false;
            lblMessage.Text = "";
        }

        protected async void gvServiciosAdicionales_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int servicioID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditServicio")
            {
                await LoadServicioForEditAsync(servicioID);
            }
            else if (e.CommandName == "DeleteServicio")
            {
                await DeleteServicioAsync(servicioID);
            }
        }

        protected void gvServiciosAdicionales_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvServiciosAdicionales.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync));
        }

        private async Task LoadServicioForEditAsync(int servicioID)
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
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ServiciosAdicionalesEvento/{servicioID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var servicio = JsonConvert.DeserializeObject<ServicioAdicionalEventoViewModel>(jsonResponse);
                    if (servicio != null)
                    {
                        hfServicioID.Value = servicio.ServicioAdicionalID.ToString();
                        txtNombreServicio.Text = servicio.NombreServicio;
                        txtDescripcionServicio.Text = servicio.Descripcion;
                        txtPrecioBaseServicio.Text = servicio.PrecioBase.ToString("F2");
                        chkRequierePersonal.Checked = servicio.RequierePersonalPago;

                        litFormTitle.Text = "Editar Servicio Adicional";
                        pnlFormServicio.Visible = true;
                        btnShowAddServicioForm.Visible = false;
                    }
                }
                else { lblMessage.Text = "Error al cargar datos del servicio para edición."; }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        private async Task DeleteServicioAsync(int servicioID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.DeleteAsync($"{_apiBaseUrl.TrimEnd('/')}/api/ServiciosAdicionalesEvento/{servicioID}");
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Servicio adicional eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar servicio: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al eliminar: {ex.Message}"; }

            pnlFormServicio.Visible = false;
            btnShowAddServicioForm.Visible = true;
            await BindGridAsync();
        }
    }
}