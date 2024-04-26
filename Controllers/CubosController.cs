using ApiExamenCubos.Models;
using ApiExamenCubos.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiExamenCubos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CubosController : ControllerBase
    {
        private RepositoryCubos repo;

        public CubosController(RepositoryCubos repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<Cubo>>> GetCubos()
        {
            return await this.repo.GetCubosAsync();
        }
        [HttpGet]
        [Route("{marca}")]
        public async Task<ActionResult<List<Cubo>>> GetCubosMarca(string marca)
        {
            return await this.repo.GetCubosMarcaAsync(marca);
        }
        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<CompraCubos>>> GetPedidosUser()
        {
            string json = HttpContext.User.FindFirst(x => x.Type == "UserData").Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(json);
            Usuario currentUser = await this.repo.GetUsuarioAsync(user.IdUsuario);
            List<CompraCubos> compras = await this.repo.GetPedidosUserAsync(currentUser.IdUsuario);
            return compras;
        }
        [Authorize]
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> CreateCompra(CompraCubos compra)
        {
            string json = HttpContext.User.FindFirst(x => x.Type == "UserData").Value;
            Usuario user = JsonConvert.DeserializeObject<Usuario>(json);
            compra.IdUsuario = user.IdUsuario;
            await this.repo.CrearPedido(compra);
            return Ok();

        }

    }

}

