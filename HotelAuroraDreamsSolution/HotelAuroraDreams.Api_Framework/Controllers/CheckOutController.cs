// File: ~/Controllers/CheckOutController.csMore actions
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic; // Para List
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
    [RoutePrefix("api/CheckOut")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class CheckOutController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public CheckOutController() { }

        public CheckOutController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        // POST: api/CheckOut
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CheckOutViewModel))]
        public async Task<IHttpActionResult> PostCheckOut(CheckOutBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Validar Reserva y Check-In existente
            var reserva = await db.Reservas
                                  .Include(r => r.Cliente)
                                  .Include(r => r.Hotel)
                                  .Include(r => r.Reserva_Habitacion.Select(rh => rh.Habitacion.Tipo_Habitacion))
                                  .Include(r => r.Consumo_Servicio.Select(cs => cs.Servicio)) // Incluir consumos
                                  .FirstOrDefaultAsync(r => r.reserva_id == model.ReservaID);

            if (reserva == null)
            {
                return NotFoundResultWithMessage($"Reserva con ID {model.ReservaID} no encontrada.");
            }
            if (reserva.estado != "Hospedado") // Solo se puede hacer check-out si está "Hospedado"
            {
                return BadRequest($"La reserva ID {model.ReservaID} no está en estado 'Hospedado'. Estado actual: {reserva.estado}.");
            }


            var checkInExistente = await db.CheckIns.FirstOrDefaultAsync(ci => ci.reserva_id == model.ReservaID);
            if (checkInExistente == null)

            {
                return BadRequest($"No se encontró un check-in previo para la reserva ID {model.ReservaID}. No se puede procesar el check-out.");
            }

            // 2. Obtener Empleado que realiza el Check-Out
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

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal subtotalHabitaciones = 0;
                    int noches = Math.Max(1, (reserva.fecha_salida.Date - reserva.fecha_entrada.Date).Days); // Noches reservadas

                    foreach (var rh in reserva.Reserva_Habitacion)
                    {
                        subtotalHabitaciones += rh.precio_noche_cobrado * noches;
                    }

                    decimal subtotalServicios = 0;
                    foreach (var cs in reserva.Consumo_Servicio)
                    {
                        subtotalServicios += cs.precio_unitario_cobrado * cs.cantidad;
                    }

                    decimal subtotalGeneral = subtotalHabitaciones + subtotalServicios;
                    decimal impuestosCalculados = subtotalGeneral * 0.19m; // Ejemplo: IVA 19%
                    decimal totalFacturaCalculado = subtotalGeneral + impuestosCalculados;

                    // 4. Crear registro de Factura
                    Factura nuevaFactura = new Factura
                    {
                        reserva_id = reserva.reserva_id,
                        cliente_id = reserva.cliente_id,
                        fecha_emision = DateTime.Now,
                        subtotal = subtotalGeneral,
                        impuestos = impuestosCalculados,
                        total = totalFacturaCalculado,
                        metodo_pago_factura = model.MetodoPagoFinal,
                        estado = "Pagada", // Asumimos que se paga al hacer check-out
                        empleado_emisor_id = empleadoIdInt
                    };
                    db.Facturas.Add(nuevaFactura);
                    await db.SaveChangesAsync(); // Guardar para obtener nuevaFactura.factura_id

                    // 5. Crear Detalles de la Factura
                    // Detalle por estancia
                    foreach (var rh in reserva.Reserva_Habitacion)
                    {
                        Detalle_Factura detalleEstancia = new Detalle_Factura
                        {
                            factura_id = nuevaFactura.factura_id,
                            tipo_concepto = "habitacion",
                            referencia_id = rh.habitacion_id,
                            descripcion_concepto = $"Estancia Hab. {rh.Habitacion.numero} ({rh.Habitacion.Tipo_Habitacion.nombre}), {noches} noches",
                            cantidad = noches, // O 1 si el precio es por la estancia total
                            precio_unitario = rh.precio_noche_cobrado,
                            subtotal = rh.precio_noche_cobrado * noches
                        };
                        db.Detalle_Factura.Add(detalleEstancia);
                    }
                    foreach (var cs in reserva.Consumo_Servicio)
                    {
                        Detalle_Factura detalleServicio = new Detalle_Factura
                        {
                            factura_id = nuevaFactura.factura_id,
                            tipo_concepto = "servicio_consumido",
                            referencia_id = cs.servicio_id,
                            descripcion_concepto = $"{cs.Servicio.nombre} (x{cs.cantidad})",
                            cantidad = cs.cantidad,
                            precio_unitario = cs.precio_unitario_cobrado,
                            subtotal = cs.precio_unitario_cobrado * cs.cantidad
                        };
                        db.Detalle_Factura.Add(detalleServicio);
                    }

                    CheckOut nuevoCheckOut = new CheckOut
                    {
                        reserva_id = model.ReservaID,
                        empleado_id = empleadoIdInt,
                        fecha_hora = DateTime.Now,
                        total_factura = totalFacturaCalculado, // Total final de la factura
                        metodo_pago_final = model.MetodoPagoFinal,
                        observaciones = model.Observaciones
                    };
                    db.CheckOuts.Add(nuevoCheckOut);
                    reserva.estado = "Completada";
                    db.Entry(reserva).State = EntityState.Modified;

                    foreach (var reservaHabitacion in reserva.Reserva_Habitacion)
                    {
                        if (reservaHabitacion.Habitacion != null)
                        {
                            reservaHabitacion.Habitacion.estado = "limpieza_pendiente";
                            db.Entry(reservaHabitacion.Habitacion).State = EntityState.Modified;
                        }
                    }

                    await db.SaveChangesAsync();
                    transaction.Commit();

                    var viewModel = new CheckOutViewModel
                    {
                        CheckOutID = nuevoCheckOut.checkout_id,
                        ReservaID = nuevoCheckOut.reserva_id,
                        NombreCliente = reserva.Cliente != null ? $"{reserva.Cliente.nombre} {reserva.Cliente.apellido}" : "N/A",
                        FechaHora = (DateTime)nuevoCheckOut.fecha_hora,
                        TotalFactura = nuevoCheckOut.total_factura,
                        MetodoPagoFinal = nuevoCheckOut.metodo_pago_final,
                        Observaciones = nuevoCheckOut.observaciones,
                        NombreEmpleado = $"{empleadoDb.nombre} {empleadoDb.apellido}",
                        FacturaID = nuevaFactura.factura_id
                    };
                    // Usar el nombre de ruta del GetCheckOutById que crearemos
                    return CreatedAtRoute("GetCheckOutById", new { id = nuevoCheckOut.checkout_id }, viewModel);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    return InternalServerError(new Exception($"DbUpdateException al procesar check-out: {dbEx.ToString()}"));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(new Exception($"Error general al procesar check-out: {ex.ToString()}"));
                }
            } // Fin using transaction
        }

        // GET: api/CheckOut/{id} (Para que CreatedAtRoute funcione)
        [HttpGet]
        [Route("{id:int}", Name = "GetCheckOutById")]
        [ResponseType(typeof(CheckOutViewModel))]
        public async Task<IHttpActionResult> GetCheckOut(int id)
        {
            var checkOut = await db.CheckOuts
                            .Include(co => co.Reserva.Cliente)
                            .Include(co => co.Empleado)
                            .FirstOrDefaultAsync(co => co.checkout_id == id);

            if (checkOut == null) return NotFound();

            // Para obtener el FacturaID, necesitamos buscarlo ya que no hay FK directa desde CheckOut a Factura
            // (La FK es de Factura a Reserva)
            var facturaAsociada = await db.Facturas.FirstOrDefaultAsync(f => f.reserva_id == checkOut.reserva_id);

            var viewModel = new CheckOutViewModel
            {
                CheckOutID = checkOut.checkout_id,
                ReservaID = checkOut.reserva_id,
                NombreCliente = checkOut.Reserva?.Cliente != null ? $"{checkOut.Reserva.Cliente.nombre} {checkOut.Reserva.Cliente.apellido}" : "N/A",
                FechaHora = (DateTime)checkOut.fecha_hora,
                TotalFactura = checkOut.total_factura,
                MetodoPagoFinal = checkOut.metodo_pago_final,
                Observaciones = checkOut.observaciones,
                NombreEmpleado = checkOut.Empleado != null ? $"{checkOut.Empleado.nombre} {checkOut.Empleado.apellido}" : "N/A",
                FacturaID = facturaAsociada?.factura_id
            };
            return Ok(viewModel);
        }

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