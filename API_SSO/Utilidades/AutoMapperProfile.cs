using API_SSO.DTO;
using API_SSO.DTOs;
using API_SSO.Modelos;
using API_SSO.Models;
using AutoMapper;

namespace API_SSO.Utilidades
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            #region Cliente

            CreateMap<Cliente, ClienteDTO>();
            CreateMap<ClienteDTO, Cliente>()
                .ForMember(destino => destino.EmpresaXclientes, opt => opt.Ignore())
                .ForMember(destino => destino.ComprobantePagos, opt => opt.Ignore());

            #endregion

            #region ComprobantePago

            CreateMap<ComprobantePago, ComprobantePagoDTO>();
            CreateMap<ComprobantePagoDTO, ComprobantePago>()
                .ForMember(destino => destino.IdClienteNavigation, opt => opt.Ignore());


            #endregion

            #region Empresa

            CreateMap<Empresa, EmpresaDTO>();
            CreateMap<EmpresaDTO, Empresa>()
                .ForMember(destino => destino.EmpresaXclientes, opt => opt.Ignore())
                .ForMember(destino => destino.Logs, opt => opt.Ignore())
                .ForMember(destino => destino.ProyectoActuals, opt => opt.Ignore())
                .ForMember(destino => destino.UsuarioXempresas, opt => opt.Ignore());

            CreateMap<EmpresaXcliente, EmpresaXclienteDTO>();
            CreateMap<EmpresaXclienteDTO, EmpresaXcliente>()
                .ForMember(destino => destino.IdEmpresaNavigation, opt => opt.Ignore())
                .ForMember(destino => destino.IdClienteNavigation, opt => opt.Ignore());

            #endregion

            #region Log

            CreateMap<Log, LogDTO>();
            CreateMap<LogDTO, Log>()
                .ForMember(destino => destino.IdEmpresaNavigation, opt => opt.Ignore());

            #endregion

            #region ProyectoActual

            CreateMap<ProyectoActual, ProyectoActualDTO>();
            CreateMap<ProyectoActualDTO, ProyectoActual>()
                .ForMember(destino => destino.IdEmpresaNavigation, opt => opt.Ignore());

            #endregion

            #region UsuarioXEmpresa

            CreateMap<UsuarioXempresa, UsuarioXempresaDTO>();
            CreateMap<UsuarioXempresaDTO, UsuarioXempresa>()
                .ForMember(destino => destino.IdEmpresaNavigation, opt => opt.Ignore());

            #endregion

            #region Rol

            CreateMap<Rol, RolDTO>();
            CreateMap<RolDTO, Rol>();

            #endregion

        }
    }
}
