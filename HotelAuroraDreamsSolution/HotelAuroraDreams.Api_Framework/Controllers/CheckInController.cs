// File: ~/Controllers/CheckInController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
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
    [RoutePrefix("api/CheckIn")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class CheckInController : ApiController
    {
        private readonly ClsCheckIn _servicio = new ClsCheckIn();

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PostCheckIn(CheckInBindingModel model)
        {
            return await _servicio.ProcesarCheckIn(model);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetCheckInById")]
        public async Task<IHttpActionResult> GetCheckIn(int id)
        {
            return await _servicio.ObtenerCheckInPorId(id);
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