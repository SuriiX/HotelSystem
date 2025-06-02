// File: ~/Controllers/AccountController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System; // Necesario para DateTime
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Net;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
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
                return _roleManager ?? Request.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok(new { Message = "User registered successfully." });
        }

        // Este es el bloque de atributos que estaba suelto y causaba el problema.
        // Ya no es necesario porque GetUserInfo() está definido correctamente abajo.
        // [Authorize]
        // [Route("UserInfo")]
        // [HttpGet] 

        [HttpPost]
        [AllowAnonymous]
        [Route("SetupRoles")]
        public async Task<IHttpActionResult> SetupRoles()
        {
            string[] roleNames = { "Administrador", "Empleado" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                if (!await RoleManager.RoleExistsAsync(roleName))
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        return GetErrorResult(roleResult);
                    }
                }
            }
            return Ok(new { Message = "Roles 'Administrador' y 'Empleado' configurados/verificados." });
        }

        [HttpGet]
        [Route("AdminData")]
        [Authorize(Roles = "Administrador")]
        public IHttpActionResult GetAdminSpecificData()
        {
            var adminData = new
            {
                Message = "Estos son datos secretos solo para Administradores.",
                CurrentServerTime = DateTime.Now,
                LoggedInUser = User.Identity.Name
            };
            return Ok(adminData);
        }

        [HttpGet] // Asegúrate de que [HttpGet] esté aquí
        [Authorize]
        [Route("UserInfo")]
        public async Task<IHttpActionResult> GetUserInfo()
        {
            string userId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in token.");
            }

            ApplicationUser user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

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

            return Ok(userInfo);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AssignRoleToUser")]
        public async Task<IHttpActionResult> AssignRoleToUser(string userEmail, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Email del usuario y nombre del rol son requeridos.");
            }

            ApplicationUser user = await UserManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFoundResultWithMessage($"Usuario con email '{userEmail}' no encontrado.");
            }

            if (!await RoleManager.RoleExistsAsync(roleName))
            {
                return NotFoundResultWithMessage($"Rol '{roleName}' no encontrado. Por favor, ejecute SetupRoles primero o créelo manualmente.");
            }

            if (await UserManager.IsInRoleAsync(user.Id, roleName))
            {
                return Ok(new { Message = $"El usuario '{userEmail}' ya pertenece al rol '{roleName}'." });
            }

            IdentityResult result = await UserManager.AddToRoleAsync(user.Id, roleName);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok(new { Message = $"Usuario '{userEmail}' añadido exitosamente al rol '{roleName}'." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
            }
            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }
                return BadRequest(ModelState);
            }
            return null;
        }

        private IHttpActionResult NotFoundResultWithMessage(string message)
        {
            return Content(HttpStatusCode.NotFound, new { Message = message });
        }
    }
}