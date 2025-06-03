using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsCheckIn : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ClsCheckIn() { }

        public ClsCheckIn(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public async Task<IHttpActionResult> ProcesarCheckIn(CheckInBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return new BadRequestResult(ModelState, Request);
            }

            // 1. Validar Reserva
            var reserva = await db.Reservas
                                .Include(r => r.Reserva_Habitacion.Select(rh => rh.Habitacion))
                                .FirstOrDefaultAsync(r => r.reserva_id == model.ReservaID);

            if (reserva == null)
            {
                return new NotFoundResultWithMessage($"Reserva con ID {model.ReservaID} no encontrada.");
            }

            if (reserva.Estado != "Confirmada")
            {
                return new BadRequestResult($"La reserva ID {model.ReservaID} no está en estado 'Confirmada'. Estado actual: {reserva.Estado}.", Request);
            }

            if (reserva.FechaEntrada.Date > DateTime.Today)
            {
                return new BadRequestResult("No se puede hacer check-in antes de la fecha de entrada de la reserva.", Request);
            }

            if (await db.CheckIns.AnyAsync(ci => ci.reserva_id == model.ReservaID))
            {
                return new BadRequestResult($"Ya se ha realizado un check-in para la reserva ID {model.ReservaID}.", Request);
            }

            // 2. Obtener Empleado
            var (empleadoDb, errorEmpleado) = await ObtenerEmpleadoActual();
            if (errorEmpleado != null)
            {
                return new BadRequestResult(errorEmpleado, Request);
            }
            int empleadoIdInt = empleadoDb.empleado_id;

            // 3. Crear CheckIn
            var (checkIn, errorCheckIn) = await CrearCheckIn(model, reserva, empleadoIdInt);
            if (errorCheckIn != null)
            {
                return new InternalServerErrorResult(new Exception(errorCheckIn));
            }

            // 4. Obtener cliente
            var cliente = await db.Clientes.FindAsync(reserva.ClienteID);

            // 5. Crear ViewModel
            var viewModel = new CheckInViewModel
            {
                CheckInID = checkIn.checkin_id,
                ReservaID = checkIn.reserva_id,
                NombreCliente = cliente != null ? $"{cliente.nombre} {cliente.apellido}" : "N/A",
                FechaHora = (DateTime)checkIn.fecha_hora,
                MetodoPagoAdelanto = checkIn.metodo_pago_adelanto,
                DocumentosVerificados = (bool)checkIn.documentos_verificados,
                Observaciones = checkIn.observaciones,
                NombreEmpleado = $"{empleadoDb.nombre} {empleadoDb.apellido}"
            };

            return new CreatedAtRouteResult("GetCheckInById", new { id = checkIn.checkin_id }, viewModel);
        }

        private async Task<(Empleado empleado, string error)> ObtenerEmpleadoActual()
        {
            string empleadoAspNetUserId = HttpContext.Current.User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);

            if (empleadoAppUser == null)
                return (null, "No se pudo identificar al empleado (Identity User not found).");

            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);

            if (empleadoDb == null)
                return (null, "No se encontró el registro en la tabla Empleado para el usuario autenticado.");

            return (empleadoDb, null);
        }

        private async Task<(CheckIn checkIn, string error)> CrearCheckIn(CheckInBindingModel model, Reserva reserva, int empleadoId)
        {
            try
            {
                CheckIn nuevoCheckIn = new CheckIn
                {
                    reserva_id = model.ReservaID,
                    empleado_id = empleadoId,
                    fecha_hora = DateTime.Now,
                    metodo_pago_adelanto = model.MetodoPagoAdelanto,
                    documentos_verificados = model.DocumentosVerificados,
                    observaciones = model.Observaciones
                };
                db.CheckIns.Add(nuevoCheckIn);

                // Actualizar estados
                reserva.Estado = "Hospedado";
                db.Entry(reserva).State = EntityState.Modified;

                foreach (var reservaHabitacion in reserva.Reserva_Habitacion)
                {
                    var habitacion = reservaHabitacion.Habitacion;
                    if (habitacion != null)
                    {
                        habitacion.estado = "ocupada";
                        db.Entry(habitacion).State = EntityState.Modified;
                    }
                }

                await db.SaveChangesAsync();
                return (nuevoCheckIn, null);
            }
            catch (DbUpdateException dbEx)
            {
                return (null, $"DbUpdateException al procesar check-in: {dbEx.ToString()}");
            }
            catch (Exception ex)
            {
                return (null, $"Error general al procesar check-in: {ex.ToString()}");
            }
        }

        public async Task<IHttpActionResult> ObtenerCheckInPorId(int id)
        {
            var checkIn = await db.CheckIns
                            .Include(ci => ci.Reserva.Cliente)
                            .Include(ci => ci.Empleado)
                            .FirstOrDefaultAsync(ci => ci.checkin_id == id);

            if (checkIn == null)
                return new NotFoundResult();

            var viewModel = new CheckInViewModel
            {
                CheckInID = checkIn.checkin_id,
                ReservaID = checkIn.reserva_id,
                NombreCliente = checkIn.Reserva?.Cliente != null ? $"{checkIn.Reserva.Cliente.nombre} {checkIn.Reserva.Cliente.apellido}" : "N/A",
                FechaHora = (DateTime)checkIn.fecha_hora,
                MetodoPagoAdelanto = checkIn.metodo_pago_adelanto,
                DocumentosVerificados = checkIn.documentos_verificados ?? false,
                Observaciones = checkIn.observaciones,
                NombreEmpleado = checkIn.Empleado != null ? $"{checkIn.Empleado.nombre} {checkIn.Empleado.apellido}" : "N/A"
            };

            return new OkResult(viewModel, Request);
        }

        public void Dispose()
        {
            db.Dispose();
            _userManager?.Dispose();
        }
    }
}