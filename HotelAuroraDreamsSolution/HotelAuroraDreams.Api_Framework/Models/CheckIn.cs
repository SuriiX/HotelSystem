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
    
    public partial class CheckIn
    {
        public int checkin_id { get; set; }
        public int reserva_id { get; set; }
        public int empleado_id { get; set; }
        public Nullable<System.DateTime> fecha_hora { get; set; }
        public string metodo_pago_adelanto { get; set; }
        public Nullable<bool> documentos_verificados { get; set; }
        public string observaciones { get; set; }
    
        public virtual Empleado Empleado { get; set; }
        public virtual Reserva Reserva { get; set; }
    }
}
