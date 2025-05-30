//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HotalSystem.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    
    public partial class Habitacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Habitacion()
        {
            this.Reserva_Habitacion = new HashSet<Reserva_Habitacion>();
        }
    
        public int habitacion_id { get; set; }
        public int hotel_id { get; set; }
        public int tipo_habitacion_id { get; set; }
        public string numero { get; set; }
        public int piso { get; set; }
        public string estado { get; set; }
        public string vista { get; set; }
        public string descripcion { get; set; }

        [JsonIgnore]
        public virtual Hotel Hotel { get; set; }

        [JsonIgnore]
        public virtual Tipo_Habitacion Tipo_Habitacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Reserva_Habitacion> Reserva_Habitacion { get; set; }
    }
}
