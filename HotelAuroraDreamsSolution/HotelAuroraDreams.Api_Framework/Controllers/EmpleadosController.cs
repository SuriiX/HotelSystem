// File: ~/Controllers/EmpleadosController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity; // Para ToListAsync en el contexto EDMX
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Empleados")]
    [Authorize(Roles = "Administrador")]
    public class EmpleadosController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private HotelManagementSystemEntities _dbEdmx = new HotelManagementSystemEntities();

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ApplicationRoleManager RoleManager
        {
            get => _roleManager ?? Request.GetOwinContext().Get<ApplicationRoleManager>();
            private set => _roleManager = value;
        }

        public EmpleadosController() { }

        public EmpleadosController(ApplicationUserManager userManager, ApplicationRoleManager roleManager, HotelManagementSystemEntities dbEdmx)
        {
            UserManager = userManager;
            RoleManager = roleManager;
            _dbEdmx = dbEdmx;
        }

        // GET: api/Empleados
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetEmpleados()
        {
            try
            {
                var users = await UserManager.Users.ToListAsync();
                var empleadoViewModels = new List<EmpleadoViewModel>();

                foreach (var user in users)
                {
                    var roles = await UserManager.GetRolesAsync(user.Id);
                    var hotel = user.HotelID.HasValue ? await _dbEdmx.Hotels.FindAsync(user.HotelID.Value) : null;
                    var cargo = user.CargoID.HasValue ? await _dbEdmx.Cargoes.FindAsync(user.CargoID.Value) : null;

                    empleadoViewModels.Add(new EmpleadoViewModel
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
                return Ok(empleadoViewModels.OrderBy(e => e.Apellido).ThenBy(e => e.Nombre));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener empleados: {ex.Message}", ex.InnerException));
            }
        }

        // GET: api/Empleados/{id} (id es el string GUID de IdentityUser)
        [HttpGet]
        [Route("{id}", Name = "GetEmpleadoById")]
        public async Task<IHttpActionResult> GetEmpleado(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID de empleado no puede ser vacío.");

            try
            {
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await UserManager.GetRolesAsync(user.Id);
                var hotel = user.HotelID.HasValue ? await _dbEdmx.Hotels.FindAsync(user.HotelID.Value) : null;
                var cargo = user.CargoID.HasValue ? await _dbEdmx.Cargoes.FindAsync(user.CargoID.Value) : null;

                var viewModel = new EmpleadoViewModel
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
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al obtener empleado ID {id}: {ex.Message}", ex.InnerException));
            }
        }

        // PUT: api/Empleados/{id} (Actualizar perfil y roles)
        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEmpleado(string id, EmpleadoUpdateBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("ID de empleado no puede ser vacío.");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

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


            IdentityResult result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            // Actualizar Roles
            if (model.Roles != null)
            {
                var currentRoles = await UserManager.GetRolesAsync(user.Id);

                var rolesToRemove = currentRoles.Except(model.Roles).ToArray();
                if (rolesToRemove.Any())
                {
                    result = await UserManager.RemoveFromRolesAsync(user.Id, rolesToRemove);
                    if (!result.Succeeded) return GetErrorResult(result);
                }

                // Roles a añadir: los que están en model.Roles pero no en currentRoles
                var rolesToAdd = model.Roles.Except(currentRoles).ToArray();
                if (rolesToAdd.Any())
                {
                    // Antes de añadir, asegurar que los roles existan en el sistema
                    foreach (var roleToAdd in rolesToAdd)
                    {
                        if (!await RoleManager.RoleExistsAsync(roleToAdd))
                        {
                            // Opcional: Crear el rol si no existe, o devolver error.
                            // await RoleManager.CreateAsync(new IdentityRole(roleToAdd));
                            ModelState.AddModelError("Roles", $"El rol '{roleToAdd}' no existe.");
                            return BadRequest(ModelState);
                        }
                    }
                    result = await UserManager.AddToRolesAsync(user.Id, rolesToAdd);
                    if (!result.Succeeded) return GetErrorResult(result);
                }
            }
            else // Si model.Roles es null, quizás se quieren quitar todos los roles
            {
                var currentRoles = await UserManager.GetRolesAsync(user.Id);
                if (currentRoles.Any())
                {
                    result = await UserManager.RemoveFromRolesAsync(user.Id, currentRoles.ToArray());
                    if (!result.Succeeded) return GetErrorResult(result);
                }
            }


            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Empleados/{id}
        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> DeleteEmpleado(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("ID de empleado no puede ser vacío.");

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult resultDelete = await UserManager.DeleteAsync(user);
            if (!resultDelete.Succeeded)
            {
                return GetErrorResult(resultDelete);
            }
            return Ok(new { Message = "Empleado eliminado exitosamente.", Id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null) _userManager.Dispose();
                if (_roleManager != null) _roleManager.Dispose();
                if (_dbEdmx != null) _dbEdmx.Dispose();
            }
            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null) return InternalServerError();
            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors) ModelState.AddModelError("", error);
                }
                if (ModelState.IsValid) return BadRequest(); // No ModelState errors are available to send
                return BadRequest(ModelState);
            }
            return null;
        }
    }
}