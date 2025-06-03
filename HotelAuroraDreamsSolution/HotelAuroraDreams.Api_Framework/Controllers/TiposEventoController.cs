using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/TiposEvento")]
    [Authorize(Roles = "Administrador")]
    public class TiposEventoController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetTiposEvento()
        {
            var tipos = await db.TipoEventoes
                .Select(t => new TipoEventoViewModel
                {
                    TipoEventoID = t.TipoEventoID,
                    NombreTipo = t.NombreTipo,
                    Descripcion = t.Descripcion
                })
                .OrderBy(t => t.NombreTipo)
                .ToListAsync();
            return Ok(tipos);
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetTipoEventoById")]
        public async Task<IHttpActionResult> GetTipoEvento(int id)
        {
            var tipoViewModel = await db.TipoEventoes.Where(t => t.TipoEventoID == id)
                .Select(t => new TipoEventoViewModel
                {
                    TipoEventoID = t.TipoEventoID,
                    NombreTipo = t.NombreTipo,
                    Descripcion = t.Descripcion
                })
                .FirstOrDefaultAsync();
            if (tipoViewModel == null) return NotFound();
            return Ok(tipoViewModel);
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostTipoEvento(TipoEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await db.TipoEventoes.AnyAsync(t => t.NombreTipo.ToLower() == model.NombreTipo.ToLower()))


            {
                ModelState.AddModelError("NombreTipo", "Este tipo de evento ya existe.");
                return BadRequest(ModelState);
            }
            TipoEvento tipoEntity = new TipoEvento
            {
                NombreTipo = model.NombreTipo,
                Descripcion = model.Descripcion
            };
            db.TipoEventoes.Add(tipoEntity);
            await db.SaveChangesAsync();
            var viewModel = new TipoEventoViewModel
            {
                TipoEventoID = tipoEntity.TipoEventoID,
                NombreTipo = tipoEntity.NombreTipo,
                Descripcion = tipoEntity.Descripcion
            };
            return CreatedAtRoute("GetTipoEventoById", new { id = tipoEntity.TipoEventoID }, viewModel);
        }

        [HttpPut]
        public async Task<IHttpActionResult> PutTipoEvento(int id, TipoEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var tipoEntity = await db.TipoEventoes.FindAsync(id);
            if (tipoEntity == null) return NotFound();
            if (await db.TipoEventoes.AnyAsync(t => t.NombreTipo.ToLower() == model.NombreTipo.ToLower() && t.TipoEventoID != id))
            {
                ModelState.AddModelError("NombreTipo", "Este nombre de tipo de evento ya está en uso.");
                return BadRequest(ModelState);
            }
            tipoEntity.NombreTipo = model.NombreTipo;
            tipoEntity.Descripcion = model.Descripcion;
            db.Entry(tipoEntity).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteTipoEvento(int id)
        {
            var tipo = await db.TipoEventoes.FindAsync(id);
            if (tipo == null) return NotFound();
            db.TipoEventoes.Remove(tipo);
            await db.SaveChangesAsync();
            return Ok(new { Message = "Tipo de evento eliminado.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();



            base.Dispose(disposing);
        }
    }
}