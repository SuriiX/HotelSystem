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
    
    public partial class DetalleFacturaEvento
    {
        public int DetalleFacturaEventoID { get; set; }
        public int FacturaEventoID { get; set; }
        public string TipoConcepto { get; set; }
        public Nullable<int> ReferenciaConceptoID { get; set; }
        public string DescripcionConcepto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    
        public virtual FacturaEvento FacturaEvento { get; set; }
    }
}
