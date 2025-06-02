namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class HabitacionViewModel
    {
        public int HabitacionID { get; set; }
        public int HotelID { get; set; }
        public string NombreHotel { get; set; } 
        public int TipoHabitacionID { get; set; }
        public string NombreTipoHabitacion { get; set; } 
        public string Numero { get; set; }
        public int Piso { get; set; }
        public string Estado { get; set; }
        public string Vista { get; set; }
        public string Descripcion { get; set; }
    }
}