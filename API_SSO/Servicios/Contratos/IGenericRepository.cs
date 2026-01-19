using System.Linq.Expressions;

namespace API_SSO.Servicios.Contratos
{
    public interface IGenericRepository<TModel, T>
    {
        Task<TModel> Obtener(Expression<Func<TModel, bool>> filtro);

        Task<List<TModel>> ObtenerTodos(Expression<Func<TModel, bool>> filtro);
        Task<List<TModel>> ObtenerTodos();
        Task<TModel> Crear(TModel modelo);
        Task<bool> CrearMultiple(List<TModel> modelo);
        Task<bool> EditarMultiple(List<TModel> modelo);
        Task<bool> Editar(TModel modelo);
        Task<bool> Eliminar(TModel modelo);
        Task<bool> EliminarMultiple(List<TModel> modelo);
    }
}
