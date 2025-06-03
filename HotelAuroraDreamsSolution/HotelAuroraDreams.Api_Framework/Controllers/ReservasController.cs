using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using HotelAuroraDreams.Api_Framework.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Web.Http;
using System;
using System.Linq;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Reservas")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class ReservasController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ReservasController() { }

        public ReservasController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        [Route("Disponibilidad")]
        [ResponseType(typeof(List<HabitacionDisponibleDto>))]
        public async Task<IHttpActionResult> VerificarDisponibilidad([FromBody] DisponibilidadRequestDto requestDto)
        {
            if (requestDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (requestDto.FechaEntrada >= requestDto.FechaSalida)
            {
                return BadRequest("La fecha de salida debe ser posterior a la fecha de entrada.");
            }
            if (requestDto.FechaEntrada < DateTime.Today)
            {
                return BadRequest("La fecha de entrada no puede ser una fecha pasada.");
            }

            try
            {
                var queryHabitacionesCandidatas = db.Habitacions
                    .Where(h => h.hotel_id == requestDto.HotelID &&
                                h.Tipo_Habitacion.capacidad >= requestDto.NumeroHuespedes &&
                                h.estado != "Mantenimiento" && h.estado != "Inactiva");

                if (requestDto.TipoHabitacionID.HasValue && requestDto.TipoHabitacionID.Value > 0)
                {
                    queryHabitacionesCandidatas = queryHabitacionesCandidatas
                        .Where(h => h.tipo_habitacion_id == requestDto.TipoHabitacionID.Value);
                }

                var habitacionesCandidatasList = await queryHabitacionesCandidatas
                    .Select(h => new
                    {
                        h.habitacion_id,
                        h.numero,
                        h.Tipo_Habitacion.tipo_habitacion_id,
                        NombreTipoHabitacion = h.Tipo_Habitacion.nombre,
                        DescripcionTipoHabitacion = h.Tipo_Habitacion.descripcion,
                        PrecioNoche = h.Tipo_Habitacion.precio_base,
                        h.Tipo_Habitacion.capacidad,
                        h.Tipo_Habitacion.comodidades,
                        h.vista,
                        h.piso
                    })
                    .ToListAsync();

                var idsHabitacionesOcupadas = await db.Reserva_Habitacion
                    .Where(rh =>
                        (rh.Reserva.estado == "Confirmada" || rh.Reserva.estado == "Completada") &&
                        rh.Reserva.hotel_id == requestDto.HotelID &&
                        (rh.Reserva.fecha_entrada < requestDto.FechaSalida && rh.Reserva.fecha_salida > requestDto.FechaEntrada)
                    )
                    .Select(rh => rh.habitacion_id)
                    .Distinct()
                    .ToListAsync();

                var habitacionesDisponibles = habitacionesCandidatasList
                    .Where(h_cand => !idsHabitacionesOcupadas.Contains(h_cand.habitacion_id))
                    .Select(h_disp => new HabitacionDisponibleDto
                    {
                        HabitacionID = h_disp.habitacion_id,
                        NumeroHabitacion = h_disp.numero,
                        TipoHabitacionID = h_disp.tipo_habitacion_id,
                        NombreTipoHabitacion = h_disp.NombreTipoHabitacion,
                        DescripcionTipoHabitacion = h_disp.DescripcionTipoHabitacion,
                        PrecioNoche = h_disp.PrecioNoche,
                        Capacidad = h_disp.capacidad,
                        Comodidades = h_disp.comodidades,
                        Vista = h_disp.vista,
                        Piso = h_disp.piso
                    })
                    .OrderBy(h => h.NombreTipoHabitacion).ThenBy(h => h.NumeroHabitacion)
                    .ToList();

                return Ok(habitacionesDisponibles);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al verificar disponibilidad: {ex.ToString()}"));
            }
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ReservaViewModel))]
        public async Task<IHttpActionResult> PostReserva(ReservaCreacionBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (model.FechaEntrada >= model.FechaSalida)
            {
                return BadRequest("La fecha de salida debe ser posterior a la fecha de entrada.");
            }
            if (model.FechaEntrada < DateTime.Today)
            {
                return BadRequest("La fecha de entrada no puede ser pasada.");
            }
            if (model.HabitacionIDsSeleccionadas == null || !model.HabitacionIDsSeleccionadas.Any())
            {
                return BadRequest("Debe seleccionar al menos una habitación.");
            }

            string empleadoAspNetUserId = User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);
            if (empleadoAppUser == null)
            {
                return BadRequest("No se pudo identificar al empleado que registra (Usuario de Identity no encontrado).");
            }

            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return BadRequest("No se encontró el registro en la tabla Empleado para el usuario autenticado. Asegúrese de que el empleado exista en ambas tablas (Identity y Empleado) con el mismo email.");
            }
            int empleadoRegistroIdInt = empleadoDb.empleado_id;

            Reserva nuevaReserva = new Reserva
            {
                ClienteID = model.ClienteID,
                HotelID = model.HotelID,
                FechaReserva = DateTime.Now,
                FechaEntrada = model.FechaEntrada,
                fecha_salida = model.FechaSalida,
                Estado = "Confirmada",
                numero_huespedes = model.NumeroHuespedes,
                notas = model.Notas,
                empleado_registro_id = empleadoRegistroIdInt
            };

            try
            {
                var idsHabitacionesOcupadas = await db.Reserva_Habitacion
                    .Where(rh => model.HabitacionIDsSeleccionadas.Contains(rh.habitacion_id) &&
                                 (rh.Reserva.estado == "Confirmada") &&
                                 (rh.Reserva.fecha_entrada < model.FechaSalida && rh.Reserva.fecha_salida > model.FechaEntrada))
                    .Select(rh => rh.habitacion_id)
                    .Distinct()
                    .ToListAsync();

                if (idsHabitacionesOcupadas.Any())
                {
                    var habitacionesNoDisponiblesNros = await db.Habitacions
                        .Where(h => idsHabitacionesOcupadas.Contains(h.habitacion_id))
                        .Select(h => h.numero).ToListAsync();
                    return Content(HttpStatusCode.Conflict, new { Message = $"Una o más habitaciones seleccionadas ({string.Join(", ", habitacionesNoDisponiblesNros)}) ya no están disponibles para estas fechas." });
                }

                db.Reservas.Add(nuevaReserva);
                await db.SaveChangesAsync();

                decimal montoTotalReserva = 0;
                int noches = Math.Max(1, (model.FechaSalida.Date - model.FechaEntrada.Date).Days);

                foreach (var habitacionId in model.HabitacionIDsSeleccionadas)
                {
                    var habitacion = await db.Habitacions.Include(h => h.Tipo_Habitacion)
                                         .FirstOrDefaultAsync(h => h.habitacion_id == habitacionId);
                    if (habitacion == null)
                    {
                        throw new Exception($"Habitación ID {habitacionId} no encontrada durante la asignación a la reserva.");
                    }
                    decimal precioNoche = habitacion.Tipo_Habitacion.precio_base;
                    montoTotalReserva += precioNoche * noches;

                    Reserva_Habitacion rh = new Reserva_Habitacion
                    {
                        reserva_id = nuevaReserva.reserva_id,
                        habitacion_id = habitacionId,
                        precio_noche_cobrado = precioNoche,
                        estado_asignacion = "asignada"
                    };
                    db.Reserva_Habitacion.Add(rh);
                }
                await db.SaveChangesAsync();

                var cliente = await db.Clientes.FindAsync(nuevaReserva.ClienteID);
                var hotel = await db.Hotels.FindAsync(nuevaReserva.HotelID);
                var habitacionesReservadasVM = await db.Reserva_Habitacion
                    .Where(r => r.reserva_id == nuevaReserva.reserva_id)
                    .Select(rh => new ReservaHabitacionViewModel
                    {
                        ReservaHabitacionID = rh.reserva_habitacion_id,
                        HabitacionID = rh.habitacion_id,
                        NumeroHabitacion = rh.Habitacion.numero,
                        NombreTipoHabitacion = rh.Habitacion.Tipo_Habitacion.nombre,
                        PrecioNocheCobrado = rh.precio_noche_cobrado,
                        EstadoAsignacion = rh.estado_asignacion
                    }).ToListAsync();

                var viewModel = new ReservaViewModel
                {
                    ReservaID = nuevaReserva.reserva_id,
                    ClienteID = nuevaReserva.ClienteID,
                    NombreCliente = cliente != null ? $"{cliente.nombre} {cliente.apellido}" : "N/A",
                    EmailCliente = cliente?.email,
                    TelefonoCliente = cliente?.telefono,
                    HotelID = nuevaReserva.HotelID,
                    NombreHotel = hotel?.nombre,
                    FechaReserva = nuevaReserva.FechaReserva,
                    FechaEntrada = nuevaReserva.FechaEntrada,
                    FechaSalida = nuevaReserva.fecha_salida,
                    Estado = nuevaReserva.Estado,
                    NumeroHuespedes = nuevaReserva.numero_huespedes,
                    Notas = nuevaReserva.notas,
                    NombreEmpleadoRegistro = empleadoDb != null ? $"{empleadoDb.nombre} {empleadoDb.apellido}" : "N/A",
                    HabitacionesReservadas = habitacionesReservadasVM,
                    MontoTotalReserva = montoTotalReserva
                };
                return CreatedAtRoute("GetReservaById", new { id = nuevaReserva.reserva_id }, viewModel);
            }
            catch (DbUpdateException dbEx)
            {
                return InternalServerError(new Exception($"DbUpdateException: {dbEx.ToString()}"));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error general al crear reserva: {ex.ToString()}"));
            }
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetReservas([FromUri] int? clienteId = null, [FromUri] int? hotelId = null, [FromUri] DateTime? fechaDesde = null, [FromUri] DateTime? fechaHasta = null, [FromUri] string estado = null)
        {
            try
            {
                var query = db.Reservas.AsQueryable();
                if (clienteId.HasValue) query = query.Where(r => r.ClienteID == clienteId.Value);
                if (hotelId.HasValue) query = query.Where(r => r.HotelID == hotelId.Value);
                if (fechaDesde.HasValue) query = query.Where(r => r.FechaEntrada >= fechaDesde.Value);
                if (fechaHasta.HasValue)
                {
                    DateTime fechaHastaFinDia = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.FechaEntrada <= fechaHastaFinDia);
                }
                if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(r => r.Estado.ToLower() == estado.ToLower());

                var reservas = await query
                    .OrderByDescending(r => r.FechaReserva)
                    .Select(r => new ReservaViewModel
                    {
                        ReservaID = r.reserva_id,
                        ClienteID = r.ClienteID,
                        NombreCliente = r.Cliente.nombre + " " + r.Cliente.apellido,
                        HotelID = r.HotelID,
                        NombreHotel = r.Hotel.nombre,
                        FechaReserva = r.FechaReserva,
                        FechaEntrada = r.FechaEntrada,
                        FechaSalida = r.fecha_salida,
                        Estado = r.Estado,
                        NumeroHuespedes = r.numero_huespedes,
                        Notas = r.notas,
                        NombreEmpleadoRegistro = r.Empleado != null ? r.Empleado.nombre + " " + r.Empleado.apellido : null
                    })
                    .ToListAsync();
                return Ok(reservas);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reservas: {ex.ToString()}"));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetReservaById")]
        [ResponseType(typeof(ReservaViewModel))]
        public async Task<IHttpActionResult> GetReserva(int id)
        {
            try
            {
                var reservaEntity = await db.Reservas
                    .Include(r => r.Cliente)
                    .Include(r => r.Hotel)
                    .Include(r => r.Empleado)
                    .Include("Reserva_Habitacion.Habitacion.Tipo_Habitacion")
                    .FirstOrDefaultAsync(r => r.reserva_id == id);

                if (reservaEntity == null)
                {
                    return NotFound();
                }

                decimal montoTotal = 0;
                int noches = Math.Max(1, (reservaEntity.fecha_salida.Date - reservaEntity.FechaEntrada.Date).Days);

                var habitacionesReservadasVM = reservaEntity.Reserva_Habitacion.Select(rh =>
                {
                    montoTotal += rh.precio_noche_cobrado * noches;
                    return new ReservaHabitacionViewModel
                    {
                        ReservaHabitacionID = rh.reserva_habitacion_id,
                        HabitacionID = rh.habitacion_id,
                        NumeroHabitacion = rh.Habitacion.numero,
                        NombreTipoHabitacion = rh.Habitacion.Tipo_Habitacion.nombre,
                        PrecioNocheCobrado = rh.precio_noche_cobrado,
                        EstadoAsignacion = rh.estado_asignacion
                    };
                }).ToList();

                var viewModel = new ReservaViewModel
                {
                    ReservaID = reservaEntity.reserva_id,
                    ClienteID = reservaEntity.ClienteID,
                    NombreCliente = $"{reservaEntity.Cliente.nombre} {reservaEntity.Cliente.apellido}",
                    EmailCliente = reservaEntity.Cliente.email,
                    TelefonoCliente = reservaEntity.Cliente.telefono,
                    HotelID = reservaEntity.HotelID,
                    NombreHotel = reservaEntity.Hotel.nombre,
                    FechaReserva = reservaEntity.FechaReserva,
                    FechaEntrada = reservaEntity.FechaEntrada,
                    FechaSalida = reservaEntity.fecha_salida,
                    Estado = reservaEntity.Estado,
                    NumeroHuespedes = reservaEntity.numero_huespedes,
                    Notas = reservaEntity.notas,
                    NombreEmpleadoRegistro = reservaEntity.Empleado != null ? $"{reservaEntity.Empleado.nombre} {reservaEntity.Empleado.apellido}" : "N/A",
                    HabitacionesReservadas = habitacionesReservadasVM,
                    MontoTotalReserva = montoTotal
                };
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener reserva ID {id}: {ex.ToString()}"));
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutReserva(int id, ReservaUpdateBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await db.Reservas.Include(r => r.Reserva_Habitacion.Select(rh => rh.Habitacion.Tipo_Habitacion))
                                  .FirstOrDefaultAsync(r => r.reserva_id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.Estado == "Cancelada" || reserva.Estado == "Completada")
            {
                return BadRequest($"La reserva no se puede modificar porque su estado es '{reserva.Estado}'.");
            }

            if (model.NumeroHuespedes != reserva.numero_huespedes)
            {
                int capacidadTotalHabitacionesAsignadas = reserva.Reserva_Habitacion.Sum(rh => rh.Habitacion.Tipo_Habitacion.capacidad);
                if (model.NumeroHuespedes > capacidadTotalHabitacionesAsignadas)
                {
                    return BadRequest($"El nuevo número de huéspedes ({model.NumeroHuespedes}) excede la capacidad de las habitaciones asignadas ({capacidadTotalHabitacionesAsignadas}).");
                }
                reserva.numero_huespedes = model.NumeroHuespedes;
            }
            reserva.notas = model.Notas;
            reserva.Estado = model.Estado;

            db.Entry(reserva).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(new Exception($"Error de concurrencia al actualizar reserva: {ex.ToString()}", ex));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar reserva: {ex.ToString()}", ex));
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("{id:int}/Cancelar")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> CancelarReserva(int id)
        {
            var reserva = await db.Reservas.Include(r => r.Reserva_Habitacion).FirstOrDefaultAsync(r => r.reserva_id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.Estado == "Cancelada" || reserva.Estado == "Completada")
            {
                return Ok(new { Message = $"La reserva ya estaba {reserva.Estado.ToLower()}." });
            }

            reserva.Estado = "Cancelada";
            foreach (var rh in reserva.Reserva_Habitacion)
            {
                rh.estado_asignacion = "cancelada";
                db.Entry(rh).State = EntityState.Modified;
            }
            db.Entry(reserva).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al cancelar la reserva: {ex.ToString()}", ex));
            }
            return Ok(new { Message = "Reserva cancelada exitosamente." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
