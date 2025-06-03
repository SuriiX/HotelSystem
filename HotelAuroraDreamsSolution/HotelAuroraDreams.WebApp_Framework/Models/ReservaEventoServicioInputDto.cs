// File: ~/Models/ReservaEventoServicioInputDto.cs (WebApp)
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class ReservaEventoServicioInputDto
    {
        [Required]
        public int ServicioAdicionalID { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
        [Required]
        public decimal PrecioCobradoPorUnidad { get; set; }
        public string Notas { get; set; }
    }
}