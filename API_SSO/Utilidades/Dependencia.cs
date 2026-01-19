using API_SSO.Procesos;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;
using Microsoft.Extensions.DependencyInjection;

namespace API_SSO.Utilidades
{
    public static class Dependencia
    {
        public static void InyectarDependencias(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddAutoMapper(typeof(AutoMapperProfile));

            //Procesos
            services.AddScoped(typeof(ClienteProceso));
            services.AddScoped(typeof(BaseDeDatosProceso));
            services.AddScoped(typeof(RolProceso));
            services.AddScoped(typeof(UsuarioProceso));

            //API_SSO
            services.AddScoped(typeof(IClienteService<>),typeof(ClienteService<>));
            services.AddScoped(typeof(IComprobantePagoService<>),typeof(ComprobantePagoService<>));
            services.AddScoped(typeof(IEmpresaService<>),typeof(EmpresaService<>));
            services.AddScoped(typeof(IEmpresaXclienteService<>),typeof(EmpresaXClienteService<>));
            services.AddScoped(typeof(ILogService<>),typeof(LogService<>));
            services.AddScoped(typeof(IProyectoActualServce<>),typeof(ProyectoActualService<>));
            services.AddScoped(typeof(IUsuarioxEmpresaService<>),typeof(UsuarioXEmpresaService<>));
            services.AddScoped(typeof(IRolService<>),typeof(RolService<>));
        }
    }
}
