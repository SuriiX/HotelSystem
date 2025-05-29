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

    [RoutePrefix("api/CheckIn")]
    public class CheckInsController : ApiController
    {

        [HttpGet]
        [Route("ConsultarTodos")]
        public List<CheckIn> ConsultarTodos()
        {
            ClsChekIn checkIn = new ClsChekIn();
            return checkIn.ConsultarTodos();
        }

        [HttpGet]
        [Route("ConsultarPorReserva")]
        public CheckIn ConsultarPorReserva(int reservaId)
        {
            ClsChekIn checkIn = new ClsChekIn();
            return checkIn.ConsultarPorReserva(reservaId);
        }

        [HttpPost]
        [Route("Insertar")]
        public string Insertar([FromBody] CheckIn nuevoCheckIn)
        {
            ClsChekIn checkIn = new ClsChekIn();
            checkIn.checkin = nuevoCheckIn;
            return checkIn.Insertar();
        }

    }
}