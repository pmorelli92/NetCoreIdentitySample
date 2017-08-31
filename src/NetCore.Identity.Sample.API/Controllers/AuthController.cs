using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.Sample.API.JWT;
using NetCore.Identity.Sample.API.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly UserManager<Entities.User> _userManager;
        private readonly SignInManager<Entities.User> _signInManager;


        public AuthController(
            IJwtFactory jwtFactory,
            JwtIssuerOptions jwtOptions,
            UserManager<Entities.User> userManager,
            SignInManager<Entities.User> signInManager)
        {
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions;
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

            var identity = _jwtFactory.GenerateClaimsIdentity(user.UserName, user.Id.ToString());

            var response = new
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = _jwtFactory.GenerateEncodedToken(user.UserName, identity),
                expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
            };

            var json = JsonConvert.SerializeObject(
                response, 
                new JsonSerializerSettings { Formatting = Formatting.Indented });

            return Ok(json);






            //var principal = await _signInManager.CreateUserPrincipalAsync(user);

            //var islogged = _signInManager.IsSignedIn(principal);






            //await _signInManager.SignInAsync(user, true);

            //islogged = _signInManager.IsSignedIn(principal);


            //// Get Claims
            //var identity = _jwtFactory.GenerateClaimsIdentity(userName: guid.ToString(), id: guid.ToString());

            //// TODO: HACHA PROBA LO QUE ME HABIAS MOSTRADO PERO AHORA CON ESA LINEA
            //// LO QUE SE ME OCURRE ES QUE MIRES FUERTE EL CÓDIGO DE UNA APP CON AUTH YA ARMADA POR DEFAULT
            //// Y TE FIJES BIEN COMO HACE PARA DECIR QUE EL USUARIO ESTA LOGUEADO, PORQUE ME PARECE QUE TE
            //// ESTA FALTANDO ESA PARTE, NO CREO QUE ALCANCE CON DEVOLVER EL TOKEN Y EL ATTR ASI NOMAS
            //Thread.CurrentPrincipal = new System.Security.Claims.ClaimsPrincipal(identity);

            


            //var response = new
            //{
            //    id = guid,
            //    expires_in = (long)_jwtOptions.ValidFor.TotalSeconds,
            //    auth_token = _jwtFactory.GenerateEncodedToken(guid.ToString(), principal.i)
            //};

            //return new OkObjectResult(JsonConvert.SerializeObject(response));
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
                var identity = _jwtFactory.GenerateClaimsIdentity(user.UserName, user.Id.ToString());

                var response = new
                {
                    id = identity.Claims.Single(c => c.Type == "id").Value,
                    auth_token = _jwtFactory.GenerateEncodedToken(user.UserName, identity),
                    expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
                };

                var json = JsonConvert.SerializeObject(
                    response,
                    new JsonSerializerSettings { Formatting = Formatting.Indented });

                return StatusCode(201, json);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }


    }
}