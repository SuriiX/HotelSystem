namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaHabitacionViewModel
    {
        public int ReservaHabitacionID { get; set; }
        public int HabitacionID { get; set; }
        public string NumeroHabitacion { get; set; }
        public string NombreTipoHabitacion { get; set; }
        public decimal PrecioNocheCobrado { get; set; }
        public string EstadoAsignacion { get; set; }
    }
}