using API_SSO.Servicios.Contratos;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace API_SSO.Servicios
{
 public class S3StorageService : IStorageService
 {
 private readonly IConfiguration _configuration;
 private readonly IAmazonS3 _s3Client;
 private readonly string _bucketName;

 public S3StorageService(IConfiguration configuration)
 {
 _configuration = configuration;
 var accessKey = _configuration["AWS:AccessKeyId"];
 var secretKey = _configuration["AWS:SecretAccessKey"];
 var region = _configuration["AWS:Region"];
 _bucketName = _configuration["AWS:BucketName"];

 // Create S3 client using default credentials if keys not provided, otherwise use basic credentials
 var config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) };
 _s3Client = new AmazonS3Client(accessKey, secretKey, config);
 }

 public async Task<string> UploadFileAsync(IFormFile file, string keyPrefix, CancellationToken cancellationToken = default)
 {
 if (file == null || file.Length ==0)
 throw new ArgumentException("file");

 var fileExt = Path.GetExtension(file.FileName);
 var key = Path.Combine(keyPrefix, DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + Path.GetRandomFileName() + fileExt).Replace("\\","/");

 using var stream = file.OpenReadStream();
 var fileTransferUtility = new TransferUtility(_s3Client);
 var request = new TransferUtilityUploadRequest
 {
 InputStream = stream,
 Key = key,
 BucketName = _bucketName,
 ContentType = file.ContentType
 };

 await fileTransferUtility.UploadAsync(request, cancellationToken);

 // Return the S3 object URL (path-style may vary depending on bucket settings and region)
 var url = $"https://{_bucketName}.s3.{_configuration["AWS:Region"]}.amazonaws.com/{key}";
 return url;
 }
 }
}
