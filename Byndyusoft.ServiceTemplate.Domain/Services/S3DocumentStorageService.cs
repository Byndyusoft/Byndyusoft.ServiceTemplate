namespace Byndyusoft.ServiceTemplate.Domain.Services
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Settings;

    public class S3DocumentStorageService : IDocumentStorageService
    {
        private readonly string _bucketName;
        private readonly AmazonS3Client _client;
        private readonly ILogger<S3DocumentStorageService> _logger;

        public S3DocumentStorageService(ILogger<S3DocumentStorageService> logger,
                                        IOptions<S3Settings> options)
        {
            _logger = logger;

            var s3Settings = options.Value;

            var amazonS3Config = new AmazonS3Config
                                     {
                                         RegionEndpoint = RegionEndpoint.USEast1,
                                         ServiceURL = s3Settings.ServiceUrl,
                                         ForcePathStyle = true
                                     };

            _client = new AmazonS3Client(s3Settings.AccessKey, s3Settings.SecretKey, amazonS3Config);

            _bucketName = s3Settings.BucketName;
        }

        /// <summary>
        ///     Получение общедоступной ссылки на документ
        /// </summary>
        /// <param name="documentIdentifier"></param>
        /// <returns></returns>
        public string GetLink(string documentIdentifier)
        {
            //TODO вынести в настройки
            var storageLink = $"https://{_bucketName}.s829.storage.cloud.rt.ru/rest/{documentIdentifier}";

            return storageLink;
        }

        /// <summary>
        ///     Генерация уникального идентификатор файла и оригинальнозо названия файла
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <returns></returns>
        public string GenerateIdentifier(string? originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);

            return $"{Guid.NewGuid()}{extension}";
        }

        public async Task Upload(string documentIdentifier, byte[] bytes, CancellationToken cancellationToken)
        {
            try
            {
                var request = new PutObjectRequest
                                  {
                                      BucketName = _bucketName,
                                      Key = documentIdentifier
                                  };

                var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);

                using (request.InputStream = new MemoryStream(bytes))
                {
                    var response = await _client.PutObjectAsync(request, combinedTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3 Upload exception");
                throw;
            }
        }

        public async Task<byte[]> Download(string documentIdentifier, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Downloading file with id {FileId}", documentIdentifier);

            try
            {
                var request = new GetObjectRequest
                                  {
                                      BucketName = _bucketName,
                                      Key = documentIdentifier
                                  };

                using GetObjectResponse response = await _client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);

                await using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

                return memoryStream.ToArray();
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                _logger.LogError(amazonS3Exception, "S3 Download exception");
                throw;
            }
        }

        public async Task<bool> Delete(string documentIdentifier, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting file with id {FileId}", documentIdentifier);

            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                                              {
                                                  BucketName = _bucketName,
                                                  Key = documentIdentifier
                                              };

                var response = await _client.DeleteObjectAsync(deleteObjectRequest, cancellationToken).ConfigureAwait(false);

                return response != null;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                _logger.LogError(amazonS3Exception, "S3 delete exception");
                throw;
            }
        }

        public async Task<bool> Exists(string documentIdentifier)
        {
            try
            {
                var response = await _client.GetObjectMetadataAsync(_bucketName, documentIdentifier).ConfigureAwait(false);

                return response != null;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                _logger.LogError(amazonS3Exception, "S3 Exists exception");
                throw;
            }
        }
    }
}