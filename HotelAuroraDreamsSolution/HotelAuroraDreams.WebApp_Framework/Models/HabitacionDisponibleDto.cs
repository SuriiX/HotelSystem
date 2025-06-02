
namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class HabitacionDisponibleDto
    {
        public int HabitacionID { get; set; }
        public string NumeroHabitacion { get; set; }
        public int TipoHabitacionID { get; set; }
        public string NombreTipoHabitacion { get; set; }
        public string DescripcionTipoHabitacion { get; set; }
        public decimal PrecioNoche { get; set; }
        public int Capacidad { get; set; }
        public string Comodidades { get; set; }
        public string Vista { get; set; }
        public int? Piso { get; set; }
    }
}