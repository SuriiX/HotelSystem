// File: ~/Admin/GestionTiposEvento.aspx.cs
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
    public partial class GestionTiposEvento : System.Web.UI.Page
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
                pnlFormTipoEvento.Visible = false;
                btnShowAddTipoEventoForm.Visible = true;
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
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/TiposEvento");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tiposEvento = JsonConvert.DeserializeObject<List<TipoEventoViewModel>>(jsonResponse);
                    gvTiposEvento.DataSource = tiposEvento;
                    gvTiposEvento.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar tipos de evento: {response.StatusCode}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        protected void btnShowAddTipoEventoForm_Click(object sender, EventArgs e)
        {
            ClearTipoEventoForm();
            litFormTitle.Text = "Añadir Nuevo Tipo de Evento";
            hfTipoEventoID.Value = "0";
            pnlFormTipoEvento.Visible = true;
            btnShowAddTipoEventoForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSaveTipoEvento_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var tipoEventoData = new TipoEventoBindingModel
            {
                NombreTipo = txtNombreTipoEvento.Text.Trim(),
                Descripcion = txtDescripcionTipoEvento.Text.Trim()
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/TiposEvento";
            int tipoEventoID = Convert.ToInt32(hfTipoEventoID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(tipoEventoData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (tipoEventoID == 0) // Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar
                {
                    response = await client.PutAsync($"{apiUrl}/{tipoEventoID}", httpContent);
                }

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    lblSuccessMessage.Text = tipoEventoID == 0 ? "Tipo de evento creado exitosamente." : "Tipo de evento actualizado exitosamente.";
                    pnlFormTipoEvento.Visible = false;
                    btnShowAddTipoEventoForm.Visible = true;
                    await BindGridAsync();
                    ClearTipoEventoForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al guardar tipo de evento: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al guardar: {ex.Message}"; }
        }

        protected void btnCancelTipoEvento_Click(object sender, EventArgs e)
        {
            pnlFormTipoEvento.Visible = false;
            btnShowAddTipoEventoForm.Visible = true;
            ClearTipoEventoForm();
        }

        private void ClearTipoEventoForm()
        {
            hfTipoEventoID.Value = "0";
            txtNombreTipoEvento.Text = "";
            txtDescripcionTipoEvento.Text = "";
            lblMessage.Text = "";
        }

        protected async void gvTiposEvento_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int tipoEventoID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditTipoEvento")
            {
                await LoadTipoEventoForEditAsync(tipoEventoID);
            }
            else if (e.CommandName == "DeleteTipoEvento")
            {
                await DeleteTipoEventoAsync(tipoEventoID);
            }
        }

        protected void gvTiposEvento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvTiposEvento.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync));
        }

        private async Task LoadTipoEventoForEditAsync(int tipoEventoID)
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
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/TiposEvento/{tipoEventoID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tipoEvento = JsonConvert.DeserializeObject<TipoEventoViewModel>(jsonResponse);
                    if (tipoEvento != null)
                    {
                        hfTipoEventoID.Value = tipoEvento.TipoEventoID.ToString();
                        txtNombreTipoEvento.Text = tipoEvento.NombreTipo;
                        txtDescripcionTipoEvento.Text = tipoEvento.Descripcion;

                        litFormTitle.Text = "Editar Tipo de Evento";
                        pnlFormTipoEvento.Visible = true;
                        btnShowAddTipoEventoForm.Visible = false;
                    }
                }
                else { lblMessage.Text = "Error al cargar datos del tipo de evento para edición."; }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        private async Task DeleteTipoEventoAsync(int tipoEventoID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.DeleteAsync($"{_apiBaseUrl.TrimEnd('/')}/api/TiposEvento/{tipoEventoID}");
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Tipo de evento eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar tipo de evento: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al eliminar: {ex.Message}"; }

            pnlFormTipoEvento.Visible = false;
            btnShowAddTipoEventoForm.Visible = true;
            await BindGridAsync();
        }
    }
}