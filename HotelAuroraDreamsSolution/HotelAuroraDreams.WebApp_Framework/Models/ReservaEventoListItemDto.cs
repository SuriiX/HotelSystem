
using System;

[Serializable]
public class ReservaEventoListItemDto
{
    public int ReservaEventoID { get; set; }
    public string NombreEvento { get; set; }
    public string NombreCliente { get; set; }
    public string NombreSalon { get; set; }
    public DateTime FechaEvento { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public string Estado { get; set; }
}