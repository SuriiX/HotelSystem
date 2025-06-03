using HotelAuroraDreams.Api_Framework.Clases;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/ServiciosAdicionalesEvento")]
    [Authorize(Roles = "Administrador")]
    public class ServiciosAdicionalesEventoController : ApiController
    {
        private readonly ClsServiciosAdicionalesEvento servicio = new ClsServiciosAdicionalesEvento();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetServiciosAdicionalesEvento()
        {
            var lista = await servicio.ObtenerTodosAsync();
            return Ok(lista);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetServicioAdicionalEventoById")]
        public async Task<IHttpActionResult> GetServicioAdicionalEvento(int id)
        {
            var servicioDetalle = await servicio.ObtenerPorIdAsync(id);
            if (servicioDetalle == null) return NotFound();
            return Ok(servicioDetalle);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ServicioAdicionalEventoViewModel))]
        public async Task<IHttpActionResult> PostServicioAdicionalEvento(ServicioAdicionalEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (existe, mensaje) = await servicio.ExisteNombreServicioAsync(model.NombreServicio);
            if (existe)
            {
                ModelState.AddModelError("Nombre", mensaje);
                return BadRequest(ModelState);
            }

            var creado = await servicio.CrearAsync(model);
            return CreatedAtRoute("GetServicioAdicionalEventoById", new { id = creado.ServicioAdicionalID }, creado);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutServicioAdicionalEvento(int id, ServicioAdicionalEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (existe, mensaje) = await servicio.ExisteNombreServicioAsync(model.NombreServicio, id);
            if (existe)
            {
                ModelState.AddModelError("Nombre", mensaje);
                return BadRequest(ModelState);
            }

            var actualizado = await servicio.ActualizarAsync(id, model);
            if (!actualizado) return NotFound();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteServicioAdicionalEvento(int id)
        {
            var eliminado = await servicio.EliminarAsync(id);
            if (!eliminado) return NotFound();

            return Ok(new { Message = "Servicio adicional eliminado.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                servicio.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}