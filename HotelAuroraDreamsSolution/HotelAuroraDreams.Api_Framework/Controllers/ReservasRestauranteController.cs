// File: ~/Controllers/ReservasRestauranteController.cs
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
    [RoutePrefix("api/ReservasRestaurante")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class ReservasRestauranteController : ApiController
    {
        private ClsReservaRestaurante _reservaService;

        public ReservasRestauranteController()
        {
            _reservaService = new ClsReservaRestaurante(Request.GetOwinContext().GetUserManager<ApplicationUserManager>());
        }

        [HttpPost]
        [Route("Disponibilidad")]
        public async Task<IHttpActionResult> VerificarDisponibilidadRestaurante(RestauranteDisponibilidadRequestDto requestDto)
        {
            if (requestDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resultado = await _reservaService.VerificarDisponibilidadRestaurante(requestDto);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return Content(HttpStatusCode.NotFound, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PostReservaRestaurante(ReservaRestauranteBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.Identity.GetUserId();
                var reservaCreada = await _reservaService.CrearReserva(model, userId);
                return CreatedAtRoute("GetReservaRestauranteById", new { id = reservaCreada.ReservaRestauranteID }, reservaCreada);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.InnerException?.Message.Contains("UNIQUE KEY constraint") ?? false)
                {
                    return Content(HttpStatusCode.Conflict, new { Message = "Ya existe una reserva para este restaurante en la misma fecha y hora." });
                }
                return InternalServerError(dbEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservasRestaurante([FromUri] int? clienteId = null, [FromUri] int? restauranteId = null, [FromUri] DateTime? fecha = null)
        {
            try
            {
                var reservas = await _reservaService.ObtenerReservas(clienteId, restauranteId, fecha);
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetReservaRestauranteById")]
        public async Task<IHttpActionResult> GetReservaRestaurante(int id)
        {
            try
            {
                var reserva = await _reservaService.ObtenerReservaPorId(id);
                if (reserva == null) return NotFound();
                return Ok(reserva);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutReservaRestaurante(int id, ReservaRestauranteUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _reservaService.ActualizarReserva(id, model);
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteReservaRestaurante(int id)
        {
            try
            {
                await _reservaService.CancelarReserva(id);
                return Ok(new { Message = "Reserva de restaurante cancelada exitosamente.", Id = id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reservaService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}