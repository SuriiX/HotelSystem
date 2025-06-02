// File: ~/Controllers/TiposHabitacionController.cs
using HotelAuroraDreams.Api_Framework.Models; // Donde está HotelManagementSystemEntities y Tipo_Habitacion
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
    [RoutePrefix("api/TiposHabitacion")]
    [Authorize(Roles = "Administrador")]
    public class TiposHabitacionController : ApiController
    {
        private readonly TipoHabitacionService _service = new TipoHabitacionService();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetTiposHabitacion()
        {
            try
            {
                var tipos = await _service.ObtenerTodos();
                return Ok(tipos);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error al obtener tipos de habitación", ex));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetTipoHabitacionById")]
        public async Task<IHttpActionResult> GetTipoHabitacion(int id)
        {
            try
            {
                var tipo = await _service.ObtenerPorId(id);
                if (tipo == null) return NotFound();
                return Ok(tipo);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener tipo de habitación ID {id}", ex));
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(object))]
        public async Task<IHttpActionResult> PostTipoHabitacion(TipoHabitacionBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = await _service.Crear(model);
            if (!resultado.Exito)
            {
                ModelState.AddModelError("Nombre", resultado.Error);
                return BadRequest(ModelState);
            }

            return CreatedAtRoute("GetTipoHabitacionById", new { id = ((dynamic)resultado.Resultado).tipo_habitacion_id }, resultado.Resultado);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTipoHabitacion(int id, TipoHabitacionBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = await _service.Actualizar(id, model);
            if (!resultado.Exito)
            {
                ModelState.AddModelError("Nombre", resultado.Error);
                return BadRequest(ModelState);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteTipoHabitacion(int id)
        {
            var resultado = await _service.Eliminar(id);
            if (!resultado.Exito)
                return NotFound();

            return Ok(new { Message = "Tipo de habitación eliminado exitosamente.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _service.Dispose();
            base.Dispose(disposing);
        }
    } 
}