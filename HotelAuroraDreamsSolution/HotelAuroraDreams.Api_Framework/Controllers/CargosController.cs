using HotelAuroraDreams.Api_Framework.Models;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Data.Entity;
using System.Linq;
using HotelAuroraDreams.Api_Framework.Models.DTO;


namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Cargos")]

    [Authorize(Roles = "Empleado, Administrador")]
    public class CargosController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        // GET: api/Cargos
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetCargos()
        {
            try
            {
                var cargos = await db.Cargoes // Asegúrate que tu DbSet se llame Cargo
                    .OrderBy(c => c.nombre_cargo)
                    .Select(c => new CargoListItemDto
                    {
                        CargoID = c.cargo_id,
                        NombreCargo = c.nombre_cargo
                    })
                    .ToListAsync();
                return Ok(cargos);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener la lista de cargos: {ex.Message}", ex.InnerException));
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