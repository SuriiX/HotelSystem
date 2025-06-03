using HotelAuroraDreams.Api_Framework.Clases;
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
    [RoutePrefix("api/Clientes")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class ClientesController : ApiController
    {
        private readonly ClsCliente clsCliente = new ClsCliente();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetClientes()
        {
            try
            {
                var clientes = await clsCliente.GetClientesAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener clientes: {ex.Message}", ex.InnerException));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetClienteById")]
        public async Task<IHttpActionResult> GetCliente(int id)
        {
            try
            {
                var cliente = await clsCliente.GetClienteByIdAsync(id);
                if (cliente == null)
                    return NotFound();

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener cliente ID {id}: {ex.Message}", ex.InnerException));
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ClienteViewModel))]
        public async Task<IHttpActionResult> PostCliente(ClienteBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await clsCliente.ExisteClienteAsync(model.TipoDocumento, model.NumeroDocumento))
            {
                ModelState.AddModelError("NumeroDocumento", "Ya existe un cliente con este tipo y número de documento.");
                return BadRequest(ModelState);
            }

            try
            {
                var cliente = await clsCliente.CrearClienteAsync(model);
                return CreatedAtRoute("GetClienteById", new { id = cliente.ClienteID }, cliente);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al crear cliente: {ex.Message}", ex.InnerException));
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCliente(int id, ClienteBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await clsCliente.ExisteClienteAsync(model.TipoDocumento, model.NumeroDocumento, id))
            {
                ModelState.AddModelError("NumeroDocumento", "Ya existe otro cliente con este tipo y número de documento.");
                return BadRequest(ModelState);
            }

            try
            {
                var actualizado = await clsCliente.ActualizarClienteAsync(id, model);
                if (!actualizado)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar cliente: {ex.Message}", ex.InnerException));
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IHttpActionResult> DeleteCliente(int id)
        {
            try
            {
                var eliminado = await clsCliente.EliminarClienteAsync(id);
                if (!eliminado)
                    return NotFound();

                return Ok(new { Message = "Cliente eliminado exitosamente.", Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al eliminar cliente: {ex.Message}. Verifique dependencias.", ex.InnerException));
            }
        }
    }
}