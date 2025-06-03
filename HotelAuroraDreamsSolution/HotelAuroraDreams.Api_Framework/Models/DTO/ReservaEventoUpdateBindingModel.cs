using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaEventoUpdateBindingModel
    {

        [Required]
        [StringLength(200)]
        public string NombreEvento { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroAsistentesEstimado { get; set; }

        public int? NumeroAsistentesConfirmado { get; set; }

        public string NotasGenerales { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } // Ej: Solicitada, Confirmada, En Curso, Cancelada, Realizada

        public List<ReservaEventoServicioInputDto> ServiciosAdicionales { get; set; } = new List<ReservaEventoServicioInputDto>();
    }
}