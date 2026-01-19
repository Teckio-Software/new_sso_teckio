using API_SSO.Servicios.Contratos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace API_SSO.Servicios
{
    public class GenericRepository<TModelo, T> : IGenericRepository<TModelo, T> where TModelo : class, new() where T : DbContext
    {
        private readonly T _dbcontext;

        public GenericRepository(T dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<TModelo> Obtener(Expression<Func<TModelo, bool>> filtro)
        {
            try
            {
                var modelo = await _dbcontext.Set<TModelo>().FirstOrDefaultAsync(filtro);
                if (modelo != null)
                    return modelo;
                return new TModelo();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<TModelo>> ObtenerTodos(Expression<Func<TModelo, bool>> filtro)
        {
            if (filtro != null)
            {
                List<TModelo> modelos = await _dbcontext.Set<TModelo>().Where(filtro).ToListAsync();
                return modelos;
            }
            else
            {
                List<TModelo> modelos = await _dbcontext.Set<TModelo>().ToListAsync();
                return modelos;
            }
        }
        public async Task<List<TModelo>> ObtenerTodos()
        {
            try
            {
                List<TModelo> modelos = await _dbcontext.Set<TModelo>().ToListAsync();
                return modelos;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<TModelo>();
            }
        }

        public async Task<TModelo> Crear(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Add(modelo);
                await _dbcontext.SaveChangesAsync();
                return modelo;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new TModelo();
            }
        }

        public async Task<bool> Editar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Update(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Eliminar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Remove(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarMultiple(List<TModelo> modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().RemoveRange(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CrearMultiple(List<TModelo> modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().AddRange(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        public async Task<bool> EditarMultiple(List<TModelo> modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().UpdateRange(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}
