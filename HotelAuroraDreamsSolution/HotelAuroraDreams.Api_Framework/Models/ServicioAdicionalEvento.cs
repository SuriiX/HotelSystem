//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HotelAuroraDreams.Api_Framework.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ServicioAdicionalEvento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServicioAdicionalEvento()
        {
            this.ReservaEvento_Servicio = new HashSet<ReservaEvento_Servicio>();
        }
    
        public int ServicioAdicionalID { get; set; }
        public string NombreServicio { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioBase { get; set; }
        public Nullable<bool> RequierePersonalPago { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReservaEvento_Servicio> ReservaEvento_Servicio { get; set; }
    }
}
