using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsServiciosAdicionalesEvento : IDisposable
    {
        private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<ServicioAdicionalEventoViewModel>> ObtenerTodosAsync()
        {
            var query = db.ServicioAdicionalEventoes
                .OrderBy(s => s.NombreServicio)
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioAdicionalID = s.ServicioAdicionalID,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion,
                    PrecioBase = s.PrecioBase
                });
            return await query.ToListAsync();
        }

        public async Task<ServicioAdicionalEventoViewModel> ObtenerPorIdAsync(int id)
        {
            var query = db.ServicioAdicionalEventoes
                .Where(s => s.ServicioAdicionalID == id)
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioAdicionalID = s.ServicioAdicionalID,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion,
                    PrecioBase = s.PrecioBase
                });
            return await query.FirstOrDefaultAsync();
        }

        public async Task<(bool Existe, string Mensaje)> ExisteNombreServicioAsync(string nombre, int? excluirId = null)
        {
            var query = db.ServicioAdicionalEventoes.AsQueryable();
            var existe = await query.AnyAsync(s => s.NombreServicio.ToLower() == nombre.ToLower()
                            && (!excluirId.HasValue || s.ServicioAdicionalID != excluirId.Value));
            return (existe, existe ? "Este servicio adicional ya está registrado." : null);
        }

        public async Task<ServicioAdicionalEventoViewModel> CrearAsync(ServicioAdicionalEventoBindingModel model)
        {
            var servicio = new ServicioAdicionalEvento
            {
                NombreServicio = model.NombreServicio,
                Descripcion = model.Descripcion,
                PrecioBase = model.PrecioBase
            };

            db.Entry(servicio).State = EntityState.Added;
            await db.SaveChangesAsync();

            return new ServicioAdicionalEventoViewModel
            {
                ServicioAdicionalID = servicio.ServicioAdicionalID,
                NombreServicio = servicio.NombreServicio,
                Descripcion = servicio.Descripcion,
                PrecioBase = servicio.PrecioBase
            };
        }

        public async Task<bool> ActualizarAsync(int id, ServicioAdicionalEventoBindingModel model)
        {
            var servicio = await db.ServicioAdicionalEventoes
                .FirstOrDefaultAsync(s => s.ServicioAdicionalID == id);
            if (servicio == null) return false;

            servicio.NombreServicio = model.NombreServicio;
            servicio.Descripcion = model.Descripcion;
            servicio.PrecioBase = model.PrecioBase;

            db.Entry(servicio).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var servicio = await db.ServicioAdicionalEventoes
                .FirstOrDefaultAsync(s => s.ServicioAdicionalID == id);
            if (servicio == null) return false;

            db.Entry(servicio).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
