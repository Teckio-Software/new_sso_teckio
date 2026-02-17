using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;
using Microsoft.EntityFrameworkCore;

namespace API_SSO.Procesos
{
    public class EmpresaProceso
    {
        private readonly IEmpresaService<SSOContext> _EmpresaService;
        private readonly BaseDeDatosProceso _baseDeDatosProceso;
        private readonly IClienteService<SSOContext> _ClienteService;
        private readonly IEmpresaXclienteService<SSOContext> _empresaXClienteService;
        private readonly SSOContext _dbContext;
        private readonly LogProceso _logProceso;

        public EmpresaProceso(IEmpresaService<SSOContext> empresaService, BaseDeDatosProceso baseDeDatosProceso, IClienteService<SSOContext> clienteService, IEmpresaXclienteService<SSOContext> empresaXClienteService, SSOContext dbContext, LogProceso logProceso)
        {
            _EmpresaService = empresaService;
            _baseDeDatosProceso = baseDeDatosProceso;
            _ClienteService = clienteService;
            _empresaXClienteService = empresaXClienteService;
            _dbContext = dbContext;
            _logProceso = logProceso;
        }

        public async Task<RespuestaDTO> CrearEmpresa(EmpresaCreacionDTO empresa, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            //Crea el modelo de la empresa, inicialmente con su propio día de pago
            EmpresaDTO modelo = new EmpresaDTO
            {
                NombreComercial = empresa.NombreComercial,
                Rfc = empresa.Rfc,
                Estatus = true,
                FechaRegistro = DateTime.Now,
                CodigoPostal = empresa.CodigoPostal,
                Sociedad = empresa.Sociedad,
                DiaPago = DateTime.Now.Day,
                Eliminado = false
            };
            //Obtiene al cliente, sus relaciones y la última empresa registrada
            var cliente = await _ClienteService.ObtenerXId(empresa.IdCliente);
            var relaciones = await _empresaXClienteService.ObtenerPorIdCliente(empresa.IdCliente);
            if (relaciones.Count <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un problema al obtener la información del cliente";
                return respuesta;
            }
            var ultimaEmpresa = await _EmpresaService.ObtenerXId(relaciones[relaciones.Count - 1].IdEmpresa);
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                //Si aún no se define que los pagos son por empresa valida si aún quiere que sea el mismo día o ahora por separado
                if (!cliente.PagoXempresa)
                {
                    //Si se va a pagar el mismo día se cambia el día de pago por el de la última empresa
                    //De lo contrario se queda con el generado y se edita el cliente para que los pagos sean ahora por empresa
                    if (!empresa.PagoMismoDia)
                    {
                        cliente.PagoXempresa = true;
                        await _ClienteService.Editar(cliente);
                    }
                    else
                    {
                        modelo.DiaPago = ultimaEmpresa.DiaPago;
                    }
                }
                else
                {
                    modelo.DiaPago = ultimaEmpresa.DiaPago;
                }
                var empresaCreada = await _EmpresaService.CrearYObtener(modelo);
                if (empresaCreada != null)
                {
                    EmpresaXclienteDTO relacion = new EmpresaXclienteDTO
                    {
                        IdCliente = empresa.IdCliente,
                        IdEmpresa = empresaCreada.Id
                    };
                    var resultRelacion = await _empresaXClienteService.CrearYObtener(relacion);
                    if (resultRelacion.Id <= 0)
                    {
                        
                        throw new Exception("Ocurrió un error al intentar crear la empresa");
                        
                    }
                }
                string nombreBD = empresaCreada.NombreComercial + string.Format("{0:D3}", empresaCreada.Id);
                //Ejecuta el proceso para crear la base de datos
                await _baseDeDatosProceso.CrearBaseDeDatos(nombreBD);
                var bDCreada = await _baseDeDatosProceso.VerifyInstallation(nombreBD);
                if (!bDCreada)
                {
                    throw new Exception("Ocurrió un error al intentar dar de alta la empresa.");
                }
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(ct);
                respuesta.Estatus = false;
                respuesta.Descripcion = ex.Message;
                return respuesta;
            }
            await transaction.CommitAsync(ct);
            respuesta.Estatus = true;
            respuesta.Descripcion = "Empresa creada correctamente.";
            return respuesta;
        }

        public async Task<RespuestaDTO> EditarEmpresa(EmpresaDTO empresaDTO, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUsStr = claims.Where(z => z.Type == "idUsuario").ToList();
            if (IdUsStr[0].Value == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "La información del usuario es inconsistente.";
                return respuesta;
            }
            var IdUsuario = IdUsStr[0].Value;
            respuesta = await _EmpresaService.Editar(empresaDTO);
            if (respuesta.Estatus)
            {
                await _logProceso.CrearLog(IdUsuario, "EditarEmpresa", "Proceso", "Empresa editada exitosamente");
            }
            else
            {
                await _logProceso.CrearLog(IdUsuario, "EditarEmpresa", "Proceso", "Ocurrió un error al intentar editar la empresa");
            }
            return respuesta;
        }

        public async Task<List<EmpresaDTO>> ObtenerEmpresasXCliente(int idCliente)
        {
            List<EmpresaDTO> lista = new List<EmpresaDTO>();
            var relaciones = await _empresaXClienteService.ObtenerPorIdCliente(idCliente);
            foreach(var item in relaciones)
            {
                var empresa = await _EmpresaService.ObtenerXId(item.IdEmpresa);
                if (empresa.Id > 0)
                {
                    lista.Add(empresa);
                }
            }
            return lista;
        }
    }
}
