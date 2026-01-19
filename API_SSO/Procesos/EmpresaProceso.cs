using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios.Contratos;

namespace API_SSO.Procesos
{
    public class EmpresaProceso
    {
        private readonly IEmpresaService<SSOContext> _EmpresaService;
        private readonly BaseDeDatosProceso _baseDeDatosProceso;

        public EmpresaProceso(IEmpresaService<SSOContext> empresaService, BaseDeDatosProceso baseDeDatosProceso)
        {
            _EmpresaService = empresaService;
            _baseDeDatosProceso = baseDeDatosProceso;
        }

        public async Task<RespuestaDTO> CrearEmpresa(EmpresaDTO empresa)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            empresa.FechaRegistro = DateTime.Now;
            empresa.Eliminado = false;
            var empresaCreada = await _EmpresaService.CrearYObtener(empresa);
            string nombreBD = empresaCreada.NombreComercial + string.Format("{0:D3}", empresaCreada.Id);
            //Ejecuta el proceso para crear la base de datos
            await _baseDeDatosProceso.CrearBaseDeDatos(nombreBD);

            var bDCreada = await _baseDeDatosProceso.VerifyInstallation(nombreBD);
            if (!bDCreada)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar dar de alta la empresa.";
                return respuesta;
            }
            respuesta.Estatus = true;
            respuesta.Descripcion = "Empresa creada correctamente.";
            return respuesta;
        }
    }
}
