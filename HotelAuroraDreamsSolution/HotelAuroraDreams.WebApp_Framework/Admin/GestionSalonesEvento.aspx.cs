// File: ~/Admin/GestionSalonesEvento.aspx.cs
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
    public partial class GestionSalonesEvento : System.Web.UI.Page
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
                await LoadHotelesDropdownAsync(); // Necesario para el formulario
                await BindGridAsync();
                pnlFormSalon.Visible = false;
                btnShowAddSalonForm.Visible = true;
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
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { return; }
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
                    ddlHotelSalon.DataSource = hoteles;
                    ddlHotelSalon.DataTextField = "Nombre";
                    ddlHotelSalon.DataValueField = "HotelID";
                    ddlHotelSalon.DataBind();
                    ddlHotelSalon.Items.Insert(0, new ListItem("-- Seleccione Hotel --", "0"));
                }
                else { lblMessage.Text += " Error cargando hoteles para el dropdown."; }
            }
            catch (Exception ex) { lblMessage.Text += " Excepción cargando hoteles dropdown: " + ex.Message.Substring(0, Math.Min(ex.Message.Length, 50)); }
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
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/SalonesEvento");
                if (response.IsSuccessStatusCode)
                {
                    var salones = JsonConvert.DeserializeObject<List<SalonEventoViewModel>>(await response.Content.ReadAsStringAsync());
                    gvSalonesEvento.DataSource = salones;
                    gvSalonesEvento.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar salones: {response.StatusCode}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        protected void btnShowAddSalonForm_Click(object sender, EventArgs e)
        {
            ClearSalonForm();
            litFormTitle.Text = "Añadir Nuevo Salón de Evento";
            hfSalonEventoID.Value = "0";
            pnlFormSalon.Visible = true;
            btnShowAddSalonForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSaveSalon_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var salonData = new SalonEventoBindingModel
            {
                HotelID = Convert.ToInt32(ddlHotelSalon.SelectedValue),
                Nombre = txtNombreSalon.Text.Trim(),
                Descripcion = txtDescripcionSalon.Text.Trim(),
                CapacidadMaxima = Convert.ToInt32(txtCapacidadSalon.Text),
                Ubicacion = txtUbicacionSalon.Text.Trim(),
                PrecioPorHora = string.IsNullOrWhiteSpace(txtPrecioHoraSalon.Text) ? (decimal?)null : decimal.Parse(txtPrecioHoraSalon.Text),
                EstaActivo = chkEstaActivoSalon.Checked
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/SalonesEvento";
            int salonID = Convert.ToInt32(hfSalonEventoID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(salonData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (salonID == 0) // Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar
                {
                    response = await client.PutAsync($"{apiUrl}/{salonID}", httpContent);
                }

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Created)
                {
                    lblSuccessMessage.Text = salonID == 0 ? "Salón creado exitosamente." : "Salón actualizado exitosamente.";
                    pnlFormSalon.Visible = false;
                    btnShowAddSalonForm.Visible = true;
                    await BindGridAsync();
                    ClearSalonForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al guardar salón: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 300))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al guardar salón: {ex.Message}"; }
        }

        protected void btnCancelSalon_Click(object sender, EventArgs e)
        {
            pnlFormSalon.Visible = false;
            btnShowAddSalonForm.Visible = true;
            ClearSalonForm();
        }

        private void ClearSalonForm()
        {
            hfSalonEventoID.Value = "0";
            ddlHotelSalon.ClearSelection();
            txtNombreSalon.Text = "";
            txtDescripcionSalon.Text = "";
            txtCapacidadSalon.Text = "";
            txtUbicacionSalon.Text = "";
            txtPrecioHoraSalon.Text = "";
            chkEstaActivoSalon.Checked = true;
            lblMessage.Text = "";
        }

        protected async void gvSalonesEvento_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int salonID = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditSalon")
            {
                await LoadSalonForEditAsync(salonID);
            }
            else if (e.CommandName == "DeleteSalon")
            {
                await DeleteSalonAsync(salonID);
            }
        }

        protected void gvSalonesEvento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSalonesEvento.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(BindGridAsync));
        }

        private async Task LoadSalonForEditAsync(int salonID)
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
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/SalonesEvento/{salonID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var salon = JsonConvert.DeserializeObject<SalonEventoViewModel>(jsonResponse);
                    if (salon != null)
                    {
                        hfSalonEventoID.Value = salon.SalonEventoID.ToString();
                        ddlHotelSalon.SelectedValue = salon.HotelID.ToString();
                        txtNombreSalon.Text = salon.Nombre;
                        txtDescripcionSalon.Text = salon.Descripcion;
                        txtCapacidadSalon.Text = salon.CapacidadMaxima.ToString();
                        txtUbicacionSalon.Text = salon.Ubicacion;
                        txtPrecioHoraSalon.Text = salon.PrecioPorHora?.ToString("F2");
                        chkEstaActivoSalon.Checked = salon.EstaActivo;

                        litFormTitle.Text = "Editar Salón de Evento";
                        pnlFormSalon.Visible = true;
                        btnShowAddSalonForm.Visible = false;
                    }
                }
                else { lblMessage.Text = "Error al cargar datos del salón para edición."; }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión: {ex.Message}"; }
        }

        private async Task DeleteSalonAsync(int salonID)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { lblMessage.Text = "Sesión expirada."; return; }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }
            try
            {
                HttpResponseMessage response = await client.DeleteAsync($"{_apiBaseUrl.TrimEnd('/')}/api/SalonesEvento/{salonID}");
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Salón de evento eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar salón: {response.StatusCode} - {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex) { lblMessage.Text = $"Error de conexión al eliminar salón: {ex.Message}"; }

            pnlFormSalon.Visible = false;
            btnShowAddSalonForm.Visible = true;
            await BindGridAsync();
        }
    }
}