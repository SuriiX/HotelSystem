// File: ~/Models/DTO/RestauranteDisponibilidadRequestDto.cs (API Project)
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class RestauranteDisponibilidadRequestDto
    {
        [Required]
        public int RestauranteID { get; set; }
        [Required]
        public DateTime Fecha { get; set; } // Fecha para la reserva
        [Required]
        public TimeSpan Hora { get; set; } // Hora deseada para la reserva
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroComensales { get; set; }
    }
}