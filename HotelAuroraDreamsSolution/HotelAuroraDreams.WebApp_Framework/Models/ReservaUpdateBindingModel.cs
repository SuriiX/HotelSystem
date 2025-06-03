using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class ReservaUpdateBindingModel
    {
        [Required(ErrorMessage = "El número de huéspedes es requerido.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser al menos 1.")]
        public int NumeroHuespedes { get; set; }

        public string Notas { get; set; }

        [Required(ErrorMessage = "El estado es requerido.")]
        [StringLength(20)]
        public string Estado { get; set; } // Ej: Confirmada, Hospedado, Completada, No Show
    }
}