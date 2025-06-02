// File: ~/Controllers/SalonesEventoController.cs
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
    [RoutePrefix("api/SalonesEvento")]
    [Authorize(Roles = "Administrador")]
    public class SalonesEventoController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetSalonesEvento([FromUri] int? hotelId = null)
        {
            try
            {
                var query = db.SalonEventoes.AsQueryable();
                if (hotelId.HasValue)
                {
                    query = query.Where(s => s.HotelID == hotelId.Value);
                }

                var salones = await query
                    .Select(s => new SalonEventoViewModel
                    {
                        SalonEventoID = s.SalonEventoID,
                        HotelID = s.HotelID,
                        NombreHotel = s.Hotel.nombre,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        CapacidadMaxima = s.CapacidadMaxima,
                        Ubicacion = s.Ubicacion,
                        PrecioPorHora = s.PrecioPorHora,
                        EstaActivo = (bool)s.EstaActivo
                    })
                    .OrderBy(s => s.NombreHotel).ThenBy(s => s.Nombre)
                    .ToListAsync();
                return Ok(salones);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener salones: {ex.Message}", ex.InnerException));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetSalonEventoById")]
        public async Task<IHttpActionResult> GetSalonEvento(int id)
        {
            var salonViewModel = await db.SalonEventoes
                .Where(s => s.SalonEventoID == id)
                .Select(s => new SalonEventoViewModel
                {
                    SalonEventoID = s.SalonEventoID,
                    HotelID = s.HotelID,
                    NombreHotel = s.Hotel.nombre,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    CapacidadMaxima = s.CapacidadMaxima,
                    Ubicacion = s.Ubicacion,
                    PrecioPorHora = s.PrecioPorHora,
                    EstaActivo = (bool)s.EstaActivo
                })
                .FirstOrDefaultAsync();
            if (salonViewModel == null) return NotFound();
            return Ok(salonViewModel);
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(SalonEventoViewModel))]
        public async Task<IHttpActionResult> PostSalonEvento(SalonEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            SalonEvento salonEntity = new SalonEvento
            {
                HotelID = model.HotelID,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                CapacidadMaxima = model.CapacidadMaxima,
                Ubicacion = model.Ubicacion,
                PrecioPorHora = model.PrecioPorHora,
                EstaActivo = model.EstaActivo
            };
            db.SalonEventoes.Add(salonEntity);
            await db.SaveChangesAsync();

            var hotel = await db.Hotels.FindAsync(salonEntity.HotelID);
            var viewModel = new SalonEventoViewModel
            {
                SalonEventoID = salonEntity.SalonEventoID,
                HotelID = salonEntity.HotelID,
                NombreHotel = hotel?.nombre,
                Nombre = salonEntity.Nombre,
                Descripcion = salonEntity.Descripcion,
                CapacidadMaxima = salonEntity.CapacidadMaxima,
                Ubicacion = salonEntity.Ubicacion,
                PrecioPorHora = salonEntity.PrecioPorHora,
                EstaActivo = (bool)salonEntity.EstaActivo
            };
            return CreatedAtRoute("GetSalonEventoById", new { id = salonEntity.SalonEventoID }, viewModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSalonEvento(int id, SalonEventoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var salonEntity = await db.SalonEventoes.FindAsync(id);
            if (salonEntity == null) return NotFound();

            salonEntity.HotelID = model.HotelID;
            salonEntity.Nombre = model.Nombre;
            salonEntity.Descripcion = model.Descripcion;
            salonEntity.CapacidadMaxima = model.CapacidadMaxima;
            salonEntity.Ubicacion = model.Ubicacion;
            salonEntity.PrecioPorHora = model.PrecioPorHora;
            salonEntity.EstaActivo = model.EstaActivo;

            db.Entry(salonEntity).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteSalonEvento(int id)
        {
            var salon = await db.SalonEventoes.FindAsync(id);
            if (salon == null) return NotFound();

            db.SalonEventoes.Remove(salon);
            await db.SaveChangesAsync();
            return Ok(new { Message = "Salón de evento eliminado.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}