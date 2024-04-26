using ApiExamenCubos.Data;
using ApiExamenCubos.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiExamenCubos.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;
        public RepositoryCubos(CubosContext context)
        {
            this.context = context;
        }

        #region SEGURIDAD
        public async Task<Usuario> LogInUsuarioAsync(string email, string password)
        {
            return await this.context.Usuarios.Where(x => x.Email==email && x.Pass == password).FirstOrDefaultAsync();
        }

        #endregion

        public async Task RegisterUserAsync(Usuario user)
        {
            this.context.Usuarios.Add(user);
            await this.context.SaveChangesAsync();
        }
        public async Task<Usuario> GetUsuarioAsync(int idUser)
        {
            return await this.context.Usuarios.FirstOrDefaultAsync(z => z.IdUsuario == idUser);
        }

        public async Task<List<Cubo>> GetCubosAsync()
        {
            return await this.context.Cubos.ToListAsync();
        }

        public async Task<List<Cubo>> GetCubosMarcaAsync(string marca)
        {
            return await this.context.Cubos.Where(x => x.Marca == marca).ToListAsync();
        }

        public async Task<List<CompraCubos>> GetPedidosUserAsync(int idUser)
        {
            return await this.context.Compras.Where(x => x.IdUsuario==idUser).ToListAsync();
        }
        
        public async Task CrearPedido(CompraCubos compra)
        {
            this.context.Compras.Add(compra);
            await this.context.SaveChangesAsync();
        }

    }
}
