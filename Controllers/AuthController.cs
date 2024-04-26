using ApiExamenCubos.Helpers;
using ApiExamenCubos.Models;
using ApiExamenCubos.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiExamenCubos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryCubos repo;

        // Cuando generemos el token debemos integrar 
        // dentro de dicho token, Issuer, Audience... 
        // Para quue lo valide cuando nos lo envien 
        private HelperActionServicesOAuth helper;

        public AuthController
            (RepositoryCubos repo, HelperActionServicesOAuth helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login
            (LoginModel model)
        {
            Usuario user = await this.repo.LogInUsuarioAsync(model.Email, model.Password);
            if (user == null)
            {
                return Unauthorized();
            }
            else
            {
                SigningCredentials credentials =
                    new SigningCredentials(helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);
                string jsonUsuario = JsonConvert.SerializeObject(user);
                Claim[] info = new[]
                {
                    new Claim("UserData",jsonUsuario)
                };
                JwtSecurityToken token =
                    new JwtSecurityToken(
                        claims: info,
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(30),
                        notBefore: DateTime.UtcNow
                        );
                return Ok(new
                {
                    response = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            var user = new Usuario
            {
                IdUsuario = model.IdUsuario,
                Nombre = model.Nombre,
                Email = model.Email,
                Pass = model.Pass,
                Imagen = model.Imagen
            };

            await repo.RegisterUserAsync(user);

            return Ok();
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Usuario>> GetLoggedUser()
        {
            string json = HttpContext.User.FindFirst(x => x.Type == "UserData").Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(json);
            user.Imagen = "https://storageaccountcubos.blob.core.windows.net/cubos/" + user.Imagen;
            Usuario currentUser = await this.repo.GetUsuarioAsync(user.IdUsuario);
            return currentUser;
        }

    }
}
