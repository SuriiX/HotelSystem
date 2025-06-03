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
    public class ClsHabitacion
    {
        private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<HabitacionViewModel>> GetHabitaciones(int? hotelId, int? tipoHabitacionId, string estado)
        {
            var query = db.Habitacions.AsQueryable();

            if (hotelId.HasValue)
                query = query.Where(h => h.hotel_id == hotelId.Value);

            if (tipoHabitacionId.HasValue)
                query = query.Where(h => h.tipo_habitacion_id == tipoHabitacionId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(h => h.estado.ToLower() == estado.ToLower());

            return await query
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
        }

        public async Task<HabitacionViewModel> GetHabitacion(int id)
        {
            return await db.Habitacions
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
        }

        public async Task<(bool existe, string error)> ValidarExistenciaNumero(int hotelId, string numero, int? excluirId = null)
        {
            bool existe = await db.Habitacions.AnyAsync(h =>
                h.hotel_id == hotelId &&
                h.numero == numero &&
                (!excluirId.HasValue || h.habitacion_id != excluirId.Value));
            return (existe, existe ? $"La habitación número {numero} ya existe en el hotel especificado." : null);
        }

        public async Task<HabitacionViewModel> CrearHabitacion(HabitacionBindingModel model)
        {
            var habitacionEntity = new Habitacion
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
            await db.SaveChangesAsync();

            var hotel = await db.Hotels.FindAsync(habitacionEntity.hotel_id);
            var tipoHab = await db.Tipo_Habitacion.FindAsync(habitacionEntity.tipo_habitacion_id);

            return new HabitacionViewModel
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
        }

        public async Task<bool> ActualizarHabitacion(int id, HabitacionBindingModel model)
        {
            var habitacion = await db.Habitacions.FindAsync(id);
            if (habitacion == null) return false;

            habitacion.hotel_id = model.HotelID;
            habitacion.tipo_habitacion_id = model.TipoHabitacionID;
            habitacion.numero = model.Numero;
            habitacion.piso = model.Piso;
            habitacion.estado = string.IsNullOrWhiteSpace(model.Estado) ? habitacion.estado : model.Estado;
            habitacion.vista = model.Vista;
            habitacion.descripcion = model.Descripcion;

            db.Entry(habitacion).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarHabitacion(int id)
        {
            var habitacion = await db.Habitacions.FindAsync(id);
            if (habitacion == null) return false;

            db.Habitacions.Remove(habitacion);
            await db.SaveChangesAsync();
            return true;
        }
    }
}