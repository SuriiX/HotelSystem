using HotalSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotalSystem.Clases
{
    public class ClsChekIn
    {

        private DBHotelEntities db = new DBHotelEntities();

        public CheckIn checkin { get; set; }

        public string Insertar()
        {
            try
            {
                
                var reserva = db.Reservas.FirstOrDefault(r => r.reserva_id == checkin.reserva_id);
                if (reserva == null)
                    return "La reserva no existe.";

                if (reserva.estado.ToLower() != "confirmada")
                    return "La reserva no está confirmada.";
                
                if (db.CheckIns.Any(c => c.reserva_id == checkin.reserva_id))
                    return "Esta reserva ya tiene un check-in registrado.";

                string[] metodosValidos = { "efectivo", "tarjeta", "transferencia", "otro" };
                if (!metodosValidos.Contains(checkin.metodo_pago_adelanto.ToLower()))
                    return "Método de pago no válido.";

                checkin.fecha_hora = DateTime.Now;
                db.CheckIns.Add(checkin);

                reserva.estado = "completada";

                var habitaciones = db.Reserva_Habitacion
                                     .Where(rh => rh.reserva_id == checkin.reserva_id)
                                     .ToList();

                foreach (var rh in habitaciones)
                {
                    var habitacion = db.Habitacions.FirstOrDefault(h => h.habitacion_id == rh.habitacion_id);
                    if (habitacion != null)
                        habitacion.estado = "ocupada";
                }

                db.SaveChanges();
                return "Check-In registrado correctamente.";
            }
            catch (Exception ex)
            {
                return "Error al realizar el check-in: " + ex.Message;
            }
        }

        public CheckIn ConsultarPorReserva(int reservaId)
        {
            return db.CheckIns.FirstOrDefault(c => c.reserva_id == reservaId);
        }

        public List<CheckIn> ConsultarTodos()
        {
            return db.CheckIns.OrderBy(c => c.fecha_hora).ToList();
        }
    }

}

