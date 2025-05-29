using HotalSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace HotalSystem.Clases
{
	public class ClsReserva
	{

		private DBHotelEntities db = new DBHotelEntities();

		public Reserva reserva { get; set; }

        public string Insertar()
        {
            try
            {
                if (reserva == null)
                    return "Reserva no proporcionada.";

                var existeReservaActiva = db.Reservas.Any(r =>
                    r.estado == "Activa" && r.reserva_id == reserva.reserva_id);

                if (existeReservaActiva)
                    return "Ya existe una reserva activa con este ID.";

                reserva.estado = "Activa";
                reserva.fecha_reserva = DateTime.Now;

                db.Reservas.Add(reserva);
                db.SaveChanges();

                return "Reserva creada correctamente.";
            }
            catch (Exception ex)
            {
                return "Error al crear la reserva: " + ex.Message;
            }
        }

        public List<Reserva> ObtenerReservasActivas()
        {
            return db.Reservas.Where(r => r.estado == "Activa").ToList();
        }

        public Reserva ConsultarPorId(int id)
        {
            return db.Reservas.FirstOrDefault(r => r.reserva_id == id);
        }

        public string Cancelar(int id)
        {
            try
            {
                var reservaCancelar = db.Reservas.FirstOrDefault(r => r.reserva_id == id);
                if (reservaCancelar == null)
                    return "La reserva no existe.";

                if (reservaCancelar.estado == "Cancelada")
                    return "La reserva ya está cancelada.";

                reservaCancelar.estado = "Cancelada";
                db.SaveChanges();

                return "Reserva cancelada correctamente.";
            }
            catch (Exception ex)
            {
                return "Error al cancelar la reserva: " + ex.Message;
            }
        }


        public string Actualizar()
        {
            try
            {
                var reservaExistente = db.Reservas.FirstOrDefault(r => r.reserva_id == reserva.reserva_id);
                if (reservaExistente == null)
                {
                    return "La reserva con el ID ingresado no existe, por lo tanto no se puede actualizar.";
                }

                db.Reservas.AddOrUpdate(reserva);
                db.SaveChanges();

                return "Se actualizó la reserva correctamente.";
            }
            catch (Exception ex)
            {
                return "No se pudo actualizar la reserva: " + ex.Message;
            }
        }


    }
}