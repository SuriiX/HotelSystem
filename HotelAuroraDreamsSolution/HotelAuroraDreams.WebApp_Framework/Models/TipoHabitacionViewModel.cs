
using HotelAuroraDreams.WebApp_Framework.Models;
public class TipoHabitacionViewModel
{
    public int tipo_habitacion_id { get; set; }
    public string nombre { get; set; }
    public string descripcion { get; set; }
    public decimal precio_base { get; set; }
    public int capacidad { get; set; }
    public string comodidades { get; set; }
}
