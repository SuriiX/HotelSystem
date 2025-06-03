using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Reservas")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class ReservasController : ApiController
    {
        private readonly ClsReserva _servicioReserva;

        public ReservasController()
        {
            _servicioReserva = new ClsReserva();
        }

        [HttpPost]
        [Route("Disponibilidad")]
        [ResponseType(typeof(List<HabitacionDisponibleDto>))]
        public async Task<IHttpActionResult> VerificarDisponibilidad([FromBody] DisponibilidadRequestDto requestDto)
        {
            try
            {
                var resultado = await _servicioReserva.VerificarDisponibilidad(requestDto);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al verificar disponibilidad: {ex.Message}"));
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ReservaViewModel))]
        public async Task<IHttpActionResult> PostReserva(ReservaCreacionBindingModel model)
        {
            try
            {
                string empleadoAspNetUserId = User.Identity.GetUserId();
                var resultado = await _servicioReserva.CrearReserva(model, empleadoAspNetUserId);
                return CreatedAtRoute("GetReservaById", new { id = resultado.ReservaID }, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al crear reserva: {ex.Message}"));
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservas([FromUri] int? clienteId = null, [FromUri] int? hotelId = null,
            [FromUri] DateTime? fechaDesde = null, [FromUri] DateTime? fechaHasta = null, [FromUri] string estado = null)
        {
            try
            {
                var resultado = await _servicioReserva.ObtenerReservas(clienteId, hotelId, fechaDesde, fechaHasta, estado);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reservas: {ex.Message}"));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetReservaById")]
        public async Task<IHttpActionResult> GetReserva(int id)
        {
            try
            {
                var resultado = await _servicioReserva.ObtenerReservaPorId(id);
                return Ok(resultado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reserva: {ex.Message}"));
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReserva(int id, ReservaUpdateBindingModel model)
        {
            try
            {
                await _servicioReserva.ActualizarReserva(id, model);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar reserva: {ex.Message}"));
            }
        }

        [HttpPost]
        [Route("{id:int}/Cancelar")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> CancelarReserva(int id)
        {
            try
            {
                await _servicioReserva.CancelarReserva(id);
                return Ok(new { Message = "Reserva cancelada exitosamente." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al cancelar reserva: {ex.Message}"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _servicioReserva.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}