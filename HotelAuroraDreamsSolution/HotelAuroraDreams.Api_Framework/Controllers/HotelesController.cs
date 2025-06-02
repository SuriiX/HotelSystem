// File: ~/Controllers/HotelesController.cs
using HotelAuroraDreams.Api_Framework.Models; // Para HotelManagementSystemEntities y Hotel
using HotelAuroraDreams.Api_Framework.Models.DTO; // Para HotelListItemDto
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description; // Para ResponseType si añades más métodos

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Hoteles")]
    [Authorize(Roles = "Empleado, Administrador")]
    public class HotelesController : ApiController
    {
        private readonly ClsHotel _clsHotel = new ClsHotel();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetHoteles()
        {
            try
            {
                var hoteles = await _clsHotel.ObtenerHotelesActivos();
                return Ok(hoteles);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener la lista de hoteles: {ex.Message}", ex.InnerException));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetHotelById_HotelesController")]
        public async Task<IHttpActionResult> GetHotel(int id)
        {
            try
            {
                var hotel = await _clsHotel.ObtenerHotelPorId(id);
                if (hotel == null)
                    return NotFound();

                return Ok(hotel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener el hotel con ID {id}: {ex.Message}", ex.InnerException));
            }
        }
    }
}