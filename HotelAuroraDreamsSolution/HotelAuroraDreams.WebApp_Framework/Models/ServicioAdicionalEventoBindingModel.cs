
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class ServicioAdicionalEventoBindingModel
    {
        [Required(ErrorMessage = "El nombre del servicio es requerido.")]
        [StringLength(100)]
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "El precio base es requerido.")]
        [Range(0.00, 1000000.00, ErrorMessage = "El precio debe ser un valor positivo.")] // Permite 0 si algunos servicios son informativos
        public decimal PrecioBase { get; set; }
        public bool RequierePersonalPago { get; set; } = false;
    }
}