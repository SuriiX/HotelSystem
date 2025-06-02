using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/ServiciosAdicionalesEvento")]
    [Authorize(Roles = "Administrador")]
    public class ServiciosAdicionalesEventoController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetServiciosAdicionalesEvento()
        {
            var servicios = await db.ServicioAdicionalEventoes
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioAdicionalID = s.ServicioAdicionalID,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion,
                    PrecioBase = s.PrecioBase,
                    RequierePersonalPago = (bool)s.RequierePersonalPago
                })
                .OrderBy(s => s.NombreServicio)
                .ToListAsync();
            return Ok(servicios);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetServicioAdicionalEventoById")]
        public async Task<IHttpActionResult> GetServicioAdicionalEvento(int id)
        {
            var servicioViewModel = await db.ServicioAdicionalEventoes
                .Where(s => s.ServicioAdicionalID == id)
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioAdicionalID = s.ServicioAdicionalID,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion,
                    PrecioBase = s.PrecioBase,
                    RequierePersonalPago = (bool)s.RequierePersonalPago
                })
                .FirstOrDefaultAsync();
            if (servicioViewModel == null) return NotFound();
            return Ok(servicioViewModel);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(ServicioAdicionalEventoViewModel))]
        public async Task<IHttpActionResult> PostServicioAdicionalEvento(ServicioAdicionalEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await db.ServicioAdicionalEventoes.AnyAsync(s => s.NombreServicio.ToLower() == model.NombreServicio.ToLower()))
            {
                ModelState.AddModelError("NombreServicio", "Este servicio adicional ya existe.");
                return BadRequest(ModelState);
            }
            ServicioAdicionalEvento servicioEntity = new ServicioAdicionalEvento
            {
                NombreServicio = model.NombreServicio,
                Descripcion = model.Descripcion,
                PrecioBase = model.PrecioBase,
                RequierePersonalPago = model.RequierePersonalPago
            };
            db.ServicioAdicionalEventoes.Add(servicioEntity);
            await db.SaveChangesAsync();
            var viewModel = new ServicioAdicionalEventoViewModel
            {
                ServicioAdicionalID = servicioEntity.ServicioAdicionalID,
                NombreServicio = servicioEntity.NombreServicio,
                Descripcion = servicioEntity.Descripcion,
                PrecioBase = servicioEntity.PrecioBase,
                RequierePersonalPago = (bool)servicioEntity.RequierePersonalPago
            };
            return CreatedAtRoute("GetServicioAdicionalEventoById", new { id = servicioEntity.ServicioAdicionalID }, viewModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutServicioAdicionalEvento(int id, ServicioAdicionalEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var servicioEntity = await db.ServicioAdicionalEventoes.FindAsync(id);
            if (servicioEntity == null) return NotFound();

            if (await db.ServicioAdicionalEventoes.AnyAsync(s => s.NombreServicio.ToLower() == model.NombreServicio.ToLower() && s.ServicioAdicionalID != id))
            {
                ModelState.AddModelError("NombreServicio", "Este nombre de servicio ya está en uso.");
                return BadRequest(ModelState);
            }

            servicioEntity.NombreServicio = model.NombreServicio;
            servicioEntity.Descripcion = model.Descripcion;
            servicioEntity.PrecioBase = model.PrecioBase;
            servicioEntity.RequierePersonalPago = model.RequierePersonalPago;

            db.Entry(servicioEntity).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteServicioAdicionalEvento(int id)
        {
            var servicio = await db.ServicioAdicionalEventoes.FindAsync(id);
            if (servicio == null) return NotFound();
             if (await db.ReservaEvento_Servicio.AnyAsync(rs => rs.ServicioAdicionalID == id))
             {
                return Content(HttpStatusCode.Conflict, "No se puede eliminar el servicio, está en uso en reservas de evento.");
             }
            db.ServicioAdicionalEventoes.Remove(servicio);
            await db.SaveChangesAsync();
            return Ok(new { Message = "Servicio adicional de evento eliminado.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}