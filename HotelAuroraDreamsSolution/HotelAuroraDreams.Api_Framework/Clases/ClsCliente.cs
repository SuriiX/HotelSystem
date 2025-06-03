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
	public class ClsCliente
	{

		private readonly HotelManagementSystemEntities db = new HotelManagementSystemEntities();

        public async Task<List<ClienteViewModel>> GetClientesAsync()
        {
            return await db.Clientes
                .Select(c => new ClienteViewModel
                {
                    ClienteID = c.cliente_id,
                    Nombre = c.nombre,
                    Apellido = c.apellido,
                    TipoDocumento = c.tipo_documento,
                    NumeroDocumento = c.numero_documento,
                    Email = c.email,
                    Telefono = c.telefono,
                    Direccion = c.direccion,
                    CiudadResidenciaID = c.ciudad_residencia_id,
                    NombreCiudadResidencia = c.Ciudad != null ? c.Ciudad.nombre_ciudad : null,
                    FechaNacimiento = c.fecha_nacimiento,
                    FechaRegistro = (DateTime)c.fecha_registro
                })
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<ClienteViewModel> GetClienteByIdAsync(int id)
        {
            return await db.Clientes
                .Where(c => c.cliente_id == id)
                .Select(c => new ClienteViewModel
                {
                    ClienteID = c.cliente_id,
                    Nombre = c.nombre,
                    Apellido = c.apellido,
                    TipoDocumento = c.tipo_documento,
                    NumeroDocumento = c.numero_documento,
                    Email = c.email,
                    Telefono = c.telefono,
                    Direccion = c.direccion,
                    CiudadResidenciaID = c.ciudad_residencia_id,
                    NombreCiudadResidencia = c.Ciudad != null ? c.Ciudad.nombre_ciudad : null,
                    FechaNacimiento = c.fecha_nacimiento,
                    FechaRegistro = (DateTime)c.fecha_registro
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteClienteAsync(string tipoDocumento, string numeroDocumento, int? excludeId = null)
        {
            return await db.Clientes.AnyAsync(c =>
                c.tipo_documento == tipoDocumento &&
                c.numero_documento == numeroDocumento &&
                (!excludeId.HasValue || c.cliente_id != excludeId.Value));
        }

        public async Task<ClienteViewModel> CrearClienteAsync(ClienteBindingModel model)
        {
            Cliente cliente = new Cliente
            {
                nombre = model.Nombre,
                apellido = model.Apellido,
                tipo_documento = model.TipoDocumento,
                numero_documento = model.NumeroDocumento,
                email = model.Email,
                telefono = model.Telefono,
                direccion = model.Direccion,
                ciudad_residencia_id = model.CiudadResidenciaID,
                fecha_nacimiento = model.FechaNacimiento,
                fecha_registro = DateTime.Now
            };

            db.Clientes.Add(cliente);
            await db.SaveChangesAsync();

            string nombreCiudad = null;
            if (cliente.ciudad_residencia_id.HasValue)
            {
                var ciudad = await db.Ciudads.FindAsync(cliente.ciudad_residencia_id.Value);
                nombreCiudad = ciudad?.nombre_ciudad;
            }

            return new ClienteViewModel
            {
                ClienteID = cliente.cliente_id,
                Nombre = cliente.nombre,
                Apellido = cliente.apellido,
                TipoDocumento = cliente.tipo_documento,
                NumeroDocumento = cliente.numero_documento,
                Email = cliente.email,
                Telefono = cliente.telefono,
                Direccion = cliente.direccion,
                CiudadResidenciaID = cliente.ciudad_residencia_id,
                NombreCiudadResidencia = nombreCiudad,
                FechaNacimiento = cliente.fecha_nacimiento,
                FechaRegistro = (DateTime)cliente.fecha_registro
            };
        }

        public async Task<bool> ActualizarClienteAsync(int id, ClienteBindingModel model)
        {
            var cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
                return false;

            cliente.nombre = model.Nombre;
            cliente.apellido = model.Apellido;
            cliente.tipo_documento = model.TipoDocumento;
            cliente.numero_documento = model.NumeroDocumento;
            cliente.email = model.Email;
            cliente.telefono = model.Telefono;
            cliente.direccion = model.Direccion;
            cliente.ciudad_residencia_id = model.CiudadResidenciaID;
            cliente.fecha_nacimiento = model.FechaNacimiento;

            db.Entry(cliente).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarClienteAsync(int id)
        {
            var cliente = await db.Clientes.FindAsync(id);
            if (cliente == null)
                return false;

            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync();
            return true;
        }


    }
}