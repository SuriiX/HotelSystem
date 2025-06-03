using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.MobileControls;
using System.Windows.Documents;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsTipoEvento : IDisposable
    {
        private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<Tip oEventoViewModel>> ObtenerTodosAsync()
        {
            return await db.TipoEventoes
                .Select(t => new TipoEventoViewModel
                {
                    TipoEventoID = t.TipoEventoID,
                    NombreTipo = t.NombreTipo,
                    Descripcion = t.Descripcion
                })
                .OrderBy(t => t.NombreTipo)
                .ToListAsync();
        }

        public async Task<TipoEventoViewModel> ObtenerPorIdAsync(int id)
        {
            return await db.TipoEventoes
                .Where(t => t.TipoEventoID == id)
                .Select(t => new TipoEventoViewModel
                {
                    TipoEventoID = t.TipoEventoID,
                    NombreTipo = t.NombreTipo,
                    Descripcion = t.Descripcion
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Existe, string Mensaje)> ExisteNombreTipoAsync(string nombre, int? excluirId = null)
        {
            var existe = await db.TipoEventoes
                .AnyAsync(t => t.NombreTipo.ToLower() == nombre.ToLower()
                            && (!excluirId.HasValue || t.TipoEventoID != excluirId.Value));

            return (existe, existe ? "Este nombre de tipo de evento ya está en uso." : null);
        }

        public async Task<TipoEventoViewModel> CrearAsync(TipoEventoBindingModel model)
        {
            var entidad = new TipoEvento
            {
                NombreTipo = model.NombreTipo,
                Descripcion = model.Descripcion
            };
            db.TipoEventoes.Add(entidad);
            await db.SaveChangesAsync();

            return new TipoEventoViewModel
            {
                TipoEventoID = entidad.TipoEventoID,
                NombreTipo = entidad.NombreTipo,
                Descripcion = entidad.Descripcion
            };
        }

        public async Task<bool> ActualizarAsync(int id, TipoEventoBindingModel model)
        {
            var entidad = await db.TipoEventoes.FindAsync(id);
            if (entidad == null) return false;

            entidad.NombreTipo = model.NombreTipo;
            entidad.Descripcion = model.Descripcion;

            db.Entry(entidad).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var entidad = await db.TipoEventoes.FindAsync(id);
            if (entidad == null) return false;

            db.TipoEventoes.Remove(entidad);
            await db.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}