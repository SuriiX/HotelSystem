// File: ~/Facturacion/VerFacturasEvento.aspx.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HotelAuroraDreams.WebApp_Framework.Models;
using Newtonsoft.Json;

namespace HotelAuroraDreams.WebApp_Framework.Facturacion
{
    public partial class VerFacturasEvento : System.Web.UI.Page
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
                await BindGridAsync();
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

        private async Task BindGridAsync(string clienteId = null, string reservaEventoId = null, string fechaDesde = null, string fechaHasta = null, string estado = null)
        {
            lblMessage.Text = "";
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null) { ShowError("No autenticado."); return; }
            SetAuthorizationHeader(authTokenCookie.Value);

            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(clienteId) && clienteId != "0") queryParams.Add($"clienteId={clienteId}");
                if (!string.IsNullOrWhiteSpace(reservaEventoId) && reservaEventoId != "0") queryParams.Add($"reservaEventoId={reservaEventoId}");
                if (!string.IsNullOrWhiteSpace(fechaDesde)) queryParams.Add($"fechaDesde={HttpUtility.UrlEncode(fechaDesde)}");
                if (!string.IsNullOrWhiteSpace(fechaHasta)) queryParams.Add($"fechaHasta={HttpUtility.UrlEncode(fechaHasta)}");
                // API de FacturasEvento no tiene filtro de estado en el GET list, podemos añadirlo o quitarlo del UI
                // if (!string.IsNullOrWhiteSpace(estado)) queryParams.Add($"estado={HttpUtility.UrlEncode(estado)}"); 

                string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                string apiUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/FacturasEvento{queryString}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var facturas = JsonConvert.DeserializeObject<List<FacturaEventoViewModel>>(jsonResponse);
                    gvFacturasEvento.DataSource = facturas;
                    gvFacturasEvento.DataBind();
                }
                else
                {
                    ShowError($"Error al cargar facturas de evento: {response.StatusCode}");
                }
            }
            catch (Exception ex) { ShowError($"Error de conexión: {ex.Message}"); }
        }

        protected void btnFiltrarFacturasEvento_Click(object sender, EventArgs e)
        {
            gvFacturasEvento.PageIndex = 0;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                    txtFiltroClienteIDFacturaEv.Text.Trim(),
                    txtFiltroReservaEventoIDFacturaEv.Text.Trim(),
                    txtFiltroFechaDesdeFacturaEv.Text.Trim(),
                    txtFiltroFechaHastaFacturaEv.Text.Trim()
                // ddlFiltroEstadoFacturaEv.SelectedValue // API no lo soporta actualmente
                );
            }));
        }

        protected void btnLimpiarFiltrosFacturasEvento_Click(object sender, EventArgs e)
        {
            txtFiltroClienteIDFacturaEv.Text = "";
            txtFiltroReservaEventoIDFacturaEv.Text = "";
            txtFiltroFechaDesdeFacturaEv.Text = "";
            txtFiltroFechaHastaFacturaEv.Text = "";
            // ddlFiltroEstadoFacturaEv.SelectedValue = "";
            gvFacturasEvento.PageIndex = 0;
            RegisterAsyncTask(new PageAsyncTask(async () => { await BindGridAsync(); }));
        }

        protected void gvFacturasEvento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvFacturasEvento.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(async () => {
                await BindGridAsync(
                   txtFiltroClienteIDFacturaEv.Text.Trim(),
                   txtFiltroReservaEventoIDFacturaEv.Text.Trim(),
                   txtFiltroFechaDesdeFacturaEv.Text.Trim(),
                   txtFiltroFechaHastaFacturaEv.Text.Trim()
               // ddlFiltroEstadoFacturaEv.SelectedValue
               );
            }));
        }

        protected void gvFacturasEvento_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // No se necesita si usamos HyperLink para el PDF.
        }

        protected string GetFacturaEventoPdfUrl(object facturaEventoIdObj)
        {
            if (facturaEventoIdObj != null && int.TryParse(facturaEventoIdObj.ToString(), out int facturaEventoId))
            {
                return $"~/Facturacion/DescargarFacturaEvento.aspx?id={facturaEventoId}";
            }
            return "#";
        }

        private void ShowError(string message)
        {
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Text = message;
        }
    }
}