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
    public class ClsServiciosAdicionalesEvento : IDisposable
    {
        private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<ServicioAdicionalEventoViewModel>> ObtenerTodosAsync()
        {
            return await db.ServicioAdicionalEventos
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioID = s.ServicioID,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    Precio = s.Precio
                })
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<ServicioAdicionalEventoViewModel> ObtenerPorIdAsync(int id)
        {
            return await db.ServicioAdicionalEventos
                .Where(s => s.ServicioID == id)
                .Select(s => new ServicioAdicionalEventoViewModel
                {
                    ServicioID = s.ServicioID,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    Precio = s.Precio
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Existe, string Mensaje)> ExisteNombreServicioAsync(string nombre, int? excluirId = null)
        {
            var existe = await db.ServicioAdicionalEventos
                .AnyAsync(s => s.Nombre.ToLower() == nombre.ToLower()
                            && (!excluirId.HasValue || s.ServicioID != excluirId.Value));

            return (existe, existe ? "Este servicio adicional ya está registrado." : null);
        }

        public async Task<ServicioAdicionalEventoViewModel> CrearAsync(ServicioAdicionalEventoBindingModel model)
        {
            var servicio = new ServicioAdicionalEvento
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio
            };

            db.ServicioAdicionalEventos.Add(servicio);
            await db.SaveChangesAsync();

            return new ServicioAdicionalEventoViewModel
            {
                ServicioID = servicio.ServicioID,
                Nombre = servicio.Nombre,
                Descripcion = servicio.Descripcion,
                Precio = servicio.Precio
            };
        }

        public async Task<bool> ActualizarAsync(int id, ServicioAdicionalEventoBindingModel model)
        {
            var servicio = await db.ServicioAdicionalEventos.FindAsync(id);
            if (servicio == null) return false;

            servicio.Nombre = model.Nombre;
            servicio.Descripcion = model.Descripcion;
            servicio.Precio = model.Precio;

            db.Entry(servicio).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var servicio = await db.ServicioAdicionalEventos.FindAsync(id);
            if (servicio == null) return false;

            db.ServicioAdicionalEventos.Remove(servicio);
            await db.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}