// File: ~/Models/HabitacionBindingModel.cs (en HotelAuroraDreams.WebApp_Framework)
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class HabitacionBindingModel
    {
        [Required(ErrorMessage = "El Hotel es requerido.")]
        public int HotelID { get; set; }

        [Required(ErrorMessage = "El Tipo de Habitación es requerido.")]
        public int TipoHabitacionID { get; set; }

        [Required(ErrorMessage = "El Número de habitación es requerido.")]
        [StringLength(10)]
        public string Numero { get; set; }

        [Required(ErrorMessage = "El Piso es requerido.")]
        public int Piso { get; set; }

        [StringLength(30)]
        public string Estado { get; set; } 

        [StringLength(50)]
        public string Vista { get; set; }
        public string Descripcion { get; set; }
    }
}