using System.ComponentModel.DataAnnotations;

namespace API_SSO.DTO
{
    public class RespuestaAutenticacionDTO
    {
        /// <summary>
        /// Token temporal para ingresar al software
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Fecha de expiración del token para entrar al sistema
        /// </summary>
        public DateTime FechaExpiracion { get; set; }
    }
    /// <summary>
    /// Para entrar en sistema y para la creación de un nuevo Usuario
    /// </summary>
    public class CredencialesUsuarioDTO
    {
        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [Required]
        public string Email { get; set; }
        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    public class UsuarioRolDTO
    {
        public string IdUsuario { get; set; }
        public string IdRol { get; set; }
        public string AntiguoRolId { get; set; }
    }

    public class RecuperacionContrasenaDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NuevaContrasena { get; set; }
    }
}
