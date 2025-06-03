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
        private readonly ClsAccount _accountService;

        public AccountController()
        {
            _accountService = new ClsAccount();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            _accountService = new ClsAccount(userManager, roleManager);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _accountService.RegistrarUsuario(model);

            if (!resultado.Exito)
                return GetErrorResult(resultado.Resultado);

            return Ok(new { Message = resultado.Mensaje });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SetupRoles")]
        public async Task<IHttpActionResult> SetupRoles()
        {
            var resultado = await _accountService.SetupRoles();
            if (!resultado.Exito)
                return GetErrorResult(resultado.Resultado);

            return Ok(new { Message = resultado.Mensaje });
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        [Route("AdminData")]
        public IHttpActionResult GetAdminSpecificData()
        {
            return Ok(new
            {
                Message = "Estos son datos secretos solo para Administradores.",
                CurrentServerTime = DateTime.Now,
                LoggedInUser = User.Identity.Name
            });
        }

        [HttpGet]
        [Authorize]
        [Route("UserInfo")]
        public async Task<IHttpActionResult> GetUserInfo()
        {
            var userId = User.Identity.GetUserId();
            var resultado = await _accountService.ObtenerInfoUsuario(userId);

            if (!resultado.Exito)
                return BadRequest(resultado.Error);

            return Ok(resultado.DatosUsuario);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("AssignRoleToUser")]
        public async Task<IHttpActionResult> AssignRoleToUser(string userEmail, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Email del usuario y nombre del rol son requeridos.");

            var resultado = await _accountService.AsignarRol(userEmail, roleName);

            if (!resultado.Exito)
            {
                if (resultado.Resultado == null)
                    return Content(HttpStatusCode.NotFound, new { Message = resultado.Mensaje });

                return GetErrorResult(resultado.Resultado);
            }

            return Ok(new { Message = resultado.Mensaje });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _accountService.Dispose();

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}