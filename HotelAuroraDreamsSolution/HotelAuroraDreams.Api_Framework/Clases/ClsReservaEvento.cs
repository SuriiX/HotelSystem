// File: ~/Clases/ClsReservaEvento.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsReservaEvento : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ClsReservaEvento() { }

        public ClsReservaEvento(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public async Task<IHttpActionResult> VerificarDisponibilidadSalon(SalonDisponibilidadRequestDto requestDto)
        {
            if (requestDto == null || !ModelState.IsValid)
            {
                return new BadRequestResult(ModelState, Request);
            }
            if (requestDto.HoraInicio >= requestDto.HoraFin)
            {
                return new BadRequestResult("La hora de fin debe ser posterior a la hora de inicio.", Request);
            }

            try
            {
                var salon = await db.SalonEventoes.FindAsync(requestDto.SalonEventoID);

                if (salon == null || salon.EstaActivo != true)
                {
                    return new OkResult(new SalonDisponibleViewModel
                    {
                        SalonEventoID = requestDto.SalonEventoID,
                        NombreSalon = salon?.Nombre ?? "N/A",
                        Disponible = false,
                        Mensaje = "Salón no encontrado o no está activo."
                    }, Request);
                }

                bool ocupado = await db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == requestDto.SalonEventoID &&
                                   re.FechaEvento == requestDto.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") &&
                                   (re.HoraInicio < requestDto.HoraFin && re.HoraFin > requestDto.HoraInicio));

                if (ocupado)
                {
                    return new OkResult(new SalonDisponibleViewModel
                    {
                        SalonEventoID = salon.SalonEventoID,
                        NombreSalon = salon.Nombre,
                        Disponible = false,
                        Mensaje = "Salón no disponible para la fecha y hora seleccionadas (ya reservado)."
                    }, Request);
                }

                return new OkResult(new SalonDisponibleViewModel
                {
                    SalonEventoID = salon.SalonEventoID,
                    NombreSalon = salon.Nombre,
                    Disponible = true,
                    Mensaje = "Salón disponible."
                }, Request);
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult(new Exception($"Error al verificar disponibilidad del salón: {ex.Message}"));
            }
        }

        public async Task<IHttpActionResult> CrearReservaEvento(ReservaEventoCreacionBindingModel model, string userId)
        {
            if (model == null || !ModelState.IsValid)
            {
                return new BadRequestResult(ModelState, Request);
            }
            if (model.HoraInicio >= model.HoraFin)
            {
                return new BadRequestResult("La hora de fin del evento debe ser posterior a la hora de inicio.", Request);
            }

            var empleadoAppUser = await UserManager.FindByIdAsync(userId);
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return new BadRequestResult("No se encontró el registro de empleado para el usuario autenticado.", Request);
            }
            int empleadoResponsableIdInt = empleadoDb.empleado_id;

            bool salonOcupado = await db.ReservaEventoes
                    .AnyAsync(re => re.SalonEventoID == model.SalonEventoID &&
                                   re.FechaEvento == model.FechaEvento.Date &&
                                   (re.Estado == "Confirmada" || re.Estado == "En Curso") &&
                                   (re.HoraInicio < model.HoraFin && re.HoraFin > model.HoraInicio));
            if (salonOcupado)
            {
                return new ConflictResult(new { Message = "El salón ya no está disponible para la fecha y hora seleccionadas." }, Request);
            }

            var salon = await db.SalonEventoes.FindAsync(model.SalonEventoID);
            if (salon == null) return new BadRequestResult("Salón no válido.", Request);

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
                    db.ReservaEventoes.Add(nuevaReservaEvento);
                    await db.SaveChangesAsync();

                    if (model.ServiciosAdicionales != null)
                    {
                        foreach (var servicioInput in model.ServiciosAdicionales)
                        {
                            var servicioCatalogo = await db.ServicioAdicionalEventoes.FindAsync(servicioInput.ServicioAdicionalID);
                            if (servicioCatalogo == null)
                                throw new Exception($"Servicio adicional ID {servicioInput.ServicioAdicionalID} no encontrado.");

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
                    return new CreatedAtRouteResult("GetReservaEventoById", new { id = nuevaReservaEvento.ReservaEventoID }, viewModel);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new InternalServerErrorResult(new Exception($"Error al crear reserva de evento: {ex.Message}"));
                }
            }
        }

        public async Task<IHttpActionResult> ObtenerReservasEvento(int? clienteId = null, int? salonId = null, DateTime? fecha = null)
        {
            try
            {
                var query = db.ReservaEventoes.AsQueryable();
                if (clienteId.HasValue) query = query.Where(re => re.ClienteID == clienteId.Value);
                if (salonId.HasValue) query = query.Where(re => re.SalonEventoID == salonId.Value);
                if (fecha.HasValue) query = query.Where(re => re.FechaEvento == fecha.Value.Date);

                var reservas = await query
                    .OrderByDescending(re => re.FechaEvento).ThenByDescending(re => re.HoraInicio)
                    .Select(re => new
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
                return new OkResult(reservas, Request);
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult(new Exception($"Error al obtener reservas de evento: {ex.Message}"));
            }
        }

        public async Task<IHttpActionResult> ObtenerReservaEventoPorId(int id)
        {
            var viewModel = await GetReservaEventoViewModelById(id);
            if (viewModel == null) return new NotFoundResult();
            return new OkResult(viewModel, Request);
        }

        public async Task<IHttpActionResult> ActualizarReservaEvento(int id, ReservaEventoUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
                return new BadRequestResult(ModelState, Request);

            var reservaEvento = await db.ReservaEventoes
                                    .Include(re => re.ReservaEvento_Servicio)
                                    .FirstOrDefaultAsync(re => re.ReservaEventoID == id);

            if (reservaEvento == null)
                return new NotFoundResult();

            if (reservaEvento.Estado == "Realizada" || reservaEvento.Estado == "Cancelada")
            {
                return new BadRequestResult($"La reserva de evento no se puede modificar porque su estado es '{reservaEvento.Estado}'.", Request);
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
                            var servicioCatalogo = await db.ServicioAdicionalEventoes.FindAsync(servicioInput.ServicioAdicionalID);
                            if (servicioCatalogo == null)
                                throw new Exception($"Servicio adicional ID {servicioInput.ServicioAdicionalID} no encontrado.");

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
                    return new StatusCodeResult(HttpStatusCode.NoContent, Request);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new InternalServerErrorResult(new Exception($"Error al actualizar reserva de evento: {ex.Message}"));
                }
            }
        }

        public async Task<IHttpActionResult> CancelarReservaEvento(int id)
        {
            var reservaEvento = await db.ReservaEventoes.FindAsync(id);
            if (reservaEvento == null)
                return new NotFoundResult();

            if (reservaEvento.Estado == "Cancelada" || reservaEvento.Estado == "Realizada")
            {
                return new OkResult(new { Message = $"La reserva de evento ya estaba {reservaEvento.Estado.ToLower()}." }, Request);
            }

            reservaEvento.Estado = "Cancelada";
            db.Entry(reservaEvento).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return new OkResult(new { Message = "Reserva de evento cancelada exitosamente." }, Request);
        }

        private async Task<ReservaEventoViewModel> GetReservaEventoViewModelById(int id)
        {
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
                MontoTotalEvento = montoSalon + montoTotalServicios,
                ServiciosAdicionales = re.ReservaEvento_Servicio.Select(rs => new ReservaEventoServicioViewModel
                {
                    ServicioAdicionalID = rs.ServicioAdicionalID,
                    NombreServicio = rs.ServicioAdicionalEvento.NombreServicio,
                    Cantidad = rs.Cantidad,
                    PrecioCobradoPorUnidad = rs.PrecioCobradoPorUnidad,
                    Subtotal = (decimal)rs.Subtotal,
                    Notas = rs.Notas
                }).ToList()
            };
        }

        public void Dispose()
        {
            db.Dispose();
            _userManager?.Dispose();
        }
    }
}