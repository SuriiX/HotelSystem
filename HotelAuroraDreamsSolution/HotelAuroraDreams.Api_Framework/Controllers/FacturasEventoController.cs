// File: ~/Controllers/FacturasEventoController.cs
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/FacturasEvento")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class FacturasEventoController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        // GET: api/FacturasEvento
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetFacturasEvento([FromUri] int? clienteId = null, [FromUri] int? reservaEventoId = null, [FromUri] DateTime? fechaDesde = null, [FromUri] DateTime? fechaHasta = null)
        {
            try
            {
                var query = db.FacturaEventoes.AsQueryable();

                if (clienteId.HasValue)
                {
                    query = query.Where(f => f.ClienteID == clienteId.Value);
                }
                if (reservaEventoId.HasValue)
                {
                    query = query.Where(f => f.ReservaEventoID == reservaEventoId.Value);
                }
                if (fechaDesde.HasValue)
                {
                    query = query.Where(f => f.FechaEmision >= fechaDesde.Value);
                }
                if (fechaHasta.HasValue)
                {
                    DateTime fechaHastaFinDia = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(f => f.FechaEmision <= fechaHastaFinDia);
                }

                var facturas = await query
                    .OrderByDescending(f => f.FechaEmision)
                    .Select(f => new FacturaEventoViewModel // Proyección simplificada para la lista
                    {
                        FacturaEventoID = f.FacturaEventoID,
                        ReservaEventoID = f.ReservaEventoID,
                        NombreEvento = f.ReservaEvento.NombreEvento, // Asumiendo navegación
                        ClienteID = f.ClienteID,
                        NombreCliente = f.Cliente.nombre + " " + f.Cliente.apellido,
                        FechaEmision = (DateTime)f.FechaEmision,
                        TotalFactura = f.TotalFactura,
                        Estado = f.Estado
                    })
                    .ToListAsync();

                return Ok(facturas);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener facturas de evento: {ex.ToString()}"));
            }
        }

        // GET: api/FacturasEvento/5
        [HttpGet]
        [Route("{id:int}", Name = "GetFacturaEventoById")]
        [ResponseType(typeof(FacturaEventoViewModel))]
        public async Task<IHttpActionResult> GetFacturaEvento(int id)
        {
            try
            {
                var facturaEntity = await db.FacturaEventoes
                    .Include(f => f.Cliente)
                    .Include(f => f.Empleado) // Empleado emisor
                    .Include(f => f.ReservaEvento.SalonEvento.Hotel) // Para info del hotel y salón
                    .Include(f => f.ReservaEvento.TipoEvento)
                    .Include(f => f.DetalleFacturaEventoes)
                    .FirstOrDefaultAsync(f => f.FacturaEventoID == id);

                if (facturaEntity == null)
                {
                    return NotFound();
                }

                var viewModel = new FacturaEventoViewModel
                {
                    FacturaEventoID = facturaEntity.FacturaEventoID,
                    ReservaEventoID = facturaEntity.ReservaEventoID,
                    NombreEvento = facturaEntity.ReservaEvento?.NombreEvento,
                    ClienteID = facturaEntity.ClienteID,
                    NombreCliente = $"{facturaEntity.Cliente.nombre} {facturaEntity.Cliente.apellido}",
                    FechaEmision = (DateTime)facturaEntity.FechaEmision,
                    SubtotalSalon = facturaEntity.SubtotalSalon,
                    SubtotalServiciosAdicionales = facturaEntity.SubtotalServiciosAdicionales,
                    Impuestos = facturaEntity.Impuestos,
                    TotalFactura = facturaEntity.TotalFactura,
                    MetodoPago = facturaEntity.MetodoPago,
                    Estado = facturaEntity.Estado,
                    NombreEmpleadoEmisor = facturaEntity.Empleado != null ? $"{facturaEntity.Empleado.nombre} {facturaEntity.Empleado.apellido}" : "N/A",
                    Notas = facturaEntity.Notas,
                    Detalles = facturaEntity.DetalleFacturaEventoes.Select(df => new DetalleFacturaEventoViewModel
                    {
                        DetalleFacturaEventoID = df.DetalleFacturaEventoID,
                        TipoConcepto = df.TipoConcepto,
                        ReferenciaConceptoID = df.ReferenciaConceptoID,
                        DescripcionConcepto = df.DescripcionConcepto,
                        Cantidad = df.Cantidad,
                        PrecioUnitario = df.PrecioUnitario,
                        Subtotal = df.Subtotal
                    }).ToList()
                };
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener factura de evento ID {id}: {ex.ToString()}"));
            }
        }

        // GET: api/FacturasEvento/5/pdf
        [HttpGet]
        [Route("{id:int}/pdf")]
        public async Task<HttpResponseMessage> GetFacturaEventoPdf(int id)
        {
            try
            {
                var facturaEntity = await db.FacturaEventoes
                    .Include(f => f.Cliente)
                    .Include(f => f.Empleado)
                    .Include(f => f.ReservaEvento.SalonEvento.Hotel)
                    .Include(f => f.ReservaEvento.TipoEvento)
                    .Include(f => f.DetalleFacturaEventoes)
                    .FirstOrDefaultAsync(f => f.FacturaEventoID == id);

                if (facturaEntity == null || facturaEntity.ReservaEvento == null || facturaEntity.Cliente == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Factura de evento o datos relacionados no encontrados.");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 35, 35, 35, 35);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                    Font bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                    Font smallBoldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);
                    Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);

                    Paragraph title = new Paragraph($"FACTURA DE EVENTO N°: E{facturaEntity.FacturaEventoID}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(Chunk.NEWLINE);

                    if (facturaEntity.ReservaEvento.SalonEvento?.Hotel != null)
                    {
                        Paragraph hotelInfo = new Paragraph();
                        hotelInfo.Alignment = Element.ALIGN_CENTER;
                        hotelInfo.Add(new Chunk($"{facturaEntity.ReservaEvento.SalonEvento.Hotel.nombre}\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
                        hotelInfo.Add(new Chunk($"{facturaEntity.ReservaEvento.SalonEvento.Hotel.direccion}\n", smallFont));
                        if (!string.IsNullOrEmpty(facturaEntity.ReservaEvento.SalonEvento.Hotel.telefono))
                            hotelInfo.Add(new Chunk($"Tel: {facturaEntity.ReservaEvento.SalonEvento.Hotel.telefono}"));
                        if (!string.IsNullOrEmpty(facturaEntity.ReservaEvento.SalonEvento.Hotel.email))
                            hotelInfo.Add(new Chunk($" - Email: {facturaEntity.ReservaEvento.SalonEvento.Hotel.email}\n", smallFont));
                        else
                            hotelInfo.Add(Chunk.NEWLINE);
                        document.Add(hotelInfo);
                    }
                    document.Add(Chunk.NEWLINE);

                    PdfPTable infoTable = new PdfPTable(2);
                    infoTable.WidthPercentage = 100;
                    infoTable.SetWidths(new float[] { 1.5f, 1f });

                    PdfPCell cellCliente = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingBottom = 8f };
                    cellCliente.AddElement(new Phrase("CLIENTE:", smallBoldFont));
                    cellCliente.AddElement(new Phrase($"{facturaEntity.Cliente.nombre} {facturaEntity.Cliente.apellido}", bodyFont));
                    cellCliente.AddElement(new Phrase($"{facturaEntity.Cliente.tipo_documento}: {facturaEntity.Cliente.numero_documento}", bodyFont));
                    infoTable.AddCell(cellCliente);

                    PdfPCell cellFactura = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingBottom = 8f };
                    cellFactura.AddElement(new Phrase($"FECHA EMISIÓN: {facturaEntity.FechaEmision:yyyy-MM-dd HH:mm}", bodyFont));
                    cellFactura.AddElement(new Phrase($"EVENTO: {facturaEntity.ReservaEvento.NombreEvento}", bodyFont));
                    cellFactura.AddElement(new Phrase($"FECHA EVENTO: {facturaEntity.ReservaEvento.FechaEvento:yyyy-MM-dd}", bodyFont));
                    if (facturaEntity.Empleado != null)
                    {
                        cellFactura.AddElement(new Phrase($"ATENDIDO POR: {facturaEntity.Empleado.nombre} {facturaEntity.Empleado.apellido}", bodyFont));
                    }
                    infoTable.AddCell(cellFactura);
                    document.Add(infoTable);
                    document.Add(Chunk.NEWLINE);

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 5f, 1.5f, 2f, 2f });
                    detailsTable.SpacingBefore = 10f;
                    detailsTable.AddCell(new PdfPCell(new Phrase("Concepto", smallBoldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5 });
                    detailsTable.AddCell(new PdfPCell(new Phrase("Cant.", smallBoldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    detailsTable.AddCell(new PdfPCell(new Phrase("P. Unit.", smallBoldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    detailsTable.AddCell(new PdfPCell(new Phrase("Subtotal", smallBoldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

                    foreach (var detalle in facturaEntity.DetalleFacturaEventoes)
                    {
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.DescripcionConcepto, smallFont)) { Padding = 5 });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.Cantidad.ToString(), smallFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.PrecioUnitario.ToString("C"), smallFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.Subtotal.ToString("C"), smallFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    }
                    document.Add(detailsTable);
                    document.Add(Chunk.NEWLINE);

                    PdfPTable totalsTable = new PdfPTable(2);
                    totalsTable.WidthPercentage = 50;
                    totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalsTable.SetWidths(new float[] { 3f, 2f }); // Ajustado
                    totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    totalsTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalsTable.DefaultCell.Padding = 2;

                    totalsTable.AddCell(new Phrase("Subtotal Salón:", headerFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.SubtotalSalon.ToString("C"), bodyFont));
                    totalsTable.AddCell(new Phrase("Subtotal Servicios Adic.:", headerFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.SubtotalServiciosAdicionales.ToString("C"), bodyFont));
                    totalsTable.AddCell(new Phrase("Impuestos:", headerFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.Impuestos.ToString("C"), bodyFont));
                    totalsTable.AddCell(new Phrase("TOTAL EVENTO:", titleFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.TotalFactura.ToString("C"), titleFont));
                    document.Add(totalsTable);
                    document.Add(Chunk.NEWLINE);

                    document.Add(new Phrase($"Método de Pago: {facturaEntity.MetodoPago}", bodyFont));
                    document.Add(new Phrase($"Estado Factura: {facturaEntity.Estado}", bodyFont));

                    document.Close();
                    writer.Close();

                    byte[] pdfBytes = ms.ToArray();
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfBytes)
                    };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                    {
                        FileName = $"FacturaEvento-E{facturaEntity.FacturaEventoID}.pdf"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error al generar PDF de factura de evento: {ex.ToString()}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}