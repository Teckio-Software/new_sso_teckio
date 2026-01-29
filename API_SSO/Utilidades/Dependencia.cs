using API_SSO.Procesos;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
            services.AddScoped(typeof(EmpresaProceso));
            services.AddScoped(typeof(ComprobantePagoProceso));
            services.AddScoped(typeof(CatalogoPermisosProcess));
            //API_SSO
            services.AddScoped(typeof(IClienteService<>),typeof(ClienteService<>));
            services.AddScoped(typeof(IComprobantePagoService<>),typeof(ComprobantePagoService<>));
            services.AddScoped(typeof(IEmpresaService<>),typeof(EmpresaService<>));
            services.AddScoped(typeof(IEmpresaXclienteService<>),typeof(EmpresaXClienteService<>));
            services.AddScoped(typeof(ILogService<>),typeof(LogService<>));
            services.AddScoped(typeof(IProyectoActualServce<>),typeof(ProyectoActualService<>));
            services.AddScoped(typeof(IUsuarioxEmpresaService<>),typeof(UsuarioXEmpresaService<>));
            services.AddScoped(typeof(IRolService<>),typeof(RolService<>));
            services.AddScoped(typeof(IInvitacionService), typeof(InvitacionService));
            services.AddScoped(typeof(ITokenInvitateService), typeof(TokenInvitateService));
            services.AddScoped(typeof(ICatalogoSeccionService<>), typeof(CatalogoSeccionService<>));
            services.AddScoped(typeof(ICatalogoClaimService<>), typeof(CatalogoClaimService<>));

            // Storage (S3)
            services.AddScoped<IStorageService, S3StorageService>();
        }
    }
}
