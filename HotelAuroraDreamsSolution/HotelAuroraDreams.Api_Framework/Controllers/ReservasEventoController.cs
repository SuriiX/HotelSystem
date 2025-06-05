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
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ReservasEventoController() { }

        public ReservasEventoController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

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
                // Asume DbSet: db.SalonEventoes
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

                bool ocupado = await db.ReservaEventoes // Asume DbSet: db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == requestDto.SalonEventoID &&
                                   re.FechaEvento == requestDto.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") &&
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
            // Asume DbSet: db.Empleadoes
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return BadRequest("No se encontró el registro de empleado para el usuario autenticado.");
            }
            int empleadoResponsableIdInt = empleadoDb.empleado_id;

            bool salonOcupado = await db.ReservaEventoes // Asume DbSet: db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == model.SalonEventoID &&
                                   re.FechaEvento == model.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") &&
                                   (re.HoraInicio < model.HoraFin && re.HoraFin > model.HoraInicio)
                              );
            if (salonOcupado)
            {
                return Content(HttpStatusCode.Conflict, new { Message = $"El salón ya no está disponible para la fecha y hora seleccionadas." });
            }

            // Asume DbSet: db.SalonEventoes
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
                        Estado = "Solicitada",
                        Notas = model.NotasGenerales,
                        MontoEstimadoSalon = salon.PrecioPorHora.HasValue ?
                                            Convert.ToDecimal((model.HoraFin - model.HoraInicio).TotalHours) * salon.PrecioPorHora.Value :
                                            (decimal?)null
                    };

                    decimal montoEstimadoServicios = 0;
                    db.ReservaEventoes.Add(nuevaReservaEvento); // Asume DbSet: db.ReservaEventoes
                    await db.SaveChangesAsync();

                    if (model.ServiciosAdicionales != null)
                    {
                        foreach (var servicioInput in model.ServiciosAdicionales)
                        {
                            // Asume DbSet: db.ServicioAdicionalEventoes
                            var servicioCatalogo = await db.ServicioAdicionalEventoes.FindAsync(servicioInput.ServicioAdicionalID);
                            if (servicioCatalogo == null) throw new Exception($"Servicio adicional ID {servicioInput.ServicioAdicionalID} no encontrado.");

                            ReservaEvento_Servicio res_srv = new ReservaEvento_Servicio
                            {
                                ReservaEventoID = nuevaReservaEvento.ReservaEventoID,
                                ServicioAdicionalID = servicioInput.ServicioAdicionalID,
                                Cantidad = servicioInput.Cantidad,
                                PrecioCobradoPorUnidad = servicioInput.PrecioCobradoPorUnidad,
                                Notas = servicioInput.Notas
                            };
                            db.ReservaEvento_Servicio.Add(res_srv);
                            montoEstimadoServicios += (servicioInput.PrecioCobradoPorUnidad * servicioInput.Cantidad);
                        }
                    }
                    nuevaReservaEvento.MontoEstimadoServicios = montoEstimadoServicios;
                    db.Entry(nuevaReservaEvento).State = EntityState.Modified;

                    await db.SaveChangesAsync();
                    transaction.Commit();

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

        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<ReservaEventoListItemDto>))]
        public async Task<IHttpActionResult> GetReservasEvento(
            [FromUri] int? clienteId = null,
            [FromUri] int? salonId = null,
            [FromUri] DateTime? fecha = null,
            [FromUri] string estado = null)
        {
            try
            {
                var query = db.ReservaEventoes // Asume DbSet: db.ReservaEventoes
                              .Include(re => re.Cliente)
                              .Include(re => re.SalonEvento)
                              .AsQueryable();

                if (clienteId.HasValue) query = query.Where(re => re.ClienteID == clienteId.Value);
                if (salonId.HasValue) query = query.Where(re => re.SalonEventoID == salonId.Value);
                if (fecha.HasValue) query = query.Where(re => DbFunctions.TruncateTime(re.FechaEvento) == DbFunctions.TruncateTime(fecha.Value));
                if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(re => re.Estado.ToLower() == estado.ToLower());

                var reservasViewModel = await query
                    .OrderByDescending(re => re.FechaEvento)
                    .ThenByDescending(re => re.HoraInicio)
                    .Select(re => new ReservaEventoListItemDto
                    {
                        ReservaEventoID = re.ReservaEventoID,
                        NombreEvento = re.NombreEvento,
                        NombreCliente = (re.Cliente != null) ? (re.Cliente.nombre + " " + re.Cliente.apellido) : "N/A",
                        NombreSalon = (re.SalonEvento != null) ? re.SalonEvento.Nombre : "N/A",
                        FechaEvento = re.FechaEvento,
                        HoraInicio = re.HoraInicio, // HoraInicio es TimeSpan
                        Estado = re.Estado
                    })
                    .ToListAsync();

                return Ok(reservasViewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reservas de evento: {ex.ToString()}"));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetReservaEventoById")]
        [ResponseType(typeof(ReservaEventoViewModel))]
        public async Task<IHttpActionResult> GetReservaEvento(int id)
        {
            var viewModel = await GetReservaEventoViewModelById(id);
            if (viewModel == null) return NotFound();
            return Ok(viewModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReservaEvento(int id, ReservaEventoUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var reservaEvento = await db.ReservaEventoes // Asume DbSet
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
                    reservaEvento.Estado = model.Estado;

                    db.ReservaEvento_Servicio.RemoveRange(reservaEvento.ReservaEvento_Servicio);
                    decimal nuevoMontoServicios = 0;
                    if (model.ServiciosAdicionales != null)
                    {
                        foreach (var servicioInput in model.ServiciosAdicionales)
                        {
                            // Asume DbSet: db.ServicioAdicionalEventoes
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

        [HttpPost]
        [Route("{id:int}/Cancelar")]
        public async Task<IHttpActionResult> CancelarReservaEvento(int id)
        {
            // Asume DbSet: db.ReservaEventoes
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
            // Asume DbSet: db.ReservaEventoes
            var re = await db.ReservaEventoes
                .Include(r => r.Cliente)
                .Include(r => r.SalonEvento.Hotel)
                .Include(r => r.TipoEvento)
                .Include(r => r.Empleado)
                .Include(r => r.ReservaEvento_Servicio.Select(rs => rs.ServicioAdicionalEvento))
                .FirstOrDefaultAsync(r => r.ReservaEventoID == id);

            if (re == null) return null;

            decimal montoTotalServicios = re.ReservaEvento_Servicio.Sum(rs => rs.Cantidad * rs.PrecioCobradoPorUnidad);
            decimal montoSalon = re.MontoEstimadoSalon ?? 0;

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
                FechaSolicitud = (DateTime)re.FechaSolicitud, // Quitado el casting (DateTime)
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
                MontoTotalEvento = montoSalon + montoTotalServicios,
                ServiciosAdicionales = re.ReservaEvento_Servicio.Select(rs => new ReservaEventoServicioViewModel
                {
                    ServicioAdicionalID = rs.ServicioAdicionalID,
                    NombreServicio = rs.ServicioAdicionalEvento.NombreServicio,
                    Cantidad = rs.Cantidad,
                    PrecioCobradoPorUnidad = rs.PrecioCobradoPorUnidad,
                    Subtotal = (decimal)rs.Subtotal, // Quitado el casting (decimal)
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