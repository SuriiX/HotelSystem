using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Habitaciones")]
    [Authorize(Roles = "Administrador")]
    public class HabitacionesController : ApiController
    {
        private readonly ClsHabitacion _logic = new ClsHabitacion();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetHabitaciones([FromUri] int? hotelId = null, [FromUri] int? tipoHabitacionId = null, [FromUri] string estado = null)
        {
            try
            {
                var result = await _logic.GetHabitaciones(hotelId, tipoHabitacionId, estado);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener habitaciones: {ex.Message}", ex));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetHabitacionById")]
        public async Task<IHttpActionResult> GetHabitacion(int id)
        {
            try
            {
                var result = await _logic.GetHabitacion(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(HabitacionViewModel))]
        public async Task<IHttpActionResult> PostHabitacion(HabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (existe, error) = await _logic.ValidarExistenciaNumero(model.HotelID, model.Numero);
            if (existe)
            {
                ModelState.AddModelError("Numero", error);
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _logic.CrearHabitacion(model);
                return CreatedAtRoute("GetHabitacionById", new { id = created.HabitacionID }, created);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutHabitacion(int id, HabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (existe, error) = await _logic.ValidarExistenciaNumero(model.HotelID, model.Numero, id);
            if (existe)
            {
                ModelState.AddModelError("Numero", error);
                return BadRequest(ModelState);
            }

            try
            {
                bool actualizado = await _logic.ActualizarHabitacion(id, model);
                if (!actualizado) return NotFound();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteHabitacion(int id)
        {
            try
            {
                bool eliminado = await _logic.EliminarHabitacion(id);
                if (!eliminado) return NotFound();
                return Ok(new { Message = "Habitación eliminada exitosamente.", Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}