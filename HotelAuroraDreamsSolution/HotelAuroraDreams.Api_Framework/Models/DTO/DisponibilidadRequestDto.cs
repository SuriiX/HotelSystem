using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class DisponibilidadRequestDto
    {
        [Required]
        public DateTime FechaEntrada { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; }

        [Required]
        public int HotelID { get; set; }

        public int? TipoHabitacionID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser al menos 1.")]
        public int NumeroHuespedes { get; set; }
    }
}