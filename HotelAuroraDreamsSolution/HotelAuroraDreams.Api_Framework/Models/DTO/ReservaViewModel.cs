using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaViewModel
    {
        public int ReservaID { get; set; }
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; }
        public string EmailCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public int HotelID { get; set; }
        public string NombreHotel { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime FechaSalida { get; set; }
        public string Estado { get; set; }
        public int NumeroHuespedes { get; set; }
        public string Notas { get; set; }
        public string NombreEmpleadoRegistro { get; set; }
        public List<ReservaHabitacionViewModel> HabitacionesReservadas { get; set; }
        public decimal MontoTotalReserva { get; set; }
    }
}