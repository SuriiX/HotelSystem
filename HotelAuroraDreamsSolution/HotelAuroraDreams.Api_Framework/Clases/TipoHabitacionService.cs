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
    public class TipoHabitacionService : IDisposable
    {
        private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<object> ObtenerTodos()
        {
            return await db.Tipo_Habitacion
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
        }

        public async Task<object> ObtenerPorId(int id)
        {
            return await db.Tipo_Habitacion
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
        }

        public async Task<(bool Exito, object Resultado, string Error)> Crear(TipoHabitacionBindingModel model)
        {
            if (await db.Tipo_Habitacion.AnyAsync(th => th.nombre.ToLower() == model.Nombre.ToLower()))
                return (false, null, "Ya existe un tipo de habitación con este nombre.");

            var nuevo = new Tipo_Habitacion
            {
                nombre = model.Nombre,
                descripcion = model.Descripcion,
                precio_base = model.PrecioBase,
                capacidad = model.Capacidad,
                comodidades = model.Comodidades
            };

            db.Tipo_Habitacion.Add(nuevo);
            await db.SaveChangesAsync();

            return (true, new
            {
                nuevo.tipo_habitacion_id,
                nuevo.nombre,
                nuevo.descripcion,
                nuevo.precio_base,
                nuevo.capacidad,
                nuevo.comodidades
            }, null);
        }

        public async Task<(bool Exito, string Error)> Actualizar(int id, TipoHabitacionBindingModel model)
        {
            var existente = await db.Tipo_Habitacion.FindAsync(id);
            if (existente == null)
                return (false, "No se encontró el tipo de habitación.");

            if (await db.Tipo_Habitacion.AnyAsync(th => th.nombre.ToLower() == model.Nombre.ToLower() && th.tipo_habitacion_id != id))
                return (false, "Ya existe otro tipo de habitación con este nombre.");

            existente.nombre = model.Nombre;
            existente.descripcion = model.Descripcion;
            existente.precio_base = model.PrecioBase;
            existente.capacidad = model.Capacidad;
            existente.comodidades = model.Comodidades;

            db.Entry(existente).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Exito, string Error)> Eliminar(int id)
        {
            var tipo = await db.Tipo_Habitacion.FindAsync(id);
            if (tipo == null)
                return (false, "No se encontró el tipo de habitación.");

            db.Tipo_Habitacion.Remove(tipo);
            await db.SaveChangesAsync();

            return (true, null);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}