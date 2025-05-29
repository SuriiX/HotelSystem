using HotalSystem.Clases;
using HotalSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HotalSystem.Controllers
{

    [RoutePrefix("api/Reservas")]
    public class ReservasController : ApiController
    {
        [HttpPost]
        [Route("Insertar")]
        public string Insertar([FromBody] Reserva reserva)
        {
            ClsReserva cls = new ClsReserva();
            cls.reserva = reserva;
            return cls.Insertar();
        }

        [HttpGet]
        [Route("ConsultarActivas")]
        public List<Reserva> ConsultarActivas()
        {
            ClsReserva cls = new ClsReserva();
            return cls.ObtenerReservasActivas();
        }

        [HttpGet]
        [Route("ConsultarPorId/{id}")]
        public IHttpActionResult ConsultarPorId(int id)
        {
            ClsReserva cls = new ClsReserva();
            var reserva = cls.ConsultarPorId(id);
            if (reserva == null)
                return NotFound();

            return Ok(reserva);
        }

        [HttpPut]
        [Route("Cancelar/{id}")]
        public string Cancelar(int id)
        {
            ClsReserva cls = new ClsReserva();
            return cls.Cancelar(id);
        }

        [HttpPut]
        [Route("Actualizar")]
        public string Actualizar([FromBody] Reserva reserva)
        {
            ClsReserva cls = new ClsReserva();
            cls.reserva = reserva;
            return cls.Actualizar();
        }
    }
}
