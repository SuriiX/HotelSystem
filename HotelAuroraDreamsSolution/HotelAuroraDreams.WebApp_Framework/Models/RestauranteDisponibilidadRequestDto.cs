using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class RestauranteDisponibilidadRequestDto
    {
        [Required]
        public int RestauranteID { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public TimeSpan Hora { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroComensales { get; set; }
    }
}