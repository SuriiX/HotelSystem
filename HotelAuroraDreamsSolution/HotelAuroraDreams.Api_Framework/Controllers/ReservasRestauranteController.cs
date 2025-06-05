// File: ~/Controllers/ReservasRestauranteController.csMore actions
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
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ReservasRestauranteController() { }
        public ReservasRestauranteController(ApplicationUserManager userManager) { UserManager = userManager; }

        [HttpPost]
        [Route("Disponibilidad")]
        [ResponseType(typeof(RestauranteDisponibleViewModel))]
        public async Task<IHttpActionResult> VerificarDisponibilidadRestaurante(RestauranteDisponibilidadRequestDto requestDto)
        {
            if (requestDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var restaurante = await db.Restaurantes.FindAsync(requestDto.RestauranteID);
                if (restaurante == null)
                {
                    return NotFoundResultWithMessage($"Restaurante con ID {requestDto.RestauranteID} no encontrado.");
                }
                if (!restaurante.capacidad_mesas.HasValue || restaurante.capacidad_mesas.Value <= 0)
                {
                    return Ok(new RestauranteDisponibleViewModel
                    {
                        RestauranteID = restaurante.restaurante_id,
                        NombreRestaurante = restaurante.nombre,
                        Disponible = false,
                        Mensaje = "Restaurante no tiene capacidad definida o no acepta reservas."
                    });
                }

                // Lógica de disponibilidad: Sumar comensales de reservas confirmadas/atendidas para esa fecha y una ventana de tiempo
                TimeSpan ventanaAntes = requestDto.Hora.Subtract(TimeSpan.FromHours(1));
                TimeSpan ventanaDespues = requestDto.Hora.Add(TimeSpan.FromHours(1));

                int comensalesReservados = await db.Reserva_Restaurante
                    .Where(rr => rr.restaurante_id == requestDto.RestauranteID &&
                                 rr.fecha_reserva == requestDto.Fecha.Date &&
                                 (rr.estado == "Confirmada" || rr.estado == "Atendida") &&
                                 (rr.hora_reserva >= ventanaAntes && rr.hora_reserva <= ventanaDespues))
                    .SumAsync(rr => (int?)rr.numero_comensales) ?? 0;

                int capacidadRestante = restaurante.capacidad_mesas.Value - comensalesReservados;

                if (capacidadRestante >= requestDto.NumeroComensales)
                {
                    return Ok(new RestauranteDisponibleViewModel
                    {
                        RestauranteID = restaurante.restaurante_id,
                        NombreRestaurante = restaurante.nombre,
                        Disponible = true,
                        Mensaje = "Restaurante disponible.",
                        CapacidadRestanteEstimada = capacidadRestante - requestDto.NumeroComensales
                    });
                }
                else
                {
                    return Ok(new RestauranteDisponibleViewModel
                    {
                        RestauranteID = restaurante.restaurante_id,
                        NombreRestaurante = restaurante.nombre,
                        Disponible = false,
                        Mensaje = $"No hay suficiente capacidad. Capacidad restante estimada: {Math.Max(0, capacidadRestante)} comensales.",
                        CapacidadRestanteEstimada = Math.Max(0, capacidadRestante)
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al verificar disponibilidad del restaurante: {ex.ToString()}"));
            }
        }



        // POST: api/ReservasRestaurante
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ReservaRestauranteViewModel))]
        public async Task<IHttpActionResult> PostReservaRestaurante(ReservaRestauranteBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string empleadoAspNetUserId = User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return BadRequest("No se encontró el registro de empleado para el usuario autenticado.");


            }
            int empleadoRegistroIdInt = empleadoDb.empleado_id;

            // Re-verificar disponibilidad (simplificado, podrías reusar la lógica de VerificarDisponibilidadRestaurante)
            var restaurante = await db.Restaurantes.FindAsync(model.RestauranteID);
            if (restaurante == null) return BadRequest("Restaurante no válido.");
            // Aquí podrías añadir una verificación de capacidad más robusta como en el endpoint de disponibilidad.
            // La UNIQUE constraint en la BD (restaurante_id, fecha_reserva, hora_reserva) previene duplicados exactos.

            try
            {
                Reserva_Restaurante nuevaReserva = new Reserva_Restaurante
                {
                    cliente_id = model.ClienteID,
                    restaurante_id = model.RestauranteID,
                    fecha_reserva = model.FechaReserva.Date, // Solo la fecha
                    hora_reserva = model.HoraReserva,
                    numero_comensales = model.NumeroComensales,
                    estado = "Confirmada", // Estado por defecto
                    notas = model.Notas,
                    empleado_registro_id = empleadoRegistroIdInt
                };

                db.Reserva_Restaurante.Add(nuevaReserva);
                await db.SaveChangesAsync();

                var cliente = await db.Clientes.FindAsync(nuevaReserva.cliente_id);

                var viewModel = new ReservaRestauranteViewModel
                {
                    ReservaRestauranteID = nuevaReserva.reserva_restaurante_id,
                    ClienteID = nuevaReserva.cliente_id,
                    NombreCliente = cliente != null ? $"{cliente.nombre} {cliente.apellido}" : "N/A",
                    RestauranteID = nuevaReserva.restaurante_id,
                    NombreRestaurante = restaurante.nombre,
                    FechaReserva = nuevaReserva.fecha_reserva,
                    HoraReserva = nuevaReserva.hora_reserva,
                    NumeroComensales = nuevaReserva.numero_comensales,
                    Estado = nuevaReserva.estado,
                    Notas = nuevaReserva.notas,
                    NombreEmpleadoRegistro = $"{empleadoDb.nombre} {empleadoDb.apellido}"
                };

                return CreatedAtRoute("GetReservaRestauranteById", new { id = nuevaReserva.reserva_restaurante_id }, viewModel);
            }
            catch (DbUpdateException dbEx)
            {
                // Verificar si es por la UNIQUE constraint
                if (dbEx.InnerException?.InnerException?.Message.Contains("UNIQUE KEY constraint") ?? false)
                {
                    return Content(HttpStatusCode.Conflict, new { Message = "Ya existe una reserva para este restaurante en la misma fecha y hora." });
                }
                return InternalServerError(new Exception($"DbUpdateException al crear reserva de restaurante: {dbEx.ToString()}"));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error general al crear reserva de restaurante: {ex.ToString()}"));
            }
        }

        // GET: api/ReservasRestaurante
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservasRestaurante([FromUri] int? clienteId = null, [FromUri] int? restauranteId = null, [FromUri] DateTime? fecha = null)
        {
            try
            {
                var query = db.Reserva_Restaurante.AsQueryable();
                if (clienteId.HasValue) query = query.Where(rr => rr.cliente_id == clienteId.Value);
                if (restauranteId.HasValue) query = query.Where(rr => rr.restaurante_id == restauranteId.Value);
                if (fecha.HasValue) query = query.Where(rr => rr.fecha_reserva == fecha.Value.Date);

                var reservas = await query
                    .OrderByDescending(rr => rr.fecha_reserva).ThenBy(rr => rr.hora_reserva)
                    .Select(rr => new ReservaRestauranteViewModel
                    {
                        ReservaRestauranteID = rr.reserva_restaurante_id,
                        ClienteID = rr.cliente_id,
                        NombreCliente = rr.Cliente.nombre + " " + rr.Cliente.apellido, 
                        RestauranteID = rr.restaurante_id,
                        NombreRestaurante = rr.Restaurante.nombre, 
                        FechaReserva = rr.fecha_reserva, 
                        HoraReserva = rr.hora_reserva,   
                        NumeroComensales = rr.numero_comensales,
                        Estado = rr.estado,
                        Notas = rr.notas,
                        NombreEmpleadoRegistro = rr.Empleado != null ? rr.Empleado.nombre + " " + rr.Empleado.apellido : null // Necesita Include(rr => rr.Empleado)
                    })
                    .ToListAsync();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reservas de restaurante: {ex.ToString()}"));
            }
        }


        // GET: api/ReservasRestaurante/5
        [HttpGet]
        [Route("{id:int}", Name = "GetReservaRestauranteById")]
        [ResponseType(typeof(ReservaRestauranteViewModel))]
        public async Task<IHttpActionResult> GetReservaRestaurante(int id)
        {
            try
            {
                var rr = await db.Reserva_Restaurante
                    .Include(r => r.Cliente)
                    .Include(r => r.Restaurante)
                    .Include(r => r.Empleado)
                    .FirstOrDefaultAsync(r => r.reserva_restaurante_id == id);

                if (rr == null) return NotFound();

                var viewModel = new ReservaRestauranteViewModel
                {
                    ReservaRestauranteID = rr.reserva_restaurante_id,
                    ClienteID = rr.cliente_id,
                    NombreCliente = $"{rr.Cliente.nombre} {rr.Cliente.apellido}",
                    RestauranteID = rr.restaurante_id,
                    NombreRestaurante = rr.Restaurante.nombre,
                    FechaReserva = rr.fecha_reserva,
                    HoraReserva = rr.hora_reserva,
                    NumeroComensales = rr.numero_comensales,
                    Estado = rr.estado,
                    Notas = rr.notas,
                    NombreEmpleadoRegistro = rr.Empleado != null ? $"{rr.Empleado.nombre} {rr.Empleado.apellido}" : "N/A"
                };
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reserva de restaurante ID {id}: {ex.ToString()}"));
            }
        }

        // PUT: api/ReservasRestaurante/5
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReservaRestaurante(int id, ReservaRestauranteUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var reserva = await db.Reserva_Restaurante.FindAsync(id);
            if (reserva == null) return NotFound();

            // Lógica de validación de qué se puede actualizar y si se requiere re-verificar disponibilidad
            reserva.numero_comensales = model.NumeroComensales;
            reserva.estado = model.Estado;
            reserva.notas = model.Notas;
            // Actualizar Fecha/Hora requeriría verificar disponibilidad de nuevo.

            db.Entry(reserva).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(new Exception($"Error de concurrencia: {ex.ToString()}"));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar reserva de restaurante: {ex.ToString()}"));
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE (o Cancelar): api/ReservasRestaurante/5
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteReservaRestaurante(int id)
        {
            var reserva = await db.Reserva_Restaurante.FindAsync(id);
            if (reserva == null) return NotFound();

            // En lugar de eliminar, es mejor cambiar el estado a "Cancelada"
            if (reserva.estado == "Cancelada" || reserva.estado == "Atendida")
            {
                return Ok(new { Message = $"La reserva de restaurante ya estaba {reserva.estado.ToLower()}." });

            }
            reserva.estado = "Cancelada";
            db.Entry(reserva).State = EntityState.Modified;

            // O si realmente quieres eliminar: db.Reserva_Restaurante.Remove(reserva);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al cancelar/eliminar reserva de restaurante: {ex.ToString()}"));
            }
            return Ok(new { Message = "Reserva de restaurante cancelada exitosamente.", Id = id });
        }


        private IHttpActionResult NotFoundResultWithMessage(string message)
        {
            return Content(HttpStatusCode.NotFound, new { Message = message });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null) _userManager.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}