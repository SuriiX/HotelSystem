using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsHotel
    {
        private readonly HotelManagementSystemEntities _db = new HotelManagementSystemEntities();

        public async Task<List<HotelListItemDto>> ObtenerHotelesActivos()
        {
            return await _db.Hotels
                .Where(h => h.estado_operativo == "activo")
                .OrderBy(h => h.nombre)
                .Select(h => new HotelListItemDto
                {
                    HotelID = h.hotel_id,
                    Nombre = h.nombre
                })
                .ToListAsync();
        }

        public async Task<HotelListItemDto> ObtenerHotelPorId(int id)
        {
            return await _db.Hotels
                .Where(h => h.hotel_id == id)
                .Select(h => new HotelListItemDto
                {
                    HotelID = h.hotel_id,
                    Nombre = h.nombre
                })
                .FirstOrDefaultAsync();
        }
    }
}