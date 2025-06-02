// File: ~/Admin/GestionTiposHabitacion.aspx.cs
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
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Newtonsoft.Json;

namespace HotelAuroraDreams.WebApp_Framework.Admin
{
    public partial class GestionTiposHabitacion : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient(); // Reutilizar HttpClient

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
                pnlForm.Visible = false; // Ocultar formulario inicialmente
                btnShowAddForm.Visible = true;
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
            else if (authTokenCookie != null && !string.IsNullOrEmpty(authTokenCookie.Value))
            {
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
                catch { /* Ignorar error, acceso denegado */ }
            }
            return false;
        }

        private async Task BindGridAsync()
        {
            lblMessage.Text = "";

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "No autenticado o sesión expirada.";
                return;
            }

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
                    var tiposHabitacion = JsonConvert.DeserializeObject<List<TipoHabitacionViewModel>>(jsonResponse);
                    gvTiposHabitacion.DataSource = tiposHabitacion;

                    gvTiposHabitacion.DataBind();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    lblMessage.Text = "No autorizado para ver esta información.";
                    Response.Cookies["AuthTokenHotel"].Expires = DateTime.Now.AddDays(-1);
                    Session.Clear();
                    Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    lblMessage.Text = $"Error al cargar tipos de habitación: {response.StatusCode}. Detalles: {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al cargar datos: {ex.Message}";
            }
        }

        protected void btnShowAddForm_Click(object sender, EventArgs e)
        {
            ClearForm();
            litFormTitle.Text = "Añadir Nuevo Tipo de Habitación";
            hfTipoHabitacionID.Value = "0";
            pnlForm.Visible = true;
            btnShowAddForm.Visible = false;
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "Sesión expirada o no autenticado. Por favor, inicie sesión de nuevo.";
                return;
            }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            var tipoHabitacionData = new TipoHabitacionBindingModel
            {
                Nombre = txtNombre.Text.Trim(),
                Descripcion = txtDescripcion.Text.Trim(),
                PrecioBase = decimal.Parse(txtPrecioBase.Text), // Considerar TryParse con validación
                Capacidad = int.Parse(txtCapacidad.Text),       // Considerar TryParse con validación
                Comodidades = txtComodidades.Text.Trim()
            };

            HttpResponseMessage response;
            string apiUrl = _apiBaseUrl.TrimEnd('/') + "/api/TiposHabitacion";
            int tipoHabitacionID = Convert.ToInt32(hfTipoHabitacionID.Value);

            try
            {
                string jsonPayload = JsonConvert.SerializeObject(tipoHabitacionData);
                HttpContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (tipoHabitacionID == 0) // Crear Nuevo
                {
                    response = await client.PostAsync(apiUrl, httpContent);
                }
                else // Editar Existente
                {
                    response = await client.PutAsync($"{apiUrl}/{tipoHabitacionID}", httpContent);
                }

                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = tipoHabitacionID == 0 ? "Tipo de habitación creado exitosamente." : "Tipo de habitación actualizado exitosamente.";
                    pnlForm.Visible = false;
                    btnShowAddForm.Visible = true;
                    await BindGridAsync();
                    ClearForm();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var apiError = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorContent);
                        if (apiError != null && apiError.ContainsKey("ModelState"))
                        {
                            lblMessage.Text = ""; // Limpiar mensajes previos
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
                            else { lblMessage.Text = $"Error al guardar (ModelState): {errorContent}"; }
                        }
                        else if (apiError != null && apiError.ContainsKey("Message"))
                        {
                            lblMessage.Text = $"Error de la API: {apiError["Message"]}";
                        }
                        else
                        {
                            lblMessage.Text = $"Error al guardar: {response.StatusCode}. {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                        }
                    }
                    catch
                    {
                        lblMessage.Text = $"Error al procesar respuesta de guardado: {response.StatusCode}. {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
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
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
        }

        private void ClearForm()
        {
            hfTipoHabitacionID.Value = "0";
            txtNombre.Text = "";
            txtDescripcion.Text = "";
            txtPrecioBase.Text = "";
            txtCapacidad.Text = "";
            txtComodidades.Text = "";
            // lblMessage.Text = ""; // No limpiar aquí para que los errores persistan hasta la próxima acción
        }

        protected async void gvTiposHabitacion_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditRow")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int tipoHabitacionID = Convert.ToInt32(gvTiposHabitacion.DataKeys[rowIndex].Value);
                await LoadTipoHabitacionForEditAsync(tipoHabitacionID);
            }
            // No necesitamos manejar "Delete" aquí si usamos gvTiposHabitacion_RowDeleting
        }

        private async Task LoadTipoHabitacionForEditAsync(int tipoHabitacionID)
        {
            lblMessage.Text = "";
            lblSuccessMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value)) return;

            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiBaseUrl.TrimEnd('/')}/api/TiposHabitacion/{tipoHabitacionID}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tipoHabitacion = JsonConvert.DeserializeObject<TipoHabitacionViewModel>(jsonResponse);
                    if (tipoHabitacion != null)
                    {
                        hfTipoHabitacionID.Value = tipoHabitacion.tipo_habitacion_id.ToString();
                        txtNombre.Text = tipoHabitacion.nombre;
                        txtDescripcion.Text = tipoHabitacion.descripcion;
                        txtPrecioBase.Text = tipoHabitacion.precio_base.ToString("F2"); 
                        txtCapacidad.Text = tipoHabitacion.capacidad.ToString();
                        txtComodidades.Text = tipoHabitacion.comodidades;

                        litFormTitle.Text = "Editar Tipo de Habitación";
                        pnlForm.Visible = true;
                        btnShowAddForm.Visible = false;
                    }
                }
                else
                {
                    lblMessage.Text = "Error al cargar datos del tipo de habitación para edición.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión: {ex.Message}";
            }
        }

        protected void gvTiposHabitacion_RowEditing(object sender, GridViewEditEventArgs e)
        {
        }

        protected async void gvTiposHabitacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                lblMessage.Text = "Sesión expirada o no autenticado.";
                return;
            }
            if (client.DefaultRequestHeaders.Authorization == null || client.DefaultRequestHeaders.Authorization.Parameter != authTokenCookie.Value)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);
            }

            int tipoHabitacionID = Convert.ToInt32(gvTiposHabitacion.DataKeys[e.RowIndex].Value);
            string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/TiposHabitacion/{tipoHabitacionID}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    lblSuccessMessage.Text = "Tipo de habitación eliminado exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.Conflict) 
                    {
                        lblMessage.Text = errorContent; 
                    }
                    else
                    {
                        lblMessage.Text = $"Error al eliminar: {response.StatusCode}. {errorContent.Substring(0, Math.Min(errorContent.Length, 200))}";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error de conexión al eliminar: {ex.Message}";
            }

            pnlForm.Visible = false;
            btnShowAddForm.Visible = true;
            await BindGridAsync(); 
        }

        protected void gvTiposHabitacion_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
        }

        protected void gvTiposHabitacion_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
        }
    }
}