// File: ~/Controllers/RestaurantesController.cs
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Restaurantes")]

    [Authorize(Roles = "Empleado, Administrador")]
    public class RestaurantesController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();


        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetRestaurantes([FromUri] int? hotelId = null)
        {
            try
            {
                var query = db.Restaurantes.Include(r => r.Hotel).AsQueryable(); 

                if (hotelId.HasValue && hotelId.Value > 0)
                {
                    query = query.Where(r => r.hotel_id == hotelId.Value);
                }

                var restaurantes = await query
                    .OrderBy(r => r.Hotel.nombre)
                    .ThenBy(r => r.nombre)
                    .Select(r => new RestauranteListItemDto
                    {
                        RestauranteID = r.restaurante_id,
                        Nombre = r.nombre,
                        HotelID = r.hotel_id,
                        NombreHotel = r.Hotel.nombre 
                    })
                    .ToListAsync();
                return Ok(restaurantes);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener la lista de restaurantes: {ex.ToString()}"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}