// File: ~/Facturacion/DescargarFacturaEvento.aspx.cs
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace HotelAuroraDreams.WebApp_Framework.Facturacion
{
    public partial class DescargarFacturaEvento : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(DescargarFacturaEventoAsync));
        }

        private async Task DescargarFacturaEventoAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                Response.Clear(); Response.StatusCode = 401; Response.ContentType = "text/plain";
                Response.Write("No autorizado."); Response.End(); return;
            }

            if (string.IsNullOrEmpty(Request.QueryString["id"]) || !int.TryParse(Request.QueryString["id"], out int facturaEventoId))
            {
                Response.Clear(); Response.StatusCode = 400; Response.ContentType = "text/plain";
                Response.Write("ID de factura de evento no válido."); Response.End(); return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            try
            {
                string pdfUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/FacturasEvento/{facturaEventoId}/pdf";
                HttpResponseMessage apiResponse = await client.GetAsync(pdfUrl);

                if (apiResponse.IsSuccessStatusCode)
                {
                    byte[] pdfBytes = await apiResponse.Content.ReadAsByteArrayAsync();
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", $"inline; filename=\"FacturaEvento-{facturaEventoId}.pdf\"");
                    Response.BinaryWrite(pdfBytes);
                    Response.End();
                }
                else
                {
                    string errorContent = await apiResponse.Content.ReadAsStringAsync();
                    Response.Clear(); Response.StatusCode = (int)apiResponse.StatusCode; Response.ContentType = "text/plain";
                    Response.Write($"Error al obtener PDF de factura de evento: {apiResponse.ReasonPhrase}. Detalles: {errorContent.Substring(0, Math.Min(errorContent.Length, 500))}");
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                Response.Clear(); Response.StatusCode = 500; Response.ContentType = "text/plain";
                Response.Write($"Error al descargar factura de evento: {ex.Message}"); Response.End();
            }
        }
    }
}