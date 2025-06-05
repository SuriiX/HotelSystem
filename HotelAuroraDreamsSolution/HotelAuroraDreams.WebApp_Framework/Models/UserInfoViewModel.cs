
using System;
using System.Collections.Generic;

[Serializable]
public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int? HotelID { get; set; }
        public int? CargoID { get; set; }
        public IList<string> Roles { get; set; }
    }
