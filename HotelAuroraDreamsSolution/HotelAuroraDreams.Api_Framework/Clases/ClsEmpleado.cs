using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelAuroraDreams.Api_Framework.Clases
{
    public class ClsEmpleado
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationRoleManager _roleManager;
        private readonly HotelManagementSystemEntities _dbEdmx;

        public ClsEmpleado()
        {
            var context = HttpContext.Current.GetOwinContext();
            _userManager = context.GetUserManager<ApplicationUserManager>();
            _roleManager = context.Get<ApplicationRoleManager>();
            _dbEdmx = new HotelManagementSystemEntities();
        }

        public async Task<List<EmpleadoViewModel>> GetEmpleados()
        {
            var users = await _userManager.Users.ToListAsync();
            var empleados = new List<EmpleadoViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user.Id);
                var hotel = user.HotelID.HasValue ? await _dbEdmx.Hotels.FindAsync(user.HotelID.Value) : null;
                var cargo = user.CargoID.HasValue ? await _dbEdmx.Cargoes.FindAsync(user.CargoID.Value) : null;

                empleados.Add(new EmpleadoViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    HotelID = user.HotelID,
                    NombreHotel = hotel?.nombre,
                    CargoID = user.CargoID,
                    NombreCargo = cargo?.nombre_cargo,
                    TipoDocumento = user.TipoDocumento,
                    NumeroDocumento = user.NumeroDocumento,
                    Direccion = user.Direccion,
                    FechaNacimiento = user.FechaNacimiento,
                    FechaContratacion = user.FechaContratacion,
                    Salario = user.Salario,
                    Estado = user.Estado,
                    Roles = roles
                });
            }

            return empleados.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).ToList();
        }

        public async Task<EmpleadoViewModel> GetEmpleado(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user.Id);
            var hotel = user.HotelID.HasValue ? await _dbEdmx.Hotels.FindAsync(user.HotelID.Value) : null;
            var cargo = user.CargoID.HasValue ? await _dbEdmx.Cargoes.FindAsync(user.CargoID.Value) : null;

            return new EmpleadoViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                HotelID = user.HotelID,
                NombreHotel = hotel?.nombre,
                CargoID = user.CargoID,
                NombreCargo = cargo?.nombre_cargo,
                TipoDocumento = user.TipoDocumento,
                NumeroDocumento = user.NumeroDocumento,
                Direccion = user.Direccion,
                FechaNacimiento = user.FechaNacimiento,
                FechaContratacion = user.FechaContratacion,
                Salario = user.Salario,
                Estado = user.Estado,
                Roles = roles
            };
        }

        public async Task<IdentityResult> UpdateEmpleado(string id, EmpleadoUpdateBindingModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return IdentityResult.Failed("Empleado no encontrado.");

            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.HotelID = model.HotelID;
            user.CargoID = model.CargoID;
            user.TipoDocumento = model.TipoDocumento;
            user.NumeroDocumento = model.NumeroDocumento;
            user.PhoneNumber = model.PhoneNumber;
            user.Direccion = model.Direccion;
            user.FechaNacimiento = model.FechaNacimiento;
            user.FechaContratacion = model.FechaContratacion;
            user.Salario = model.Salario;
            user.Estado = model.Estado;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return result;

            var currentRoles = await _userManager.GetRolesAsync(user.Id);
            var rolesToRemove = currentRoles.Except(model.Roles ?? new List<string>()).ToArray();
            if (rolesToRemove.Any())
            {
                result = await _userManager.RemoveFromRolesAsync(user.Id, rolesToRemove);
                if (!result.Succeeded) return result;
            }

            var rolesToAdd = (model.Roles ?? new List<string>()).Except(currentRoles).ToArray();
            foreach (var role in rolesToAdd)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return IdentityResult.Failed($"El rol '{role}' no existe.");
                }
            }
            if (rolesToAdd.Any())
            {
                result = await _userManager.AddToRolesAsync(user.Id, rolesToAdd);
            }

            return result;
        }

        public async Task<IdentityResult> DeleteEmpleado(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return IdentityResult.Failed("Empleado no encontrado.");

            return await _userManager.DeleteAsync(user);
        }
    } 
}