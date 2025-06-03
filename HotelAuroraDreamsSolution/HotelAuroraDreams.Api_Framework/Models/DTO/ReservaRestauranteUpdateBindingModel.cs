// File: ~/Models/DTO/ReservaRestauranteUpdateBindingModel.cs (API Project)
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaRestauranteUpdateBindingModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroComensales { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } 

        public string Notas { get; set; }
    }
}