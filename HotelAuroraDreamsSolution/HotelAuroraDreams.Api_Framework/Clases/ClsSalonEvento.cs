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
    public class ClsSalonEvento : IDisposable
    {
        private HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<IQueryable<SalonEventoViewModel>> GetSalonesEventoAsync(int? hotelId = null)
        {
            var query = db.SalonEventoes.AsQueryable();
            if (hotelId.HasValue)
            {
                query = query.Where(s => s.HotelID == hotelId.Value);
            }

            var salones = query
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
                .OrderBy(s => s.NombreHotel).ThenBy(s => s.Nombre);

            return await Task.FromResult(salones);
        }

        public async Task<SalonEventoViewModel> GetSalonEventoAsync(int id)
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

            return salonViewModel;
        }

        public async Task<SalonEventoViewModel> CreateSalonEventoAsync(SalonEventoBindingModel model)
        {
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

            return viewModel;
        }

        public async Task<bool> UpdateSalonEventoAsync(int id, SalonEventoBindingModel model)
        {
            var salonEntity = await db.SalonEventoes.FindAsync(id);
            if (salonEntity == null) return false;

            salonEntity.HotelID = model.HotelID;
            salonEntity.Nombre = model.Nombre;
            salonEntity.Descripcion = model.Descripcion;
            salonEntity.CapacidadMaxima = model.CapacidadMaxima;
            salonEntity.Ubicacion = model.Ubicacion;
            salonEntity.PrecioPorHora = model.PrecioPorHora;
            salonEntity.EstaActivo = model.EstaActivo;

            db.Entry(salonEntity).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSalonEventoAsync(int id)
        {
            var salon = await db.SalonEventoes.FindAsync(id);
            if (salon == null) return false;

            db.SalonEventoes.Remove(salon);
            await db.SaveChangesAsync();
            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}