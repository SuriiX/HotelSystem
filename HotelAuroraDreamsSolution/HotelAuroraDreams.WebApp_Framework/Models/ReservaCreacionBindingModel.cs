
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
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
        public int NumeroHuespedes { get; set; }
        [Required]
        [MinLength(1)]
        public List<int> HabitacionIDsSeleccionadas { get; set; }
        public string Notas { get; set; }
    }
}