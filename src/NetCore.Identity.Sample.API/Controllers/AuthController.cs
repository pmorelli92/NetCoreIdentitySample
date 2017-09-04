using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.Sample.API.Models;
using NetCore.Identity.Sample.API.Tokens;
using System;
using System.Threading.Tasks;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly JwtFactory _jwtFactory;
        private readonly UserManager<Entities.User> _userManager;

        public AuthController(
            JwtFactory jwtFactory,
            UserManager<Entities.User> userManager)
        {
            _jwtFactory = jwtFactory;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAnonymous()
        {
            // Use a GUID for Id and UserName
            var userId = Guid.NewGuid();

            // Create the user object
            var user = new Entities.User()
            {
                Id = userId,
                IsAnonymous = true,
                UserName = userId.ToString()
            };

            // Add the user
            var result = await _userManager.CreateAsync(user);

            var tokenExp = TimeSpan.FromMinutes(10);
            var token = _jwtFactory.GenerateToken(user.Id.ToString(), user.UserName, roles: new[] { "LowSec" }, tokenDuration: tokenExp);
            return Ok(new TokenResponse(userId, token, (long)tokenExp.TotalSeconds));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel userModel)
        {
            // Use a GUID for Id
            var userId = Guid.NewGuid();

            // Create the user object
            var user = new Entities.User()
            {
                Id = userId,
                IsAnonymous = false,
                Email = userModel.Email, // We assume that is already valid
                UserName = userModel.Username,
            };

            // Add the user
            var result = await _userManager.CreateAsync(user, userModel.Password);

            if (result.Succeeded)
            {
                var tokenExp = TimeSpan.FromMinutes(10);
                var token = _jwtFactory.GenerateToken(user.Id.ToString(), user.UserName, roles: new[] { "HighSec", "Other" }, tokenDuration: tokenExp);
                return Ok(new TokenResponse(userId, token, (long)tokenExp.TotalSeconds));
            }
            else
                return BadRequest(result.Errors);
        }
    }
}