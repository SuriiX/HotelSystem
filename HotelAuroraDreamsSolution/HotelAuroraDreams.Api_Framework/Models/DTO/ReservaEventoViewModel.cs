using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaEventoServicioViewModel
    {
        public int ServicioAdicionalID { get; set; }
        public string NombreServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCobradoPorUnidad { get; set; }
        public decimal Subtotal { get; set; }
        public string Notas { get; set; }
    }
    public class ReservaEventoViewModel
    {
        public int ReservaEventoID { get; set; }
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; }
        public int SalonEventoID { get; set; }
        public string NombreSalon { get; set; }
        public int? TipoEventoID { get; set; }
        public string NombreTipoEvento { get; set; }
        public string NombreEvento { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEvento { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int NumeroAsistentesEstimado { get; set; }
        public int? NumeroAsistentesConfirmado { get; set; }
        public string NombreEmpleadoResponsable { get; set; }
        public string Estado { get; set; }
        public string NotasGenerales { get; set; }
        public decimal? MontoEstimadoSalon { get; set; }
        public decimal? MontoEstimadoServicios { get; set; } // Suma de subtotales de servicios
        public decimal MontoTotalEvento { get; set; } // Calculado
        public List<ReservaEventoServicioViewModel> ServiciosAdicionales { get; set; }
    }
}