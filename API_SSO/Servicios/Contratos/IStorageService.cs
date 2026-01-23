using Microsoft.AspNetCore.Http;

namespace API_SSO.Servicios.Contratos
{
 public interface IStorageService
 {
 Task<string> UploadFileAsync(IFormFile file, string keyPrefix, CancellationToken cancellationToken = default);
 }
}
