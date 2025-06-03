// File: ~/Controllers/SalonesEventoController.cs
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
    [RoutePrefix("api/SalonesEvento")]
    [Authorize(Roles = "Administrador")]
    public class SalonesEventoController : ApiController
    {
        private ClsSalonEvento logic = new ClsSalonEvento();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetSalonesEvento([FromUri] int? hotelId = null)
        {
            try
            {
                var salones = await logic.GetSalonesEventoAsync(hotelId);
                return Ok(salones);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener salones: {ex.Message}", ex.InnerException));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetSalonEventoById")]
        public async Task<IHttpActionResult> GetSalonEvento(int id)
        {
            var salonViewModel = await logic.GetSalonEventoAsync(id);
            if (salonViewModel == null) return NotFound();
            return Ok(salonViewModel);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(SalonEventoViewModel))]
        public async Task<IHttpActionResult> PostSalonEvento(SalonEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdSalon = await logic.CreateSalonEventoAsync(model);
            return CreatedAtRoute("GetSalonEventoById", new { id = createdSalon.SalonEventoID }, createdSalon);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSalonEvento(int id, SalonEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await logic.UpdateSalonEventoAsync(id, model);
            if (!updated) return NotFound();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteSalonEvento(int id)
        {
            var deleted = await logic.DeleteSalonEventoAsync(id);
            if (!deleted) return NotFound();

            return Ok(new { Message = "Salón de evento eliminado.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) logic.Dispose();
            base.Dispose(disposing);
        }
    }
}