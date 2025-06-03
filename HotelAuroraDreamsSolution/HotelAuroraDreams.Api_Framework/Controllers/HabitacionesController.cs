using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Habitaciones")]
    [Authorize(Roles = "Administrador")]
    public class HabitacionesController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetHabitaciones([FromUri] int? hotelId = null, [FromUri] int? tipoHabitacionId = null, [FromUri] string estado = null)
        {
            try
            {
                var query = db.Habitacions.AsQueryable();

                if (hotelId.HasValue)
                {
                    query = query.Where(h => h.hotel_id == hotelId.Value);
                }
                if (tipoHabitacionId.HasValue)
                {
                    query = query.Where(h => h.tipo_habitacion_id == tipoHabitacionId.Value);
                }
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    query = query.Where(h => h.estado.ToLower() == estado.ToLower());
                }

                var habitaciones = await query
                    .Select(h => new HabitacionViewModel
                    {
                        HabitacionID = h.habitacion_id,
                        HotelID = h.hotel_id,
                        NombreHotel = h.Hotel.nombre,
                        TipoHabitacionID = h.tipo_habitacion_id,
                        NombreTipoHabitacion = h.Tipo_Habitacion.nombre,
                        Numero = h.numero,
                        Piso = h.piso,
                        Estado = h.estado,
                        Vista = h.vista,
                        Descripcion = h.descripcion
                    })
                    .OrderBy(h => h.HotelID).ThenBy(h => h.Piso).ThenBy(h => h.Numero)
                    .ToListAsync();
                return Ok(habitaciones);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener habitaciones: {ex.Message}", ex.InnerException));
            }
        }

        [HttpGet]
        [Route("{id:int}", Name = "GetHabitacionById")]
        public async Task<IHttpActionResult> GetHabitacion(int id)
        {
            try
            {
                var habitacionViewModel = await db.Habitacions
                    .Where(h => h.habitacion_id == id)
                    .Select(h => new HabitacionViewModel
                    {
                        HabitacionID = h.habitacion_id,
                        HotelID = h.hotel_id,
                        NombreHotel = h.Hotel.nombre,
                        TipoHabitacionID = h.tipo_habitacion_id,
                        NombreTipoHabitacion = h.Tipo_Habitacion.nombre,
                        Numero = h.numero,
                        Piso = h.piso,
                        Estado = h.estado,
                        Vista = h.vista,
                        Descripcion = h.descripcion
                    })
                    .FirstOrDefaultAsync();

                if (habitacionViewModel == null)
                {
                    return NotFound();
                }
                return Ok(habitacionViewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener habitación ID {id}: {ex.Message}", ex.InnerException));
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> PostHabitacion(HabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await db.Habitacions.AnyAsync(h => h.hotel_id == model.HotelID && h.numero == model.Numero))
            {
                ModelState.AddModelError("Numero", $"La habitación número {model.Numero} ya existe en el hotel especificado.");
                return BadRequest(ModelState);
            }

            Habitacion habitacionEntity = new Habitacion
            {
                hotel_id = model.HotelID,
                tipo_habitacion_id = model.TipoHabitacionID,
                numero = model.Numero,
                piso = model.Piso,
                estado = string.IsNullOrWhiteSpace(model.Estado) ? "disponible" : model.Estado,
                vista = model.Vista,
                descripcion = model.Descripcion
            };

            db.Habitacions.Add(habitacionEntity);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al crear habitación: {ex.Message}", ex.InnerException));
            }

            // Cargar nombres para el ViewModel de respuesta
            var hotel = await db.Hotels.FindAsync(habitacionEntity.hotel_id);
            var tipoHab = await db.Tipo_Habitacion.FindAsync(habitacionEntity.tipo_habitacion_id);

            var viewModel = new HabitacionViewModel
            {
                HabitacionID = habitacionEntity.habitacion_id,
                HotelID = habitacionEntity.hotel_id,
                NombreHotel = hotel?.nombre,
                TipoHabitacionID = habitacionEntity.tipo_habitacion_id,
                NombreTipoHabitacion = tipoHab?.nombre,
                Numero = habitacionEntity.numero,
                Piso = habitacionEntity.piso,
                Estado = habitacionEntity.estado,
                Vista = habitacionEntity.vista,
                Descripcion = habitacionEntity.descripcion
            };

            return CreatedAtRoute("GetHabitacionById", new { id = habitacionEntity.habitacion_id }, viewModel);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutHabitacion(int id, HabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var habitacionEntity = await db.Habitacions.FindAsync(id);
            if (habitacionEntity == null)
            {
                return NotFound();
            }

            if (await db.Habitacions.AnyAsync(h => h.hotel_id == model.HotelID && h.numero == model.Numero && h.habitacion_id != id))
            {
                ModelState.AddModelError("Numero", $"La habitación número {model.Numero} ya existe en el hotel especificado.");
                return BadRequest(ModelState);
            }

            habitacionEntity.hotel_id = model.HotelID;
            habitacionEntity.tipo_habitacion_id = model.TipoHabitacionID;
            habitacionEntity.numero = model.Numero;
            habitacionEntity.piso = model.Piso;
            habitacionEntity.estado = string.IsNullOrWhiteSpace(model.Estado) ? habitacionEntity.estado : model.Estado;
            habitacionEntity.vista = model.Vista;
            habitacionEntity.descripcion = model.Descripcion;

            db.Entry(habitacionEntity).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(new Exception($"Error de concurrencia al actualizar habitación: {ex.Message}", ex.InnerException));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar habitación: {ex.Message}", ex.InnerException));
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteHabitacion(int id)
        {
            Habitacion habitacion = await db.Habitacions.FindAsync(id);
            if (habitacion == null)
            {
                return NotFound();
            }

            db.Habitacions.Remove(habitacion);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al eliminar habitación: {ex.Message}", ex.InnerException));
            }

            return Ok(new { Message = "Habitación eliminada exitosamente.", Id = id });
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
