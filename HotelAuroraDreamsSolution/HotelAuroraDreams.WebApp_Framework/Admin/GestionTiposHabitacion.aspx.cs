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
namespace HotelAuroraDreams.WebApp_Framework.Admin
{
    public partial class GestionTiposHabitacion : System.Web.UI.Page
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
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                await BindGridAsync();
            }
        }

        private async Task<bool> IsUserAuthorizedAdminAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                return false;
            }
            var userRoles = Session["UserRoles"] as IList<string>;
            if (userRoles != null && userRoles.Contains("Administrador"))
            {
                return true;
            }
            // Si no hay roles en sesión, intenta obtenerlos de la API
            else if (authTokenCookie != null && !string.IsNullOrEmpty(authTokenCookie.Value))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
                try
                {
                    var response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/Account/UserInfo");
                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = JsonConvert.DeserializeObject<Models.UserInfoViewModel>(await response.Content.ReadAsStringAsync());
                        if (userInfo != null && userInfo.Roles != null && userInfo.Roles.Contains("Administrador"))
                        {
                            Session["UserRoles"] = userInfo.Roles; // Guardar para futuras comprobaciones
                            return true;
                        }
                    }
                }
                catch {  }
            }
            return false; 
        }


        private async Task BindGridAsync()
        {
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "No autenticado.";
                return;
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            try
            {
                HttpResponseMessage response = await client.GetAsync(_apiBaseUrl.TrimEnd('/') + "/api/TiposHabitacion");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tiposHabitacion = JsonConvert.DeserializeObject<List<TipoHabitacionViewModel>>(jsonResponse);
                    gvTiposHabitacion.DataSource = tiposHabitacion;
                    gvTiposHabitacion.DataBind();
                }
                else
                {
                    lblMessage.Text = $"Error al cargar tipos de habitación: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        protected void btnShowAddForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            litFormTitle.Text = "Añadir Nuevo Tipo de Habitación";
            hfTipoHabitacionID.Value = "0"; // Indica que es nuevo
            pnlForm.Visible = true;
            btnShowAddForm.Visible = false;
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return; // Validadores ASP.NET

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "Sesión expirada o no autenticado.";
                return;
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            var tipoHabitacionData = new TipoHabitacionBindingModel // Usa el DTO de la API
            {
                Nombre = txtNombre.Text,
                Descripcion = txtDescripcion.Text,
                PrecioBase = decimal.Parse(txtPrecioBase.Text),
                Capacidad = int.Parse(txtCapacidad.Text),
                Comodidades = txtComodidades.Text
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/TiposHabitacion";
            int tipoHabitacionID = Convert.ToInt32(hfTipoHabitacionID.Value);

            try
            {
                if (tipoHabitacionID == 0) // Nuevo
                {
                    response = await client.PostAsJsonAsync(apiUrl, tipoHabitacionData);
                }
                else // Editar
                {
                    response = await client.PutAsJsonAsync($"{apiUrl}/{tipoHabitacionID}", tipoHabitacionData);
                }

                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = tipoHabitacionID == 0 ? "Tipo de habitación creado exitosamente." : "Tipo de habitación actualizado exitosamente.";
                    pnlForm.Visible = false;
                    btnShowAddForm.Visible = true;
                    await BindGridAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        // Intenta deserializar el error de validación del ModelState de la API
                        var apiError = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorContent);
                        if (apiError != null && apiError.ContainsKey("ModelState"))
                        {
                            var modelStateErrors = apiError["ModelState"] as Newtonsoft.Json.Linq.JObject;
                            if (modelStateErrors != null)
                            {
                                foreach (var prop in modelStateErrors.Properties())
                                {
                                    foreach (var err in prop.Value.Children<Newtonsoft.Json.Linq.JValue>())
                                    {
                                        lblMessage.Text += $"{err.Value}<br/>";
                                    }
                                }
                            }
                            else if (apiError.ContainsKey("Message"))
                            {
                                lblMessage.Text = apiError["Message"].ToString();
                            }
                            else
                            {
                                lblMessage.Text = $"Error: {response.StatusCode}. {errorContent}";
                            }
                        }
                        else if (apiError != null && apiError.ContainsKey("Message"))
                        {
                            lblMessage.Text = apiError["Message"].ToString();
                        }
                        else
                        {
                            lblMessage.Text = $"Error al guardar: {response.StatusCode} - {errorContent}";
                        }
                    }
                    catch
                    {
                        lblMessage.Text = $"Error al guardar: {response.StatusCode} - {errorContent}";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al guardar: {ex.Message}";
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            btnShowAddForm.Visible = true;
            ClearForm();
        }

        private void ClearForm()
        {
            hfTipoHabitacionID.Value = "0";
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            txtPrecioBase.Text = "";
            txtCapacidad.Text = "";
            txtComodidades.Text = "";
            lblMessage.Text = "";
        }

        protected async void gvTiposHabitacion_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRow") // Renombramos para evitar colisión con "Edit" reservado
            {
            }
        }

        protected async void gvTiposHabitacion_RowEditing(object sender, GridViewEditEventArgs e)
        {

            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
            pnlForm.Visible = true;
            btnShowAddForm.Visible = false;
            litFormTitle.Text = "Editar Tipo de Habitación";

            int tipoHabitacionID = Convert.ToInt32(gvTiposHabitacion.DataKeys[e.NewEditIndex].Value);
            hfTipoHabitacionID.Value = tipoHabitacionID.ToString();

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value)) return;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/TiposHabitacion/{tipoHabitacionID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tipoHabitacion = JsonConvert.DeserializeObject<TipoHabitacionViewModel>(jsonResponse);
                    if (tipoHabitacion != null)
                    {
                        txtNombre.Text = tipoHabitacion.nombre;
                        txtDescripcion.Text = tipoHabitacion.descripcion;
                        txtPrecioBase.Text = tipoHabitacion.precio_base.ToString("N2"); // Formato de moneda
                        txtCapacidad.Text = tipoHabitacion.capacidad.ToString();
                        txtComodidades.Text = tipoHabitacion.comodidades;
                    }
                }
                else
                {
                    lblMessage.Text = "Error al cargar datos para edición.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        protected async void gvTiposHabitacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "Sesión expirada o no autenticado.";
                return;
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            int tipoHabitacionID = Convert.ToInt32(gvTiposHabitacion.DataKeys[e.RowIndex].Value);
            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/TiposHabitacion/{tipoHabitacionID}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Tipo de habitación eliminado exitosamente.";
                    await BindGridAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al eliminar: {response.StatusCode} - {errorContent}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al eliminar: {ex.Message}";
            }
            pnlForm.Visible = false; // Ocultar formulario si estaba visible
            btnShowAddForm.Visible = true;
        }

        protected void gvTiposHabitacion_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
        }

        protected void gvTiposHabitacion_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {

        }
    }
}