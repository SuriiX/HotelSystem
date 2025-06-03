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
using System.Threading.Tasks;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsReservaRestaurante : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get => _userManager;
            set => _userManager = value;
        }

        public ClsReservaRestaurante(ApplicationUserManager userManager = null)
        {
            UserManager = userManager;
        }

        public async Task<RestauranteDisponibleViewModel> VerificarDisponibilidadRestaurante(RestauranteDisponibilidadRequestDto requestDto)
        {
            var restaurante = await db.Restaurantes.FindAsync(requestDto.RestauranteID);
            if (restaurante == null)
            {
                throw new KeyNotFoundException($"Restaurante con ID {requestDto.RestauranteID} no encontrado.");
            }

            if (!restaurante.capacidad_mesas.HasValue || restaurante.capacidad_mesas.Value <= 0)
            {
                return new RestauranteDisponibleViewModel
                {
                    RestauranteID = restaurante.restaurante_id,
                    NombreRestaurante = restaurante.nombre,
                    Disponible = false,
                    Mensaje = "Restaurante no tiene capacidad definida o no acepta reservas."
                };
            }

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
                return new RestauranteDisponibleViewModel
                {
                    RestauranteID = restaurante.restaurante_id,
                    NombreRestaurante = restaurante.nombre,
                    Disponible = true,
                    Mensaje = "Restaurante disponible.",
                    CapacidadRestanteEstimada = capacidadRestante - requestDto.NumeroComensales
                };
            }

            return new RestauranteDisponibleViewModel
            {
                RestauranteID = restaurante.restaurante_id,
                NombreRestaurante = restaurante.nombre,
                Disponible = false,
                Mensaje = $"No hay suficiente capacidad. Capacidad restante estimada: {Math.Max(0, capacidadRestante)} comensales.",
                CapacidadRestanteEstimada = Math.Max(0, capacidadRestante)
            };
        }

        public async Task<ReservaRestauranteViewModel> CrearReserva(ReservaRestauranteBindingModel model, string userId)
        {
            var empleadoAppUser = await UserManager.FindByIdAsync(userId);
            var empleadoDb = await db.Empleadoes.FirstOrDefaultAsync(e => e.email == empleadoAppUser.Email);
            if (empleadoDb == null)
            {
                throw new InvalidOperationException("No se encontró el registro de empleado para el usuario autenticado.");
            }

            var restaurante = await db.Restaurantes.FindAsync(model.RestauranteID);
            if (restaurante == null) throw new KeyNotFoundException("Restaurante no válido.");

            Reserva_Restaurante nuevaReserva = new Reserva_Restaurante
            {
                cliente_id = model.ClienteID,
                restaurante_id = model.RestauranteID,
                fecha_reserva = model.FechaReserva.Date,
                hora_reserva = model.HoraReserva,
                numero_comensales = model.NumeroComensales,
                estado = "Confirmada",
                notas = model.Notas,
                empleado_registro_id = empleadoDb.empleado_id
            };

            db.Reserva_Restaurante.Add(nuevaReserva);
            await db.SaveChangesAsync();

            var cliente = await db.Clientes.FindAsync(nuevaReserva.cliente_id);

            return new ReservaRestauranteViewModel
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
        }

        public async Task<List<ReservaRestauranteViewModel>> ObtenerReservas(int? clienteId = null, int? restauranteId = null, DateTime? fecha = null)
        {
            var query = db.Reserva_Restaurante.AsQueryable();
            if (clienteId.HasValue) query = query.Where(rr => rr.cliente_id == clienteId.Value);
            if (restauranteId.HasValue) query = query.Where(rr => rr.restaurante_id == restauranteId.Value);
            if (fecha.HasValue) query = query.Where(rr => rr.fecha_reserva == fecha.Value.Date);

            return await query
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
                    NombreEmpleadoRegistro = rr.Empleado != null ? rr.Empleado.nombre + " " + rr.Empleado.apellido : null
                })
                .ToListAsync();
        }

        public async Task<ReservaRestauranteViewModel> ObtenerReservaPorId(int id)
        {
            var rr = await db.Reserva_Restaurante
                .Include(r => r.Cliente)
                .Include(r => r.Restaurante)
                .Include(r => r.Empleado)
                .FirstOrDefaultAsync(r => r.reserva_restaurante_id == id);

            if (rr == null) return null;

            return new ReservaRestauranteViewModel
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
        }

        public async Task ActualizarReserva(int id, ReservaRestauranteUpdateBindingModel model)
        {
            var reserva = await db.Reserva_Restaurante.FindAsync(id);
            if (reserva == null) throw new KeyNotFoundException();

            reserva.numero_comensales = model.NumeroComensales;
            reserva.estado = model.Estado;
            reserva.notas = model.Notas;

            db.Entry(reserva).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task CancelarReserva(int id)
        {
            var reserva = await db.Reserva_Restaurante.FindAsync(id);
            if (reserva == null) throw new KeyNotFoundException();

            if (reserva.estado == "Cancelada" || reserva.estado == "Atendida") return;

            reserva.estado = "Cancelada";
            db.Entry(reserva).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (_userManager != null) _userManager.Dispose();
            db.Dispose();
        }
    }
}