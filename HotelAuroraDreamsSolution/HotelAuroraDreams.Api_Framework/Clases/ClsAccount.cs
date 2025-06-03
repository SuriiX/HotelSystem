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
    public class ClsAccount : IDisposable
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ClsAccount() { }

        public ClsAccount(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public async Task<(bool Exito, IdentityResult Resultado, string Mensaje)> RegistrarUsuario(RegisterBindingModel model)
        {
            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                FechaContratacion = DateTime.UtcNow,
                Estado = "activo",
                Salario = model.Salario,
                HotelID = model.HotelID,
                CargoID = model.CargoID
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            return (result.Succeeded, result, result.Succeeded ? "User registered successfully." : null);
        }

        public async Task<(bool Exito, IdentityResult Resultado, string Mensaje)> SetupRoles()
        {
            string[] roles = { "Administrador", "Empleado" };
            foreach (var role in roles)
            {
                if (!await RoleManager.RoleExistsAsync(role))
                {
                    var result = await RoleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        return (false, result, $"Error al crear el rol '{role}'.");
                }
            }
            return (true, IdentityResult.Success, "Roles configurados/verificados.");
        }

        public async Task<(bool Exito, UserInfoViewModel DatosUsuario, string Error)> ObtenerInfoUsuario(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, null, "User ID not found in token.");

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
                return (false, null, "Usuario no encontrado.");

            var roles = await UserManager.GetRolesAsync(userId);

            var userInfo = new UserInfoViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                HotelID = user.HotelID,
                CargoID = user.CargoID,
                Roles = roles
            };

            return (true, userInfo, null);
        }

        public async Task<(bool Exito, IdentityResult Resultado, string Mensaje)> AsignarRol(string userEmail, string roleName)
        {
            var user = await UserManager.FindByEmailAsync(userEmail);
            if (user == null)
                return (false, null, $"Usuario con email '{userEmail}' no encontrado.");

            if (!await RoleManager.RoleExistsAsync(roleName))
                return (false, null, $"Rol '{roleName}' no encontrado. Ejecute SetupRoles primero.");

            if (await UserManager.IsInRoleAsync(user.Id, roleName))
                return (true, IdentityResult.Success, $"El usuario '{userEmail}' ya pertenece al rol '{roleName}'.");

            var result = await UserManager.AddToRoleAsync(user.Id, roleName);
            return (result.Succeeded, result, result.Succeeded ? $"Usuario '{userEmail}' añadido exitosamente al rol '{roleName}'." : null);
        }

        public void Dispose()
        {
            _userManager?.Dispose();
            _roleManager?.Dispose();
        }
    }
}