// File: ~/Controllers/CheckInController.csMore actions
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
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
    [RoutePrefix("api/CheckIn")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class CheckInController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public CheckInController() { }

        public CheckInController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        // POST: api/CheckIn
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CheckInViewModel))]
        public async Task<IHttpActionResult> PostCheckIn(CheckInBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Validar Reserva
            var reserva = await db.Reservas
                                  .Include(r => r.Reserva_Habitacion.Select(rh => rh.Habitacion))
                                  .FirstOrDefaultAsync(r => r.reserva_id == model.ReservaID);

            if (reserva == null)
            {
                return NotFoundResultWithMessage($"Reserva con ID {model.ReservaID} no encontrada.");
            }
            if (reserva.Estado != "Confirmada")
            {
                return BadRequest($"La reserva ID {model.ReservaID} no está en estado 'Confirmada'. Estado actual: {reserva.Estado}.");
            }
            if (reserva.FechaEntrada.Date > DateTime.Today)
            {
            }
            if (await db.CheckIns.AnyAsync(ci => ci.reserva_id == model.ReservaID))
            {
                return BadRequest($"Ya se ha realizado un check-in para la reserva ID {model.ReservaID}.");
            }


            // 2. Obtener Empleado que realiza el Check-In
            string empleadoAspNetUserId = User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);
            if (empleadoAppUser == null)
            {
                return BadRequest("No se pudo identificar al empleado (Identity User not found).");
            }
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return BadRequest("No se encontró el registro en la tabla Empleado para el usuario autenticado.");
            }
            int empleadoIdInt = empleadoDb.empleado_id;

            CheckIn nuevoCheckIn = new CheckIn
            {
                reserva_id = model.ReservaID,
                empleado_id = empleadoIdInt,
                fecha_hora = DateTime.Now,
                metodo_pago_adelanto = model.MetodoPagoAdelanto,
                documentos_verificados = model.DocumentosVerificados,
                observaciones = model.Observaciones
            };
            db.CheckIns.Add(nuevoCheckIn);

            // 3. Actualizar Estado de la Reserva
            reserva.Estado = "Hospedado"; // Nuevo estado
            db.Entry(reserva).State = EntityState.Modified;

            // 4. Actualizar Estado de las Habitaciones a "Ocupada"
            foreach (var reservaHabitacion in reserva.Reserva_Habitacion)
            {
                var habitacion = reservaHabitacion.Habitacion; // Ya está cargada por el Include
                if (habitacion != null)
                {
                    habitacion.estado = "ocupada";
                    db.Entry(habitacion).State = EntityState.Modified;
                }
            }

            try
            {
                await db.SaveChangesAsync(); // Guardar todos los cambios
            }
            catch (DbUpdateException dbEx)
            {
                return InternalServerError(new Exception($"DbUpdateException al procesar check-in: {dbEx.ToString()}"));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error general al procesar check-in: {ex.ToString()}"));
            }

            var cliente = await db.Clientes.FindAsync(reserva.ClienteID);

            var viewModel = new CheckInViewModel
            {
                CheckInID = nuevoCheckIn.checkin_id,
                ReservaID = nuevoCheckIn.reserva_id,
                NombreCliente = cliente != null ? $"{cliente.nombre} {cliente.apellido}" : "N/A",
                FechaHora = (DateTime)nuevoCheckIn.fecha_hora,
                MetodoPagoAdelanto = nuevoCheckIn.metodo_pago_adelanto,
                DocumentosVerificados = (bool)nuevoCheckIn.documentos_verificados,
                Observaciones = nuevoCheckIn.observaciones,
                NombreEmpleado = $"{empleadoDb.nombre} {empleadoDb.apellido}"
            };

            return CreatedAtRoute("GetCheckInById", new { id = nuevoCheckIn.checkin_id }, viewModel);

        }

        // GET: api/CheckIn/{id} (Ejemplo de endpoint para que CreatedAtRoute funcione)
        [HttpGet]
        [Route("{id:int}", Name = "GetCheckInById")]
        [ResponseType(typeof(CheckInViewModel))]
        public async Task<IHttpActionResult> GetCheckIn(int id)
        {
            var checkIn = await db.CheckIns
                            .Include(ci => ci.Reserva.Cliente)
                            .Include(ci => ci.Empleado)
                            .FirstOrDefaultAsync(ci => ci.checkin_id == id);

            if (checkIn == null)
            {
                return NotFound();
            }

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
            return Ok(viewModel);
        }

        // Helper para mensajes NotFound con contenido JSON
        private IHttpActionResult NotFoundResultWithMessage(string message)
        {
            return Content(HttpStatusCode.NotFound, new { Message = message });
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