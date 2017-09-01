using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.Sample.API.Models;
using NetCore.Identity.Sample.API.Tokens;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly JwtFactory _jwtFactory;
        private readonly UserManager<Entities.User> _userManager;
        private readonly SignInManager<Entities.User> _signInManager;

        public AuthController(
            JwtFactory jwtFactory,
            UserManager<Entities.User> userManager,
            SignInManager<Entities.User> signInManager)
        {
            _jwtFactory = jwtFactory;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAnonymous()
        {
            // Use a GUID for Id and UserName
            var guid = Guid.NewGuid();

            // Create the user object
            var user = new Entities.User()
            {
                Id = guid,
                IsAnonymous = true,
                UserName = guid.ToString()
            };

            // Add the user
            var result = await _userManager.CreateAsync(user);

            var response = new
            {
                id = guid,
                expires_in = TimeSpan.FromMinutes(10).TotalSeconds,
                auth_token = _jwtFactory.GenerateToken(user.Id.ToString(), user.UserName, roles: new[] { "LowSec" }, tokenDuration: TimeSpan.FromMinutes(10))
            };

            return Ok(JsonConvert.SerializeObject(response, Formatting.None));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel userModel)
        {
            // Use a GUID for Id
            var guid = Guid.NewGuid();

            // Create the user object
            var user = new Entities.User()
            {
                Id = guid,
                IsAnonymous = false,
                Email = userModel.Email,
                UserName = userModel.Username,
            };

            // Add the user
            var result = await _userManager.CreateAsync(user, userModel.Password);

            if (result.Succeeded)
            {
                var response = new
                {
                    id = guid,
                    expires_in = TimeSpan.FromMinutes(10).TotalSeconds,
                    auth_token = _jwtFactory.GenerateToken(user.Id.ToString(), user.UserName, roles: new[] { "HighSec", "Other" }, tokenDuration: TimeSpan.FromMinutes(10))
                };

                return Ok(JsonConvert.SerializeObject(response, Formatting.None));
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}