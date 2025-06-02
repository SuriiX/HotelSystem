using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaRestauranteBindingModel
    {
        [Required]
        public int ClienteID { get; set; }
        [Required]
        public int RestauranteID { get; set; }
        [Required]
        public DateTime FechaReserva { get; set; } // Fecha para la cual es la reserva
        [Required]
        public TimeSpan HoraReserva { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroComensales { get; set; }
        public string Notas { get; set; }
    }
}