using API_SSO.Context;
using API_SSO.DTO;
using API_SSO.Servicios;
using API_SSO.Servicios.Contratos;

namespace API_SSO.Procesos
{
    public class ComprobantePagoProceso
    {
        private readonly IComprobantePagoService<SSOContext> _Comprobanteservice;
        private readonly IConfiguration _configuracion;
        private readonly IInvitacionService _invitacionService;
        private readonly IClienteService<SSOContext> _clienteService;

        public ComprobantePagoProceso(IComprobantePagoService<SSOContext> comprobanteservice, IConfiguration configuracion, IInvitacionService invitacionService, IClienteService<SSOContext> clienteService)
        {
            _Comprobanteservice = comprobanteservice;
            _configuracion = configuracion;
            _invitacionService = invitacionService;
            _clienteService = clienteService;
        }

        public async Task<RespuestaDTO> SubirComprobante(IFormFile archivo, int idCliente, List<System.Security.Claims.Claim> claims)
        {
            RespuestaDTO respuesta = new RespuestaDTO();
            var IdUsStr = claims.Where(z => z.Type == "idUsuario").ToList();
            if (IdUsStr[0].Value == null)
            {
                respuesta.Descripcion = "Los datos del usuario están incompletos.";
                respuesta.Estatus = false;
                return respuesta;
            }
            //Obtiene la ruta base para guardar las imagenes
            var ruta = _configuracion["Rutas:Imagenes"];
            if(ruta == null)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró la ruta de destino para las imágenes.";
                return respuesta;
            }
            if(archivo == null)
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
            var fecha = DateTime.Now;
            var mes = fecha.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
            //Obtiene el nombre del archivo
            var nombreArchivo = DateTime.Now.Millisecond + archivo.FileName;
            //Genera la ruta compuesta
            var rutaCompuesta = Path.Combine(ruta, fecha.Year.ToString(), mes, fecha.Day.ToString());
            //Comprueba si la ruta existe, si no existe la crea
            if (!Directory.Exists(rutaCompuesta))
            {
                try
                {
                    Directory.CreateDirectory(rutaCompuesta);
                }
                catch
                {
                    respuesta.Descripcion = "Ocurrió un error al intentar subir el archivo.";
                    respuesta.Estatus = false;
                    return respuesta;
                }
            }
            //Crea la ruta final con el nombre del archivo
            var rutaFinal = Path.Combine(rutaCompuesta, nombreArchivo);
            var pesoBytes = 0;
            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    //Lee el archivo y lo guarda en la ruta final
                    await archivo.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    pesoBytes = contenido.Length;
                    await File.WriteAllBytesAsync(rutaFinal, contenido);
                }
                catch
                {
                    respuesta.Descripcion = "Ocurrió un error al intentar subir el archivo.";
                    respuesta.Estatus = false;
                    return respuesta;
                }
            }
            ComprobantePagoDTO comprobante = new ComprobantePagoDTO
            {
                IdCliente = idCliente,
                UserId = IdUsStr[0].Value,
                Ruta = rutaFinal,
                Estatus = 0, //Capturado
                FechaCarga = DateTime.Now,
                IdUsuarioAutorizador = null,
                Eliminado = false,
            };
            var comprobanteCreado = await _Comprobanteservice.CrearYObtener(comprobante);
            if (comprobanteCreado.Id <= 0)
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
            if (comprobante.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el comprobante de pago";
                return respuesta;
            }
            comprobante.Estatus = 2; //Cancelado
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
            if (comprobante.Id <= 0)
            {
                respuesta.Estatus = false;
                respuesta.Descripcion = "No se encontró el comprobante de pago";
                return respuesta;
            }
            comprobante.Estatus = 1; //Autorizado
            comprobante.IdUsuarioAutorizador = IdUsStr[0].Value;
            respuesta = await _Comprobanteservice.Editar(comprobante);
            if (respuesta.Estatus)
            {
                var cliente = await _clienteService.ObtenerXId(comprobante.IdCliente);
                if(cliente.Id <= 0)
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
