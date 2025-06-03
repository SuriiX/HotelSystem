// File: ~/Controllers/ReservasEventoController.cs
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
    [RoutePrefix("api/ReservasEvento")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class ReservasEventoController : ApiController
    {
        private readonly ClsReservaEvento _servicio = new ClsReservaEvento();

        [HttpPost]
        [Route("DisponibilidadSalon")]
        public async Task<IHttpActionResult> VerificarDisponibilidadSalon(SalonDisponibilidadRequestDto requestDto)
        {
            return await _servicio.VerificarDisponibilidadSalon(requestDto);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PostReservaEvento(ReservaEventoCreacionBindingModel model)
        {
            return await _servicio.CrearReservaEvento(model, User.Identity.GetUserId());
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservasEvento([FromUri] int? clienteId = null, [FromUri] int? salonId = null, [FromUri] DateTime? fecha = null)
        {
            return await _servicio.ObtenerReservasEvento(clienteId, salonId, fecha);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetReservaEventoById")]
        public async Task<IHttpActionResult> GetReservaEvento(int id)
        {
            return await _servicio.ObtenerReservaEventoPorId(id);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutReservaEvento(int id, ReservaEventoUpdateBindingModel model)
        {
            return await _servicio.ActualizarReservaEvento(id, model);
        }

        [HttpPost]
        [Route("{id:int}/Cancelar")]
        public async Task<IHttpActionResult> CancelarReservaEvento(int id)
        {
            return await _servicio.CancelarReservaEvento(id);
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