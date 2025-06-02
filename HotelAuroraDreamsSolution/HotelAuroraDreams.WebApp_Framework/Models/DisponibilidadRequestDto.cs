
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    [Serializable]
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
        public int NumeroHuespedes { get; set; }
    }
}