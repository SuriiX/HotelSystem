using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class TipoEventoBindingModel
    {
        [Required]
        [StringLength(100)]
        public string NombreTipo { get; set; }
        public string Descripcion { get; set; }
    }
}