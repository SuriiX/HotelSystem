using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class SalonEventoBindingModel
    {
        [Required]
        public int HotelID { get; set; }
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        [Required]
        public int CapacidadMaxima { get; set; }
        [StringLength(100)]
        public string Ubicacion { get; set; }
        public decimal? PrecioPorHora { get; set; }
        public bool EstaActivo { get; set; } = true;
    }
}