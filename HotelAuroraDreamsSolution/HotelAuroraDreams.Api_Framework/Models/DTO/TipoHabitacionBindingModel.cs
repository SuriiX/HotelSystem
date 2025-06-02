// File: ~/Models/DTO/TipoHabitacionBindingModel.cs (en el proyecto API)
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class TipoHabitacionBindingModel
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        [Range(0.01, 100000.00)]
        public decimal PrecioBase { get; set; }

        [Required]
        [Range(1, 10)]
        public int Capacidad { get; set; }

        public string Comodidades { get; set; }
    }
}