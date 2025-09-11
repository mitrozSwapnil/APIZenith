using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon;
using Microsoft.Extensions.Configuration;
using ZenithApp.ZenithMessage;

namespace ZenithApp.Services
{
    public class S3Repository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _settings;

        public S3Repository(IOptions<S3Settings> options)
        {
            _settings = options.Value;

            _s3Client = new AmazonS3Client(
                _settings.AccessKey,
                _settings.SecretKey,
                RegionEndpoint.GetBySystemName(_settings.AWSRegion)
            );
        }

        public async Task<(bool Success, string FileUrl, string ErrorMessage)> UploadFileAsync(IFormFile file, string keyName)
        {
            try
            {
                string fileName = Path.GetFileName(file.FileName);
                string fullKey = $"{_settings.BucketParentKey}/{keyName}/{fileName}";

                using var stream = file.OpenReadStream();
                var fileTransferUtility = new TransferUtility(_s3Client);

                await fileTransferUtility.UploadAsync(stream, _settings.BucketName, fullKey);

                // ✅ Build correct S3 URL
                string fileUrl = $"https://{_settings.BucketName}.s3.{_settings.AWSRegion}.amazonaws.com/{fullKey}";


                return (true, fileUrl, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }



    }
}
