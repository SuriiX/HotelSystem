using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsFactura : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<IHttpActionResult> ObtenerFacturas(int? clienteId = null, int? reservaId = null,
            DateTime? fechaDesde = null, DateTime? fechaHasta = null, string estado = null)
        {
            try
            {
                var query = db.Facturas.AsQueryable();

                if (clienteId.HasValue)
                    query = query.Where(f => f.cliente_id == clienteId.Value);

                if (reservaId.HasValue)
                    query = query.Where(f => f.reserva_id == reservaId.Value);

                if (fechaDesde.HasValue)
                    query = query.Where(f => f.fecha_emision >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                {
                    DateTime fechaHastaFinDia = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(f => f.fecha_emision <= fechaHastaFinDia);
                }

                if (!string.IsNullOrWhiteSpace(estado))
                    query = query.Where(f => f.estado.ToLower() == estado.ToLower());

                var facturas = await query
                    .OrderByDescending(f => f.fecha_emision)
                    .Select(f => new FacturaViewModel
                    {
                        FacturaID = f.factura_id,
                        ReservaID = f.reserva_id,
                        ClienteID = f.cliente_id,
                        NombreCliente = f.Cliente.nombre + " " + f.Cliente.apellido,
                        FechaEmision = (DateTime)f.fecha_emision,
                        Total = f.total,
                        Estado = f.estado,
                        MetodoPagoFactura = f.metodo_pago_factura
                    })
                    .ToListAsync();

                return new OkResult(facturas, HttpContext.Current.Request.ToHttpRequestMessage());
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult(new Exception($"Error al obtener facturas: {ex.ToString()}"));
            }
        }

        public async Task<IHttpActionResult> ObtenerFacturaPorId(int id)
        {
            try
            {
                var facturaEntity = await db.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Empleado)
                    .Include(f => f.Detalle_Factura)
                    .FirstOrDefaultAsync(f => f.factura_id == id);

                if (facturaEntity == null)
                    return new NotFoundResult();

                var viewModel = new FacturaViewModel
                {
                    FacturaID = facturaEntity.factura_id,
                    ReservaID = facturaEntity.reserva_id,
                    ClienteID = facturaEntity.cliente_id,
                    NombreCliente = $"{facturaEntity.Cliente.nombre} {facturaEntity.Cliente.apellido}",
                    FechaEmision = (DateTime)facturaEntity.fecha_emision,
                    Subtotal = facturaEntity.subtotal,
                    Impuestos = facturaEntity.impuestos,
                    Total = facturaEntity.total,
                    MetodoPagoFactura = facturaEntity.metodo_pago_factura,
                    Estado = facturaEntity.estado,
                    NombreEmpleadoEmisor = facturaEntity.Empleado != null ?
                        $"{facturaEntity.Empleado.nombre} {facturaEntity.Empleado.apellido}" : "N/A",
                    Detalles = facturaEntity.Detalle_Factura.Select(df => new DetalleFacturaViewModel
                    {
                        DetalleFacturaID = df.detalle_factura_id,
                        TipoConcepto = df.tipo_concepto,
                        ReferenciaID = df.referencia_id,
                        DescripcionConcepto = df.descripcion_concepto,
                        Cantidad = df.cantidad,
                        PrecioUnitario = df.precio_unitario,
                        Subtotal = df.subtotal
                    }).ToList()
                };

                return new OkResult(viewModel, HttpContext.Current.Request.ToHttpRequestMessage());
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult(new Exception($"Error al obtener factura ID {id}: {ex.ToString()}"));
            }
        }

        public async Task<HttpResponseMessage> GenerarPdfFactura(int id)
        {
            try
            {
                var facturaEntity = await db.Facturas
                                     .Include(f => f.Cliente)
                                     .Include(f => f.Empleado)
                                     .Include(f => f.Reserva.Hotel)
                                     .Include(f => f.Detalle_Factura)
                                     .FirstOrDefaultAsync(f => f.factura_id == id);

                if (facturaEntity == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Factura no encontrada.")
                    };
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 35, 35, 35, 35);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Configuración de fuentes
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                    Font bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                    Font smallBoldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);
                    Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);

                    // Título
                    Paragraph title = new Paragraph($"FACTURA N°: {facturaEntity.factura_id}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(Chunk.NEWLINE);

                    // Información del hotel
                    if (facturaEntity.Reserva?.Hotel != null)
                    {
                        Paragraph hotelInfo = new Paragraph();
                        hotelInfo.Alignment = Element.ALIGN_CENTER;
                        hotelInfo.Add(new Chunk($"{facturaEntity.Reserva.Hotel.nombre}\n", headerFont));
                        hotelInfo.Add(new Chunk($"{facturaEntity.Reserva.Hotel.direccion}\n", bodyFont));
                        if (!string.IsNullOrEmpty(facturaEntity.Reserva.Hotel.telefono))
                            hotelInfo.Add(new Chunk($"Tel: {facturaEntity.Reserva.Hotel.telefono}"));
                        if (!string.IsNullOrEmpty(facturaEntity.Reserva.Hotel.email))
                            hotelInfo.Add(new Chunk($" - Email: {facturaEntity.Reserva.Hotel.email}\n", bodyFont));
                        else
                            hotelInfo.Add(Chunk.NEWLINE);
                        document.Add(hotelInfo);
                        document.Add(Chunk.NEWLINE);
                    }

                    // Tabla de información
                    PdfPTable infoTable = new PdfPTable(2);
                    infoTable.WidthPercentage = 100;
                    infoTable.SetWidths(new float[] { 1.5f, 1f });

                    // Celda cliente
                    PdfPCell cellCliente = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingBottom = 8f };
                    cellCliente.AddElement(new Phrase("CLIENTE:", smallBoldFont));
                    cellCliente.AddElement(new Phrase($"{facturaEntity.Cliente.nombre} {facturaEntity.Cliente.apellido}", bodyFont));
                    cellCliente.AddElement(new Phrase($"{facturaEntity.Cliente.tipo_documento}: {facturaEntity.Cliente.numero_documento}", bodyFont));
                    if (!string.IsNullOrEmpty(facturaEntity.Cliente.direccion))
                        cellCliente.AddElement(new Phrase($"Dirección: {facturaEntity.Cliente.direccion}", smallFont));
                    if (!string.IsNullOrEmpty(facturaEntity.Cliente.telefono))
                        cellCliente.AddElement(new Phrase($"Teléfono: {facturaEntity.Cliente.telefono}", smallFont));
                    infoTable.AddCell(cellCliente);

                    // Celda factura
                    PdfPCell cellFactura = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingBottom = 8f };
                    cellFactura.AddElement(new Phrase($"FECHA EMISIÓN: {facturaEntity.fecha_emision:yyyy-MM-dd HH:mm}", bodyFont));
                    cellFactura.AddElement(new Phrase($"RESERVA ID: {facturaEntity.reserva_id}", bodyFont));
                    if (facturaEntity.Empleado != null)
                        cellFactura.AddElement(new Phrase($"ATENDIDO POR: {facturaEntity.Empleado.nombre} {facturaEntity.Empleado.apellido}", bodyFont));
                    cellFactura.AddElement(new Phrase($"ESTADO: {facturaEntity.estado.ToUpper()}", bodyFont));
                    infoTable.AddCell(cellFactura);

                    document.Add(infoTable);
                    document.Add(Chunk.NEWLINE);

                    // Tabla de detalles
                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 5f, 1.5f, 2f, 2f });
                    detailsTable.SpacingBefore = 10f;
                    detailsTable.AddCell(new PdfPCell(new Phrase("Descripción", smallBoldFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Padding = 5
                    });
                    detailsTable.AddCell(new PdfPCell(new Phrase("Cant.", smallBoldFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    });
                    detailsTable.AddCell(new PdfPCell(new Phrase("P. Unit.", smallBoldFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    });
                    detailsTable.AddCell(new PdfPCell(new Phrase("Subtotal", smallBoldFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    });

                    foreach (var detalle in facturaEntity.Detalle_Factura)
                    {
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.descripcion_concepto, smallFont)) { Padding = 5 });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.cantidad.ToString(), smallFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            Padding = 5
                        });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.precio_unitario.ToString("C"), smallFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            Padding = 5
                        });
                        detailsTable.AddCell(new PdfPCell(new Phrase(detalle.subtotal.ToString("C"), smallFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            Padding = 5
                        });
                    }
                    document.Add(detailsTable);
                    document.Add(Chunk.NEWLINE);

                    // Tabla de totales
                    PdfPTable totalsTable = new PdfPTable(2);
                    totalsTable.WidthPercentage = 40;
                    totalsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalsTable.SetWidths(new float[] { 2f, 1.5f });
                    totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    totalsTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalsTable.DefaultCell.Padding = 2;

                    totalsTable.AddCell(new Phrase("Subtotal:", headerFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.subtotal.ToString("C"), bodyFont));
                    totalsTable.AddCell(new Phrase("Impuestos:", headerFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.impuestos.ToString("C"), bodyFont));
                    totalsTable.AddCell(new Phrase("TOTAL:", titleFont));
                    totalsTable.AddCell(new Phrase(facturaEntity.total.ToString("C"), titleFont));

                    document.Add(totalsTable);
                    document.Add(Chunk.NEWLINE);

                    // Método de pago
                    document.Add(new Phrase($"Método de Pago: {facturaEntity.metodo_pago_factura}", bodyFont));

                    document.Close();
                    writer.Close();

                    // Preparar respuesta
                    byte[] pdfBytes = ms.ToArray();
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfBytes)
                    };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                    {
                        FileName = $"Factura-{facturaEntity.factura_id}.pdf"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error al generar PDF de factura: {ex.ToString()}")
                };
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}