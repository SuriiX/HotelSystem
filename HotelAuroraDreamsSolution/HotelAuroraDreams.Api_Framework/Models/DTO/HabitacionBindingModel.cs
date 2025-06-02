using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class HabitacionBindingModel
    {
        [Required]
        public int HotelID { get; set; }

        [Required]
        public int TipoHabitacionID { get; set; }

        [Required]
        [StringLength(10)]
        public string Numero { get; set; }

        [Required]
        public int Piso { get; set; }

        [StringLength(30)]
        public string Estado { get; set; } // Validar contra CK_Habitacion_Estado

        [StringLength(50)]
        public string Vista { get; set; }
        public string Descripcion { get; set; }
    }
}