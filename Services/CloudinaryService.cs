using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
namespace App.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing or incorrect!");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }


        public async Task<string> UploadImageAsync(IFormFile file, string prefix)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ!");

            string uniqueFileName = $"{prefix}_{Guid.NewGuid()}";

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = uniqueFileName,
                    UseFilename = true,
                    UniqueFilename = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                _logger.LogInformation($"Ảnh được tải lên thành công: {uploadResult.SecureUrl}");

                return uploadResult.SecureUrl.ToString();
            }
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("Bỏ qua xóa ảnh Cloudinary vì publicId rỗng");
                return;
            }

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            _logger.LogInformation("Xóa ảnh Cloudinary - publicId: {PublicId}, result: {Result}",
                publicId, result.Result);
        }

        public async Task<string> UploadVideoAsync(IFormFile file, string prefix)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ!");

            string uniqueFileName = $"{prefix}_{Guid.NewGuid()}";

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = uniqueFileName,
                    UseFilename = true,
                    UniqueFilename = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                _logger.LogInformation($"Video được tải lên thành công: {uploadResult.SecureUrl}");

                return uploadResult.SecureUrl.ToString();
            }
        }

        public async Task DeleteVideoAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Video // Sửa lỗi: Dùng enum thay vì string
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            _logger.LogInformation($"Xóa video kết quả: {result.Result}");
        }



        public static string ExtractPublicId(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            // Tìm vị trí cuối cùng của "/upload/"
            int startIndex = imageUrl.IndexOf("/upload/") + 8;
            if (startIndex == 7) return null; // Nếu không tìm thấy "/upload/"

            // Lấy phần còn lại của đường dẫn (bỏ "/upload/")
            string filePath = imageUrl.Substring(startIndex);

            // Bỏ phần version (v1701234567) nếu có
            string[] parts = filePath.Split('/');
            if (parts.Length > 1 && parts[0].StartsWith("v"))
            {
                return string.Join("/", parts, 1, parts.Length - 1).Split('.')[0]; // Bỏ phần mở rộng ".jpg", ".png"
            }

            return filePath.Split('.')[0]; // Nếu không có version, chỉ lấy tên file
        }

    }

}