// File: ~/Controllers/ClientesController.cs
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
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<ClienteViewModel>))]
        public async Task<IHttpActionResult> GetClientes()
        {
            try
            {
                var clientes = await db.Clientes
                    .Select(c => new ClienteViewModel
                    {
                        ClienteID = c.cliente_id,
                        Nombre = c.nombre,
                        Apellido = c.apellido,
                        TipoDocumento = c.tipo_documento,
                        NumeroDocumento = c.numero_documento,
                        Email = c.email,
                        Telefono = c.telefono,
                        Direccion = c.direccion,
                        CiudadResidenciaID = c.ciudad_residencia_id,
                        NombreCiudadResidencia = c.Ciudad != null ? c.Ciudad.nombre_ciudad : null,
                        FechaNacimiento = c.fecha_nacimiento,
                        FechaRegistro = (DateTime)c.fecha_registro
                    })
                    .OrderBy(c => c.Apellido)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener clientes: {ex.ToString()}"));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetClienteById")]
        [ResponseType(typeof(ClienteViewModel))]
        public async Task<IHttpActionResult> GetCliente(int id)
        {
            try
            {
                var clienteViewModel = await db.Clientes
                    .Where(c => c.cliente_id == id)
                    .Select(c => new ClienteViewModel
                    {
                        ClienteID = c.cliente_id,
                        Nombre = c.nombre,
                        Apellido = c.apellido,
                        TipoDocumento = c.tipo_documento,
                        NumeroDocumento = c.numero_documento,
                        Email = c.email,
                        Telefono = c.telefono,
                        Direccion = c.direccion,
                        CiudadResidenciaID = c.ciudad_residencia_id,
                        NombreCiudadResidencia = c.Ciudad != null ? c.Ciudad.nombre_ciudad : null,
                        FechaNacimiento = c.fecha_nacimiento,
                        FechaRegistro = (DateTime)c.fecha_registro
                    })
                    .FirstOrDefaultAsync();

                if (clienteViewModel == null)
                {
                    return NotFound();
                }
                return Ok(clienteViewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener cliente ID {id}: {ex.ToString()}"));
            }
        }

        [HttpGet]
        [Route("Buscar")]
        [ResponseType(typeof(List<ClienteListItemDto>))]
        public async Task<IHttpActionResult> BuscarClientes([FromUri] string terminoBusqueda)
        {
            if (string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                return Ok(new List<ClienteListItemDto>());
            }

            try
            {
                string busquedaLower = terminoBusqueda.ToLower().Trim();
                var clientesEncontrados = await db.Clientes
                    .Where(c =>
                        (c.nombre + " " + c.apellido).ToLower().Contains(busquedaLower) ||
                        (c.apellido + " " + c.nombre).ToLower().Contains(busquedaLower) ||
                        c.numero_documento.Contains(terminoBusqueda) ||
                        (c.email != null && c.email.ToLower().Contains(busquedaLower))
                    )
                    .Select(c => new ClienteListItemDto
                    {
                        ClienteID = c.cliente_id,
                        NombreCompleto = c.apellido + ", " + c.nombre + " (Doc: " + c.numero_documento + ")",
                        NumeroDocumento = c.numero_documento,
                        Email = c.email
                    })
                    .OrderBy(c => c.NombreCompleto)
                    .Take(20)
                    .ToListAsync();

                return Ok(clientesEncontrados);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al buscar clientes: {ex.ToString()}"));
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ClienteViewModel))]
        public async Task<IHttpActionResult> PostCliente(ClienteBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await db.Clientes.AnyAsync(c => c.numero_documento == model.NumeroDocumento && c.tipo_documento == model.TipoDocumento))
            {
                ModelState.AddModelError("NumeroDocumento", "Ya existe un cliente con este tipo y número de documento.");
                return BadRequest(ModelState);
            }

            Cliente clienteEntity = new Cliente
            {
                nombre = model.Nombre,
                apellido = model.Apellido,
                tipo_documento = model.TipoDocumento,
                numero_documento = model.NumeroDocumento,
                email = model.Email,
                telefono = model.Telefono,
                direccion = model.Direccion,
                ciudad_residencia_id = model.CiudadResidenciaID,
                fecha_nacimiento = model.FechaNacimiento,
                fecha_registro = DateTime.Now
            };

            db.Clientes.Add(clienteEntity);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al crear cliente: {ex.ToString()}", ex.InnerException));
            }

            string nombreCiudad = null;
            if (clienteEntity.ciudad_residencia_id.HasValue)
            {
                var ciudad = await db.Ciudads.FindAsync(clienteEntity.ciudad_residencia_id.Value);
                nombreCiudad = ciudad?.nombre_ciudad;
            }

            var viewModel = new ClienteViewModel
            {
                ClienteID = clienteEntity.cliente_id,
                Nombre = clienteEntity.nombre,
                Apellido = clienteEntity.apellido,
                TipoDocumento = clienteEntity.tipo_documento,
                NumeroDocumento = clienteEntity.numero_documento,
                Email = clienteEntity.email,
                Telefono = clienteEntity.telefono,
                Direccion = clienteEntity.direccion,
                CiudadResidenciaID = clienteEntity.ciudad_residencia_id,
                NombreCiudadResidencia = nombreCiudad,
                FechaNacimiento = clienteEntity.fecha_nacimiento,
                FechaRegistro = (DateTime)clienteEntity.fecha_registro
            };

            return CreatedAtRoute("GetClienteById", new { id = clienteEntity.cliente_id }, viewModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCliente(int id, ClienteBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clienteEntity = await db.Clientes.FindAsync(id);
            if (clienteEntity == null)
            {
                return NotFound();
            }

            if (await db.Clientes.AnyAsync(c => c.numero_documento == model.NumeroDocumento && c.tipo_documento == model.TipoDocumento && c.cliente_id != id))
            {
                ModelState.AddModelError("NumeroDocumento", "Ya existe otro cliente con este tipo y número de documento.");
                return BadRequest(ModelState);
            }

            clienteEntity.nombre = model.Nombre;
            clienteEntity.apellido = model.Apellido;
            clienteEntity.tipo_documento = model.TipoDocumento;
            clienteEntity.numero_documento = model.NumeroDocumento;
            clienteEntity.email = model.Email;
            clienteEntity.telefono = model.Telefono;
            clienteEntity.direccion = model.Direccion;
            clienteEntity.ciudad_residencia_id = model.CiudadResidenciaID;
            clienteEntity.fecha_nacimiento = model.FechaNacimiento;

            db.Entry(clienteEntity).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(new Exception($"Error de concurrencia al actualizar cliente: {ex.ToString()}", ex.InnerException));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar cliente: {ex.ToString()}", ex.InnerException));
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IHttpActionResult> DeleteCliente(int id)
        {
            Cliente cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            bool tieneReservas = await db.Reservas.AnyAsync(r => r.cliente_id == id); // Asumiendo DbSet Reserva
            if (tieneReservas)
            {
                return Content(HttpStatusCode.Conflict, new { Message = "No se puede eliminar el cliente porque tiene reservas de habitación asociadas." });
            }
            // Añadir verificación similar para ReservaEvento si es necesario
            bool tieneReservasEvento = await db.ReservaEventoes.AnyAsync(re => re.ClienteID == id); // Asumiendo DbSet ReservaEventoes
            if (tieneReservasEvento)
            {
                return Content(HttpStatusCode.Conflict, new { Message = "No se puede eliminar el cliente porque tiene reservas de evento asociadas." });
            }


            db.Clientes.Remove(cliente);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al eliminar cliente: {ex.ToString()}", ex.InnerException));
            }

            return Ok(new { Message = "Cliente eliminado exitosamente.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}