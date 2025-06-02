
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class TipoEventoBindingModel
    {
        [Required(ErrorMessage = "El nombre del tipo de evento es requerido.")]
        [StringLength(100)]
        public string NombreTipo { get; set; }
        public string Descripcion { get; set; }
    }
}