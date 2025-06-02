using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ServicioAdicionalEventoBindingModel
    {
        [Required]
        [StringLength(100)]
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        [Required]
        public decimal PrecioBase { get; set; }
        public bool RequierePersonalPago { get; set; } = false;
    }
}