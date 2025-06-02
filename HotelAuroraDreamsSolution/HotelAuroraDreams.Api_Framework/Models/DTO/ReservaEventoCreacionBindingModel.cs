using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaEventoServicioInputDto // Para los servicios dentro de la reserva
    {
        [Required]
        public int ServicioAdicionalID { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
        [Required]
        public decimal PrecioCobradoPorUnidad { get; set; } // Puede ser diferente al precio base del servicio
        public string Notas { get; set; }
    }

    public class ReservaEventoCreacionBindingModel
    {
        [Required]
        public int ClienteID { get; set; }
        [Required]
        public int SalonEventoID { get; set; }
        public int? TipoEventoID { get; set; }
        [Required]
        [StringLength(200)]
        public string NombreEvento { get; set; }
        [Required]
        public DateTime FechaEvento { get; set; }
        [Required]
        public TimeSpan HoraInicio { get; set; } // TimeSpan para la hora
        [Required]
        public TimeSpan HoraFin { get; set; }   // TimeSpan para la hora
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroAsistentesEstimado { get; set; }
        public string NotasGenerales { get; set; }
        public List<ReservaEventoServicioInputDto> ServiciosAdicionales { get; set; } = new List<ReservaEventoServicioInputDto>();
    }
}