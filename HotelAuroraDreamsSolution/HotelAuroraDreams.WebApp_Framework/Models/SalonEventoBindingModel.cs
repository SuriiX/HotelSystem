
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class SalonEventoBindingModel
    {
        [Required(ErrorMessage = "El Hotel es requerido.")]
        public int HotelID { get; set; }

        [Required(ErrorMessage = "El Nombre del salón es requerido.")]
        [StringLength(150)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La Capacidad Máxima es requerida.")]
        [Range(1, 10000, ErrorMessage = "La capacidad debe ser mayor a 0.")] 
        public int CapacidadMaxima { get; set; }

        [StringLength(100)]
        public string Ubicacion { get; set; }

        public decimal? PrecioPorHora { get; set; } 
        public bool EstaActivo { get; set; } = true;
    }
}