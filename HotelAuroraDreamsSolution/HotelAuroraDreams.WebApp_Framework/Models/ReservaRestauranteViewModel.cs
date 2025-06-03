// File: ~/Models/ReservaRestauranteViewModel.cs (WebApp)
using System;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class ReservaRestauranteViewModel
    {
        public int ReservaRestauranteID { get; set; }
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; }
        public int RestauranteID { get; set; }
        public string NombreRestaurante { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraReserva { get; set; }
        public int NumeroComensales { get; set; }
        public string Estado { get; set; }
        public string Notas { get; set; }
        public string NombreEmpleadoRegistro { get; set; }
    }
}