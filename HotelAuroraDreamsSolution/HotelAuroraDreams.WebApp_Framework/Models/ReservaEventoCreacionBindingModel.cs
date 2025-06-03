// File: ~/Models/ReservaEventoCreacionBindingModel.cs (WebApp)
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
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
        public TimeSpan HoraInicio { get; set; }
        [Required]
        public TimeSpan HoraFin { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroAsistentesEstimado { get; set; }
        public string NotasGenerales { get; set; }
        public List<ReservaEventoServicioInputDto> ServiciosAdicionales { get; set; } = new List<ReservaEventoServicioInputDto>();
    }
}