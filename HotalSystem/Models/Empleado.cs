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
    
    public partial class Empleado
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Empleado()
        {
            this.CheckIns = new HashSet<CheckIn>();
            this.CheckOuts = new HashSet<CheckOut>();
            this.Consumo_Servicio = new HashSet<Consumo_Servicio>();
            this.Facturas = new HashSet<Factura>();
            this.Pedido_Suministro = new HashSet<Pedido_Suministro>();
            this.Reservas = new HashSet<Reserva>();
            this.Reserva_Restaurante = new HashSet<Reserva_Restaurante>();
        }
    
        public int empleado_id { get; set; }
        public int hotel_id { get; set; }
        public int cargo_id { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string tipo_documento { get; set; }
        public string numero_documento { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string direccion { get; set; }
        public Nullable<System.DateTime> fecha_nacimiento { get; set; }
        public System.DateTime fecha_contratacion { get; set; }
        public decimal salario { get; set; }
        public string estado { get; set; }

        [JsonIgnore]
        public virtual Cargo Cargo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<CheckIn> CheckIns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]

        public virtual ICollection<CheckOut> CheckOuts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Consumo_Servicio> Consumo_Servicio { get; set; }

        [JsonIgnore]
        public virtual Hotel Hotel { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Factura> Facturas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Pedido_Suministro> Pedido_Suministro { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Reserva> Reservas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Reserva_Restaurante> Reserva_Restaurante { get; set; }
    }
}
