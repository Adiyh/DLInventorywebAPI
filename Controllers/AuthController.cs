using System.ComponentModel.DataAnnotations;
using LaptopService.Core.Services.Interface;
using LaptopService.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            try
            {
                _authService = authService;
            }
            catch (Exception ex)
            {
                StatusCode(500, "An Error has occured:" + ex.Message);
                throw;
            }
        }

        [HttpPost]
        [Route("user-login")]
        public IActionResult Login(User user)
        {
            try
            {
                var loggedInUser = _authService.Login(user.Username, user.Password);

                if (loggedInUser == null)
                    return Unauthorized("Invalid username or password.");

                return Ok("Login successful.");
            }
            catch (Exception ex)
            {
                StatusCode(500, "An Error has occured:" + ex.Message);
                throw;
            }
        }
    }
}
