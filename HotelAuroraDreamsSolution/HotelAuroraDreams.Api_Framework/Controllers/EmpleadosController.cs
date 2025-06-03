// File: ~/Controllers/EmpleadosController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity; // Para ToListAsync en el contexto EDMX
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Empleados")]
    [Authorize(Roles = "Administrador")]
    public class EmpleadosController : ApiController
    {
        private readonly ClsEmpleado _clsEmpleado = new ClsEmpleado();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetEmpleados()
        {
            var empleados = await _clsEmpleado.GetEmpleados();
            return Ok(empleados);
        }

        [HttpGet]
        [Route("{id}", Name = "GetEmpleadoById")]
        public async Task<IHttpActionResult> GetEmpleado(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID de empleado no puede ser vacío.");

            var empleado = await _clsEmpleado.GetEmpleado(id);
            if (empleado == null) return NotFound();

            return Ok(empleado);
        }

        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmpleado(string id, EmpleadoUpdateBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _clsEmpleado.UpdateEmpleado(id, model);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return BadRequest(ModelState);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> DeleteEmpleado(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID de empleado no puede ser vacío.");

            var result = await _clsEmpleado.DeleteEmpleado(id);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return BadRequest(ModelState);
            }

            return Ok(new { Message = "Empleado eliminado exitosamente.", Id = id });
        }
    }
}