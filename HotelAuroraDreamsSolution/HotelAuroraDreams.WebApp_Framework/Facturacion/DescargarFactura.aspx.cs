// File: ~/Facturacion/DescargarFactura.aspx.cs
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace HotelAuroraDreams.WebApp_Framework.Facturacion
{
    public partial class DescargarFactura : System.Web.UI.Page
    {
        private static readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(DescargarFacturaAsync));
        }

        private async Task DescargarFacturaAsync()
        {
            HttpCookie authTokenCookie = Request.Cookies["AuthTokenHotel"];
            if (authTokenCookie == null || string.IsNullOrEmpty(authTokenCookie.Value))
            {
                // No autenticado, redirigir a Login o mostrar error
                Response.Clear();
                Response.StatusCode = 401; // Unauthorized
                Response.ContentType = "text/plain";
                Response.Write("No autorizado para descargar la factura. Por favor, inicie sesión.");
                Response.End();
                return;
            }

            if (string.IsNullOrEmpty(Request.QueryString["id"]) || !int.TryParse(Request.QueryString["id"], out int facturaId))
            {
                Response.Clear();
                Response.StatusCode = 400; // Bad Request
                Response.ContentType = "text/plain";
                Response.Write("ID de factura no válido o no proporcionado.");
                Response.End();
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authTokenCookie.Value);

            try
            {
                string pdfUrl = $"{_apiBaseUrl.TrimEnd('/')}/api/Facturas/{facturaId}/pdf";
                HttpResponseMessage apiResponse = await client.GetAsync(pdfUrl);

                if (apiResponse.IsSuccessStatusCode)
                {
                    byte[] pdfBytes = await apiResponse.Content.ReadAsByteArrayAsync();

                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    // Para descargar como archivo:
                    // Response.AddHeader("Content-Disposition", $"attachment; filename=\"Factura-{facturaId}.pdf\"");
                    // Para mostrar en el navegador (inline):
                    Response.AddHeader("Content-Disposition", $"inline; filename=\"Factura-{facturaId}.pdf\"");

                    Response.BinaryWrite(pdfBytes);
                    // Response.Flush(); // Asegura que todo se envíe
                    // HttpContext.Current.ApplicationInstance.CompleteRequest(); // Forma más segura de terminar
                    Response.End(); // Termina la respuesta para que no se renderice más HTML de la página
                }
                else
                {
                    string errorContent = await apiResponse.Content.ReadAsStringAsync();
                    Response.Clear();
                    Response.StatusCode = (int)apiResponse.StatusCode;
                    Response.ContentType = "text/plain";
                    Response.Write($"Error al obtener el PDF de la API: {apiResponse.ReasonPhrase}. Detalles: {errorContent.Substring(0, Math.Min(errorContent.Length, 500))}");
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.StatusCode = 500; // Internal Server Error
                Response.ContentType = "text/plain";
                Response.Write($"Error de conexión o procesamiento al intentar descargar la factura: {ex.Message}");
                Response.End();
            }
        }
    }
}