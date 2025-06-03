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
    [RoutePrefix("api/Facturas")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class FacturasController : ApiController
    {
        private readonly ClsFactura _servicio = new ClsFactura();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetFacturas([FromUri] int? clienteId = null, [FromUri] int? reservaId = null,
            [FromUri] DateTime? fechaDesde = null, [FromUri] DateTime? fechaHasta = null, [FromUri] string estado = null)
        {
            return await _servicio.ObtenerFacturas(clienteId, reservaId, fechaDesde, fechaHasta, estado);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetFacturaById")]
        public async Task<IHttpActionResult> GetFactura(int id)
        {
            return await _servicio.ObtenerFacturaPorId(id);
        }

        [HttpGet]
        [Route("{id:int}/pdf")]
        public async Task<HttpResponseMessage> GetFacturaPdf(int id)
        {
            return await _servicio.GenerarPdfFactura(id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _servicio.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}