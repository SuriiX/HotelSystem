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
    [Authorize(Roles = "Empleado, Administrador")] // Empleados pueden crear y ver, Admin puede todo
    public class ReservasEventoController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ReservasEventoController() { }
        public ReservasEventoController(ApplicationUserManager userManager) { UserManager = userManager; }

        // POST: api/ReservasEvento/DisponibilidadSalon
        [HttpPost]
        [Route("DisponibilidadSalon")]
        [ResponseType(typeof(SalonDisponibleViewModel))]
        public async Task<IHttpActionResult> VerificarDisponibilidadSalon(SalonDisponibilidadRequestDto requestDto)
        {
            if (requestDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (requestDto.HoraInicio >= requestDto.HoraFin)
            {
                return BadRequest("La hora de fin debe ser posterior a la hora de inicio.");
            }

            try
            {
                var salon = await db.SalonEventoes.FindAsync(requestDto.SalonEventoID);

                if (salon == null || salon.EstaActivo != true)
                {
                    return Ok(new SalonDisponibleViewModel
                    {
                        SalonEventoID = requestDto.SalonEventoID,
                        NombreSalon = salon?.Nombre ?? "N/A",
                        Disponible = false,
                        Mensaje = "Salón no encontrado o no está activo."
                    });
                }

                // Verificar solapamientos de fecha y hora
                // Un salón está ocupado si: (InicioEventoExistente < FinSolicitado) Y (FinEventoExistente > InicioSolicitado) en la misma fecha.
                bool ocupado = await db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == requestDto.SalonEventoID &&
                                   re.FechaEvento == requestDto.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") && // Solo contra reservas activas
                                   (re.HoraInicio < requestDto.HoraFin && re.HoraFin > requestDto.HoraInicio)
                              );

                if (ocupado)
                {
                    return Ok(new SalonDisponibleViewModel { SalonEventoID = salon.SalonEventoID, NombreSalon = salon.Nombre, Disponible = false, Mensaje = $"Salón no disponible para la fecha y hora seleccionadas (ya reservado)." });
                }

                return Ok(new SalonDisponibleViewModel { SalonEventoID = salon.SalonEventoID, NombreSalon = salon.Nombre, Disponible = true, Mensaje = "Salón disponible." });
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al verificar disponibilidad del salón: {ex.ToString()}"));
            }
        }


        // POST: api/ReservasEvento
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ReservaEventoViewModel))]
        public async Task<IHttpActionResult> PostReservaEvento(ReservaEventoCreacionBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (model.HoraInicio >= model.HoraFin)
            {
                return BadRequest("La hora de fin del evento debe ser posterior a la hora de inicio.");
            }

            string empleadoAspNetUserId = User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return BadRequest("No se encontró el registro de empleado para el usuario autenticado.");
            }
            int empleadoResponsableIdInt = empleadoDb.empleado_id;

            // Re-verificar disponibilidad del salón
            bool salonOcupado = await db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == model.SalonEventoID &&
                                   re.FechaEvento == model.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") &&
                                   (re.HoraInicio < model.HoraFin && re.HoraFin > model.HoraInicio)
                              );
            if (salonOcupado)
            {
                return Content(HttpStatusCode.Conflict, new { Message = $"El salón ya no está disponible para la fecha y hora seleccionadas." });
            }


            var salon = await db.SalonEventoes.FindAsync(model.SalonEventoID);
            if (salon == null) return BadRequest("Salón no válido.");


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    ReservaEvento nuevaReservaEvento = new ReservaEvento
                    {
                        ClienteID = model.ClienteID,
                        SalonEventoID = model.SalonEventoID,
                        TipoEventoID = model.TipoEventoID,
                        NombreEvento = model.NombreEvento,
                        FechaSolicitud = DateTime.Now,
                        FechaEvento = model.FechaEvento.Date,
                        HoraInicio = model.HoraInicio,
                        HoraFin = model.HoraFin,
                        NumeroAsistentesEstimado = model.NumeroAsistentesEstimado,
                        EmpleadoResponsableID = empleadoResponsableIdInt,
                        Estado = "Solicitada", // O "Confirmada" directamente si el flujo lo permite
                        Notas = model.NotasGenerales,
                        MontoEstimadoSalon = salon.PrecioPorHora.HasValue ?
                                            Convert.ToDecimal((model.HoraFin - model.HoraInicio).TotalHours) * salon.PrecioPorHora.Value :
                                            (decimal?)null
                    };

                    decimal montoEstimadoServicios = 0;
                    db.ReservaEventoes.Add(nuevaReservaEvento);
                    await db.SaveChangesAsync(); // Guardar para obtener nuevaReservaEvento.ReservaEventoID

                    if (model.ServiciosAdicionales != null)
                    {
                        foreach (var servicioInput in model.ServiciosAdicionales)
                        {
                            var servicioCatalogo = await db.ServicioAdicionalEventoes.FindAsync(servicioInput.ServicioAdicionalID);
                            if (servicioCatalogo == null) throw new Exception($"Servicio adicional ID {servicioInput.ServicioAdicionalID} no encontrado.");

                            ReservaEvento_Servicio res_srv = new ReservaEvento_Servicio
                            {
                                ReservaEventoID = nuevaReservaEvento.ReservaEventoID,
                                ServicioAdicionalID = servicioInput.ServicioAdicionalID,
                                Cantidad = servicioInput.Cantidad,
                                PrecioCobradoPorUnidad = servicioInput.PrecioCobradoPorUnidad, // Usar el precio del input
                                Notas = servicioInput.Notas
                                // El subtotal es una columna calculada en la BD
                            };
                            db.ReservaEvento_Servicio.Add(res_srv);
                            montoEstimadoServicios += (servicioInput.PrecioCobradoPorUnidad * servicioInput.Cantidad);
                        }
                    }
                    nuevaReservaEvento.MontoEstimadoServicios = montoEstimadoServicios;
                    db.Entry(nuevaReservaEvento).State = EntityState.Modified; // Para guardar el monto de servicios

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    // Construir y devolver el ViewModel completo
                    var viewModel = await GetReservaEventoViewModelById(nuevaReservaEvento.ReservaEventoID);
                    return CreatedAtRoute("GetReservaEventoById", new { id = nuevaReservaEvento.ReservaEventoID }, viewModel);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(new Exception($"Error al crear reserva de evento: {ex.ToString()}"));
                }
            }
        }

        // GET: api/ReservasEvento
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservasEvento([FromUri] int? clienteId = null, [FromUri] int? salonId = null, [FromUri] DateTime? fecha = null)
        {
            try
            {
                var query = db.ReservaEventoes.AsQueryable();
                if (clienteId.HasValue) query = query.Where(re => re.ClienteID == clienteId.Value);
                if (salonId.HasValue) query = query.Where(re => re.SalonEventoID == salonId.Value);
                if (fecha.HasValue) query = query.Where(re => re.FechaEvento == fecha.Value.Date);

                var reservas = await query
                    .OrderByDescending(re => re.FechaEvento).ThenByDescending(re => re.HoraInicio)
                    .Select(re => new // Proyección simplificada para la lista
                    {
                        re.ReservaEventoID,
                        re.NombreEvento,
                        re.Cliente.nombre,
                        re.Cliente.apellido,
                        NombreSalon = re.SalonEvento.Nombre,
                        re.FechaEvento,
                        re.HoraInicio,
                        re.Estado
                    })
                    .ToListAsync();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reservas de evento: {ex.ToString()}"));
            }
        }

        // GET: api/ReservasEvento/5
        [HttpGet]
        [Route("{id:int}", Name = "GetReservaEventoById")]
        [ResponseType(typeof(ReservaEventoViewModel))]
        public async Task<IHttpActionResult> GetReservaEvento(int id)
        {
            var viewModel = await GetReservaEventoViewModelById(id);
            if (viewModel == null) return NotFound();
            return Ok(viewModel);
        }

        // PUT: api/ReservasEvento/5
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReservaEvento(int id, ReservaEventoUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var reservaEvento = await db.ReservaEventoes
                                        .Include(re => re.ReservaEvento_Servicio)
                                        .FirstOrDefaultAsync(re => re.ReservaEventoID == id);

            if (reservaEvento == null) return NotFound();

            if (reservaEvento.Estado == "Realizada" || reservaEvento.Estado == "Cancelada")
            {
                return BadRequest($"La reserva de evento no se puede modificar porque su estado es '{reservaEvento.Estado}'.");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    reservaEvento.NombreEvento = model.NombreEvento;
                    reservaEvento.NumeroAsistentesEstimado = model.NumeroAsistentesEstimado;
                    reservaEvento.NumeroAsistentesConfirmado = model.NumeroAsistentesConfirmado;
                    reservaEvento.Notas = model.NotasGenerales;
                    reservaEvento.Estado = model.Estado; // Validar que sea un estado permitido

                    // Actualizar servicios: eliminar los existentes y añadir los nuevos
                    db.ReservaEvento_Servicio.RemoveRange(reservaEvento.ReservaEvento_Servicio);
                    decimal nuevoMontoServicios = 0;
                    if (model.ServiciosAdicionales != null)
                    {
                        foreach (var servicioInput in model.ServiciosAdicionales)
                        {
                            var servicioCatalogo = await db.ServicioAdicionalEventoes.FindAsync(servicioInput.ServicioAdicionalID);
                            if (servicioCatalogo == null) throw new Exception($"Servicio adicional ID {servicioInput.ServicioAdicionalID} no encontrado.");

                            db.ReservaEvento_Servicio.Add(new ReservaEvento_Servicio
                            {
                                ReservaEventoID = id,
                                ServicioAdicionalID = servicioInput.ServicioAdicionalID,
                                Cantidad = servicioInput.Cantidad,
                                PrecioCobradoPorUnidad = servicioInput.PrecioCobradoPorUnidad,
                                Notas = servicioInput.Notas
                            });
                            nuevoMontoServicios += (servicioInput.PrecioCobradoPorUnidad * servicioInput.Cantidad);
                        }
                    }
                    reservaEvento.MontoEstimadoServicios = nuevoMontoServicios;
                    // Recalcular MontoEstimadoSalon si la duración o el salón cambiaran (no permitido en este DTO simple)

                    db.Entry(reservaEvento).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    transaction.Commit();
                    return StatusCode(HttpStatusCode.NoContent);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(new Exception($"Error al actualizar reserva de evento: {ex.ToString()}"));
                }
            }
        }


        // POST: api/ReservasEvento/{id}/Cancelar
        [HttpPost]
        [Route("{id:int}/Cancelar")]
        public async Task<IHttpActionResult> CancelarReservaEvento(int id)
        {
            var reservaEvento = await db.ReservaEventoes.FindAsync(id);
            if (reservaEvento == null) return NotFound();

            if (reservaEvento.Estado == "Cancelada" || reservaEvento.Estado == "Realizada")
            {
                return Ok(new { Message = $"La reserva de evento ya estaba {reservaEvento.Estado.ToLower()}." });
            }
            reservaEvento.Estado = "Cancelada";
            db.Entry(reservaEvento).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Ok(new { Message = "Reserva de evento cancelada exitosamente." });
        }


        private async Task<ReservaEventoViewModel> GetReservaEventoViewModelById(int id)
        {
            var re = await db.ReservaEventoes
                .Include(r => r.Cliente)
                .Include(r => r.SalonEvento.Hotel) // Para nombre del hotel
                .Include(r => r.TipoEvento)
                .Include(r => r.Empleado) // Empleado responsable
                .Include(r => r.ReservaEvento_Servicio.Select(rs => rs.ServicioAdicionalEvento))
                .FirstOrDefaultAsync(r => r.ReservaEventoID == id);

            if (re == null) return null;

            decimal montoTotalServicios = re.ReservaEvento_Servicio.Sum(rs => rs.Cantidad * rs.PrecioCobradoPorUnidad);
            decimal montoSalon = re.MontoEstimadoSalon ?? 0;
            // Si el precio del salón es por hora y se quiere calcular aquí:
            // if (re.SalonEvento.PrecioPorHora.HasValue)
            // {
            //     montoSalon = Convert.ToDecimal((re.HoraFin - re.HoraInicio).TotalHours) * re.SalonEvento.PrecioPorHora.Value;
            // }


            return new ReservaEventoViewModel
            {
                ReservaEventoID = re.ReservaEventoID,
                ClienteID = re.ClienteID,
                NombreCliente = $"{re.Cliente.nombre} {re.Cliente.apellido}",
                SalonEventoID = re.SalonEventoID,
                NombreSalon = re.SalonEvento.Nombre,
                TipoEventoID = re.TipoEventoID,
                NombreTipoEvento = re.TipoEvento?.NombreTipo,
                NombreEvento = re.NombreEvento,
                FechaSolicitud = (DateTime)re.FechaSolicitud,
                FechaEvento = re.FechaEvento,
                HoraInicio = re.HoraInicio,
                HoraFin = re.HoraFin,
                NumeroAsistentesEstimado = re.NumeroAsistentesEstimado,
                NumeroAsistentesConfirmado = re.NumeroAsistentesConfirmado,
                NombreEmpleadoResponsable = re.Empleado != null ? $"{re.Empleado.nombre} {re.Empleado.apellido}" : "N/A",
                Estado = re.Estado,
                NotasGenerales = re.Notas,
                MontoEstimadoSalon = montoSalon,
                MontoEstimadoServicios = montoTotalServicios,
                MontoTotalEvento = montoSalon + montoTotalServicios, // Simplificado, añadir impuestos si aplica
                ServiciosAdicionales = re.ReservaEvento_Servicio.Select(rs => new ReservaEventoServicioViewModel
                {
                    ServicioAdicionalID = rs.ServicioAdicionalID,
                    NombreServicio = rs.ServicioAdicionalEvento.NombreServicio,
                    Cantidad = rs.Cantidad,
                    PrecioCobradoPorUnidad = rs.PrecioCobradoPorUnidad,
                    Subtotal = (decimal)rs.Subtotal, // Ya es calculado
                    Notas = rs.Notas
                }).ToList()
            };
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