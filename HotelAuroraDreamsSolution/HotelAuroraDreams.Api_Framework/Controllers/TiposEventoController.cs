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
    [RoutePrefix("api/TiposEvento")]
    [Authorize(Roles = "Administrador")]
    public class TiposEventoController : ApiController
    {
        private readonly ClsTipoEvento servicio = new ClsTipoEvento();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetTiposEvento()
        {
            var tipos = await servicio.ObtenerTodosAsync();
            return Ok(tipos);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetTipoEventoById")]
        public async Task<IHttpActionResult> GetTipoEvento(int id)
        {
            var tipo = await servicio.ObtenerPorIdAsync(id);
            if (tipo == null) return NotFound();
            return Ok(tipo);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(TipoEventoViewModel))]
        public async Task<IHttpActionResult> PostTipoEvento(TipoEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (existe, mensaje) = await servicio.ExisteNombreTipoAsync(model.NombreTipo);
            if (existe)
            {
                ModelState.AddModelError("NombreTipo", mensaje);
                return BadRequest(ModelState);
            }

            var creado = await servicio.CrearAsync(model);
            return CreatedAtRoute("GetTipoEventoById", new { id = creado.TipoEventoID }, creado);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTipoEvento(int id, TipoEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (existe, mensaje) = await servicio.ExisteNombreTipoAsync(model.NombreTipo, id);
            if (existe)
            {
                ModelState.AddModelError("NombreTipo", mensaje);
                return BadRequest(ModelState);
            }

            var actualizado = await servicio.ActualizarAsync(id, model);
            if (!actualizado) return NotFound();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteTipoEvento(int id)
        {
            var eliminado = await servicio.EliminarAsync(id);
            if (!eliminado) return NotFound();

            return Ok(new { Message = "Tipo de evento eliminado.", Id = id });
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