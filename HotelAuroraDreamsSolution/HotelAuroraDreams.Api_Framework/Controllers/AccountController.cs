// File: ~/Controllers/AccountController.cs
using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Models.DTO;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HotelAuroraDreams.Api_Framework.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;

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

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

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
                FechaContratacion = System.DateTime.UtcNow,
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

            // Aquí podrías asignar roles si ya los tienes definidos
            // await UserManager.AddToRoleAsync(user.Id, "Empleado");

            return Ok(new { Message = "User registered successfully." });
        }

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

            var userInfo = new UserInfoViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                HotelID = user.HotelID,
                CargoID = user.CargoID
            };

            return Ok(userInfo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
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
    }
}