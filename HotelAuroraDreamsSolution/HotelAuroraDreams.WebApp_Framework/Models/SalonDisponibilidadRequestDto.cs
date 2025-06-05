
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    [Serializable]
    public class SalonDisponibilidadRequestDto
    {
        [Required]
        public int SalonEventoID { get; set; }
        [Required]
        public DateTime FechaEvento { get; set; }
        [Required]
        public TimeSpan HoraInicio { get; set; }
        [Required]
        public TimeSpan HoraFin { get; set; }
    }
}