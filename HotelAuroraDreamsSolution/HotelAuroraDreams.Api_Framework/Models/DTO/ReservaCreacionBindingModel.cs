using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaCreacionBindingModel
    {
        [Required]
        public int ClienteID { get; set; }

        [Required]
        public int HotelID { get; set; }

        [Required]
        public DateTime FechaEntrada { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser al menos 1.")]
        public int NumeroHuespedes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe seleccionar al menos una habitación.")]
        public List<int> HabitacionIDsSeleccionadas { get; set; }

        public string Notas { get; set; }
    }
}