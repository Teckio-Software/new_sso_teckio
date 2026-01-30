using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;
using Microsoft.AspNetCore.Identity;

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

        public ComprobantePagoProceso(IComprobantePagoService<SSOContext> comprobanteservice, IConfiguration configuracion, IInvitacionService invitacionService, IClienteService<SSOContext> clienteService, IStorageService storageService, UserManager<IdentityUser> userManager)
        {
            _Comprobanteservice = comprobanteservice;
            _configuracion = configuracion;
            _invitacionService = invitacionService;
            _clienteService = clienteService;
            _storageService = storageService;
            _UserManager = userManager;

        }

        public async Task<RespuestaDTO> SubirComprobante(IFormFile archivo, int idCliente, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var userEmail = claims.FirstOrDefault(z => z.Type == "email");
            
            if (userEmail == null)  
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
            };
            var comprobanteCreado = await _Comprobanteservice.CrearYObtener(comprobante);
            if (comprobanteCreado.Id <=0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "Ocurrió un error al intentar guardar el comprobante";
                return respuesta;
            }
            respuesta.Estatus = true;
            respuesta.Descripcion = "Comprobante guardado exitosamente";
            return respuesta;
        }

        public async Task<RespuestaDTO> CancelaComprobantePago(int idComprobante)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var comprobante = await _Comprobanteservice.ObtenerXId(idComprobante);
            if (comprobante.Id <=0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el comprobante de pago";
                return respuesta;
            }
            comprobante.Estatus =2; //Cancelado
            respuesta = await _Comprobanteservice.Editar(comprobante);
            return respuesta;
        }

        public async Task<RespuestaDTO> AutorizarComprobantePago(int idComprobante, List<System.Security.Claims.Claim> claims, CancellationToken ct)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUsStr = claims.Where(z => z.Type == "idUsuario").ToList();
            if (IdUsStr[0].Value == null)
            {
                respuesta.Descripcion = "Los datos del usuario están incompletos.";
                respuesta.Estatus = false;
                return respuesta;
            }
            var comprobante = await _Comprobanteservice.ObtenerXId(idComprobante);
            if (comprobante.Id <=0)
            {
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
                    respuesta.Estatus = false;
                    respuesta.Descripcion = "Ocurrío un error al encontrar al cliente";
                    return respuesta;
                }
                await _invitacionService.CrearYEnviar((string)cliente.Correo, ct);
            }
            return respuesta;
        }
    }
}
