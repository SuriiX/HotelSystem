// File: ~/Controllers/CheckOutController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic; // Para List
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
    namespace HotelAuroraDreams.Api_Framework.Controllers
    {
        [RoutePrefix("api/CheckOut")]
        [Authorize(Roles = "Empleado, Administrador")]
        public class CheckOutController : ApiController
        {
            private readonly ClsCheckOut _servicio = new ClsCheckOut();

            [HttpPost]
            [Route("")]
            public async Task<IHttpActionResult> PostCheckOut(CheckOutBindingModel model)
            {
                return await _servicio.ProcesarCheckOut(model);
            }

            [HttpGet]
            [Route("{id:int}", Name = "GetCheckOutById")]
            public async Task<IHttpActionResult> GetCheckOut(int id)
            {
                return await _servicio.ObtenerCheckOutPorId(id);
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
}