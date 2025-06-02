// File: ~/Controllers/TiposHabitacionController.cs
using HotelAuroraDreams.Api_Framework.Models; // Donde está HotelManagementSystemEntities y Tipo_Habitacion
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/TiposHabitacion")]
    [Authorize(Roles = "Administrador")]
    public class TiposHabitacionController : ApiController
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetTiposHabitacion()
        {
            try
            {
                var tiposHabitacion = await db.Tipo_Habitacion
                                            .Select(th => new {
                                                th.tipo_habitacion_id,
                                                th.nombre,
                                                th.descripcion,
                                                th.precio_base,
                                                th.capacidad,
                                                th.comodidades
                                            })
                                            .OrderBy(th => th.nombre)
                                            .ToListAsync();
                return Ok(tiposHabitacion);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener tipos de habitación: {ex.Message}", ex.InnerException));
            }
        }
        [HttpGet]
        [Route("{id:int}", Name = "GetTipoHabitacionById")]
        public async Task<IHttpActionResult> GetTipoHabitacion(int id)
        {
            try
            {
                var tipoHabitacion = await db.Tipo_Habitacion
                                            .Where(th => th.tipo_habitacion_id == id)
                                            .Select(th => new {
                                                th.tipo_habitacion_id,
                                                th.nombre,
                                                th.descripcion,
                                                th.precio_base,
                                                th.capacidad,
                                                th.comodidades
                                            })
                                            .FirstOrDefaultAsync();
                if (tipoHabitacion == null)
                {
                    return NotFound();
                }
                return Ok(tipoHabitacion);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener tipo de habitación ID {id}: {ex.Message}", ex.InnerException));
            }
        }
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(Tipo_Habitacion))]
        public async Task<IHttpActionResult> PostTipoHabitacion(TipoHabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await db.Tipo_Habitacion.AnyAsync(th => th.nombre.ToLower() == model.Nombre.ToLower()))
            {
                ModelState.AddModelError("Nombre", "Ya existe un tipo de habitación con este nombre.");
                return BadRequest(ModelState);
            }

            Tipo_Habitacion tipoHabitacion = new Tipo_Habitacion
            {
                nombre = model.Nombre,
                descripcion = model.Descripcion,
                precio_base = model.PrecioBase,
                capacidad = model.Capacidad,
                comodidades = model.Comodidades
            };

            db.Tipo_Habitacion.Add(tipoHabitacion);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al crear tipo de habitación: {ex.Message}", ex.InnerException));
            }
            var dto = new
            {
                tipoHabitacion.tipo_habitacion_id,
                tipoHabitacion.nombre,
                tipoHabitacion.descripcion,
                tipoHabitacion.precio_base,
                tipoHabitacion.capacidad,
                tipoHabitacion.comodidades
            };
            return CreatedAtRoute("GetTipoHabitacionById", new { id = tipoHabitacion.tipo_habitacion_id }, dto);
        }
        [HttpPut]
        [Route("{id:int}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTipoHabitacion(int id, TipoHabitacionBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipoHabitacion = await db.Tipo_Habitacion.FindAsync(id);
            if (tipoHabitacion == null)
            {
                return NotFound();
            }

            if (await db.Tipo_Habitacion.AnyAsync(th => th.nombre.ToLower() == model.Nombre.ToLower() && th.tipo_habitacion_id != id))
            {
                ModelState.AddModelError("Nombre", "Ya existe otro tipo de habitación con este nombre.");
                return BadRequest(ModelState);
            }

            tipoHabitacion.nombre = model.Nombre;
            tipoHabitacion.descripcion = model.Descripcion;
            tipoHabitacion.precio_base = model.PrecioBase;
            tipoHabitacion.capacidad = model.Capacidad;
            tipoHabitacion.comodidades = model.Comodidades;

            db.Entry(tipoHabitacion).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return InternalServerError(new Exception($"Error de concurrencia al actualizar: {ex.Message}", ex.InnerException));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al actualizar tipo de habitación: {ex.Message}", ex.InnerException));
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> DeleteTipoHabitacion(int id)
        {
            Tipo_Habitacion tipoHabitacion = await db.Tipo_Habitacion.FindAsync(id);
            if (tipoHabitacion == null)
            {
                return NotFound();
            }
            db.Tipo_Habitacion.Remove(tipoHabitacion);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al eliminar tipo de habitación: {ex.Message}", ex.InnerException));
            }

            return Ok(new { Message = "Tipo de habitación eliminado exitosamente.", Id = id });
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