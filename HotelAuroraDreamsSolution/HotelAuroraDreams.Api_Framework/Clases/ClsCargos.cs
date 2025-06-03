using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsCargos : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<CargoListItemDto>> ObtenerCargosAsync()
        {
            return await db.Cargoes
                .OrderBy(c => c.nombre_cargo)
                .Select(c => new CargoListItemDto
                {
                    CargoID = c.cargo_id,
                    NombreCargo = c.nombre_cargo
                })
                .ToListAsync();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}