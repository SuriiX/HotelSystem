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
    public class ClsCheckOut : IDisposable

    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ClsCheckOut() { }

        public ClsCheckOut(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public async Task<IHttpActionResult> ProcesarCheckOut(CheckOutBindingModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return new BadRequestResult(ModelState, Request);
            }

            var reserva = await db.Reservas
                                  .Include(r => r.Cliente)
                                  .Include(r => r.Hotel)
                                  .Include(r => r.Reserva_Habitacion.Select(rh => rh.Habitacion.Tipo_Habitacion))
                                  .Include(r => r.Consumo_Servicio.Select(cs => cs.Servicio))
                                  .FirstOrDefaultAsync(r => r.reserva_id == model.ReservaID);

            if (reserva == null)
            {
                return new NotFoundResultWithMessage($"Reserva con ID {model.ReservaID} no encontrada.");
            }

            if (reserva.Estado != "Hospedado")
            {
                return new BadRequestResult($"La reserva ID {model.ReservaID} no está en estado 'Hospedado'. Estado actual: {reserva.Estado}.", Request);
            }

            var checkInExistente = await db.CheckIns.FirstOrDefaultAsync(ci => ci.reserva_id == model.ReservaID);
            if (checkInExistente == null)
            {
                return new BadRequestResult($"No se encontró un check-in previo para la reserva ID {model.ReservaID}. No se puede procesar el check-out.", Request);
            }

            string empleadoAspNetUserId = HttpContext.Current.User.Identity.GetUserId();
            var empleadoAppUser = await UserManager.FindByIdAsync(empleadoAspNetUserId);
            if (empleadoAppUser == null)
            {
                return new BadRequestResult("No se pudo identificar al empleado (Identity User not found).", Request);
            }

            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                return new BadRequestResult("No se encontró el registro en la tabla Empleado para el usuario autenticado.", Request);
            }
            int empleadoIdInt = empleadoDb.empleado_id;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal subtotalHabitaciones = 0;
                    int noches = Math.Max(1, (reserva.fecha_salida.Date - reserva.FechaEntrada.Date).Days);

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
                    decimal impuestosCalculados = subtotalGeneral * 0.19m;
                    decimal totalFacturaCalculado = subtotalGeneral + impuestosCalculados;

                    Factura nuevaFactura = new Factura
                    {
                        reserva_id = reserva.reserva_id,
                        cliente_id = reserva.ClienteID,
                        fecha_emision = DateTime.Now,
                        subtotal = subtotalGeneral,
                        impuestos = impuestosCalculados,
                        total = totalFacturaCalculado,
                        metodo_pago_factura = model.MetodoPagoFinal,
                        estado = "Pagada",
                        empleado_emisor_id = empleadoIdInt
                    };
                    db.Facturas.Add(nuevaFactura);
                    await db.SaveChangesAsync();

                    foreach (var rh in reserva.Reserva_Habitacion)
                    {
                        Detalle_Factura detalleEstancia = new Detalle_Factura
                        {
                            factura_id = nuevaFactura.factura_id,
                            tipo_concepto = "habitacion",
                            referencia_id = rh.habitacion_id,
                            descripcion_concepto = $"Estancia Hab. {rh.Habitacion.numero} ({rh.Habitacion.Tipo_Habitacion.nombre}), {noches} noches",
                            cantidad = noches,
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
                        total_factura = totalFacturaCalculado,
                        metodo_pago_final = model.MetodoPagoFinal,
                        observaciones = model.Observaciones
                    };
                    db.CheckOuts.Add(nuevoCheckOut);
                    reserva.Estado = "Completada";
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

                    var empleado = await db.Empleadoes.FindAsync(empleadoIdInt);
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

                    return new CreatedAtRouteResult("GetCheckOutById", new { id = nuevoCheckOut.checkout_id }, viewModel);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    return new InternalServerErrorResult(new Exception($"DbUpdateException al procesar check-out: {dbEx.ToString()}"));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new InternalServerErrorResult(new Exception($"Error general al procesar check-out: {ex.ToString()}"));
                }
            }
        }

        public async Task<IHttpActionResult> ObtenerCheckOutPorId(int id)
        {
            var checkOut = await db.CheckOuts
                            .Include(co => co.Reserva.Cliente)
                            .Include(co => co.Empleado)
                            .FirstOrDefaultAsync(co => co.checkout_id == id);

            if (checkOut == null) return new NotFoundResult();

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

            return new OkResult(viewModel, Request);
        }

        public void Dispose()
        {
            db.Dispose();
            _userManager?.Dispose();
        }
    }
}