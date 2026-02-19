using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace API_SSO.Procesos
{
    public class ComprobantePagoProceso
    {
        private readonly IComprobantePagoService<SSOContext> _Comprobanteservice;
        private readonly IConfiguration _configuracion;
        private readonly IInvitacionService _invitacionService;
        private readonly IClienteService<SSOContext> _clienteService;
        private readonly IStorageService _storageService;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly LogProceso _logProceso;
        public ComprobantePagoProceso(IComprobantePagoService<SSOContext> comprobanteservice, IConfiguration configuracion, IInvitacionService invitacionService, IClienteService<SSOContext> clienteService, IStorageService storageService, UserManager<IdentityUser> userManager, LogProceso logProceso)
        {
            _Comprobanteservice = comprobanteservice;
            _configuracion = configuracion;
            _invitacionService = invitacionService;
            _clienteService = clienteService;
            _storageService = storageService;
            _UserManager = userManager;
            _logProceso = logProceso;
        }

        public async Task<List<ComprobantePagoDTO>> ObtenerTodos(List<Claim> claims)
        {
            var idUsStr = claims.Find(z => z.Type == "guid")?.Value;
            if(idUsStr == null)
            {
                return new List<ComprobantePagoDTO>();
            }
            var lista = await _Comprobanteservice.ObtenerTodos();
            await _logProceso.CrearLog(idUsStr, "Proceso", "ObtenerTodos", $"Consulta de todos los comprobantes de pago con {lista.Count} resultados");
            return lista;
        }

        public async Task<List<ComprobantePagoDTO>> ObtenerXIdCliente(int idCliente, List<Claim> claims)
        {
            var IdUsStr = claims.Find(z => z.Type == "guid")?.Value;
            if (IdUsStr == null)
            {
                return new List<ComprobantePagoDTO>();
            }
            var lista = await _Comprobanteservice.ObtenerTodos();
            lista = lista.Where(c=>c.IdCliente==idCliente).ToList();
            await _logProceso.CrearLog(IdUsStr, "Proceso", "ObtenerXIdCliente", $"Consulta de comprobantes de pago para cliente {idCliente} con {lista.Count} resultados");
            return lista;
        }

        public async Task<RespuestaDTO> SubirComprobante(IFormFile archivo, int idCliente, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var userEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var idUsuario = claims.FirstOrDefault(c => c.Type == "guid")?.Value;

            if (userEmail == null || claims.Count<=0 || idUsuario == null)  
            {
                respuesta.Descripcion = "Los datos del usuario están incompletos.";
                respuesta.Estatus = false;
                return respuesta;
            }

            var user = await _UserManager.FindByEmailAsync(userEmail.Value);

            if(user == null)
            {
                respuesta.Descripcion = "No se encontró al usuario.";
                respuesta.Estatus = false;
                return respuesta;
            }

            if (archivo == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el archivo que se va a publicar";
                return respuesta;
            }
            if (!(System.IO.Path.GetExtension(archivo.FileName).ToLower().Contains("png") || System.IO.Path.GetExtension(archivo.FileName).ToLower().Contains("jpg") || System.IO.Path.GetExtension(archivo.FileName).ToLower().Contains("jpeg") || System.IO.Path.GetExtension(archivo.FileName).ToLower().Contains("pdf")))
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "El tipo de archivo es incorrecto, debe ser pdf, jpg, jpeg o png";
                return respuesta;
            }

            // Use S3 storage service to upload file
            string keyPrefix = $"comprobantes/{idCliente}";
            string s3Url;
            try
            {
                s3Url = await _storageService.UploadFileAsync(archivo, keyPrefix);
            }
            catch
            {
                await _logProceso.CrearLog(idUsuario, "Proceso", "SubirComprobante", $"Error al subir el archivo {archivo.FileName} para cliente {idCliente} a almacenamiento S3");
                respuesta.Descripcion = "Ocurrió un error al intentar subir el archivo a almacenamiento S3.";
                respuesta.Estatus = false;
                return respuesta;
            }

            ComprobantePagoDTO comprobante = new ComprobantePagoDTO
            {
                IdCliente = idCliente,
                UserId = user.Id,
                Ruta = s3Url,
                Estatus =0, //Capturado
                FechaCarga = DateTime.Now,
                IdUsuarioAutorizador = null,
                Eliminado = false,
                IdEmpresa = null,
            };
            var comprobanteCreado = await _Comprobanteservice.CrearYObtener(comprobante);
            if (comprobanteCreado.Id <=0)
            {
                await _logProceso.CrearLog(idUsuario, "Proceso", "SubirComprobante", $"Error al guardar el comprobante para cliente {idCliente} con ruta {s3Url}");
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar guardar el comprobante";
                return respuesta;
            }
            await _logProceso.CrearLog(idUsuario, "Proceso", "SubirComprobante", $"Comprobante {comprobanteCreado.Id} creado exitosamente para cliente {idCliente} con ruta {s3Url}");
            respuesta.Estatus = true;
            respuesta.Descripcion = "Comprobante guardado exitosamente";
            return respuesta;
        }

        public async Task<RespuestaDTO> CancelaComprobantePago(int idComprobante, List<Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUsStr = claims.Find(z => z.Type == "guid")?.Value;
            if(IdUsStr == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Los datos del usuario están incompletos.";
                return respuesta;
            }
            var comprobante = await _Comprobanteservice.ObtenerXId(idComprobante);
            if (comprobante.Id <=0)
            {
                await _logProceso.CrearLog(IdUsStr, "Proceso", "CancelaComprobantePago", $"Intento de cancelación de comprobante {idComprobante} que no existe");
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el comprobante de pago";
                return respuesta;
            }
            comprobante.Estatus =2; //Cancelado
            respuesta = await _Comprobanteservice.Editar(comprobante);
            await _logProceso.CrearLog(IdUsStr, "Proceso", "CancelaComprobantePago", $"Cancelación de comprobante {idComprobante} con estatus: {respuesta.Descripcion}");
            return respuesta;
        }

        public async Task<RespuestaDTO> AutorizarComprobantePago(int idComprobante, List<System.Security.Claims.Claim> claims, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUsStr = claims.Where(z => z.Type == "idUsuario").ToList();
            if (IdUsStr.Count<=0)
            {
                respuesta.Descripcion = "Los datos del usuario están incompletos.";
                respuesta.Estatus = false;
                return respuesta;
            }
            var comprobante = await _Comprobanteservice.ObtenerXId(idComprobante);
            if (comprobante.Id <=0)
            {
                await _logProceso.CrearLog(IdUsStr[0].Value,"Proceso" ,"AutorizarComprobantePago", $"Intento de autorización de comprobante {idComprobante} que no existe");
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el comprobante de pago";
                return respuesta;
            }
            comprobante.Estatus =1; //Autorizado
            comprobante.IdUsuarioAutorizador = IdUsStr[0].Value;
            respuesta = await _Comprobanteservice.Editar(comprobante);
            if (respuesta.Estatus)
            {
                var cliente = await _clienteService.ObtenerXId(comprobante.IdCliente);
                if(cliente.Id <=0)
                {
                    await _logProceso.CrearLog(IdUsStr[0].Value,"Proceso" ,"AutorizarComprobantePago", $"Intento de autorización de comprobante {idComprobante} con cliente {comprobante.IdCliente} que no existe");
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "Ocurrío un error al encontrar al cliente";
                    return respuesta;
                }
                await _invitacionService.CrearYEnviar((string)cliente.Correo, ct);
                await _logProceso.CrearLog(IdUsStr[0].Value, "Proceso", "AutorizarComprobantePago", $"Autorización de comprobante {idComprobante} con estatus {respuesta.Descripcion}");
            }
            else
            {
                await _logProceso.CrearLog(IdUsStr[0].Value, "Proceso", "AutorizarComprobantePago", $"Intento de autorización de comprobante {idComprobante} con error {respuesta.Descripcion}");

            }
            return respuesta;
        }
    }
}
