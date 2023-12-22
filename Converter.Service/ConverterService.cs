using Converter.Service.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using System.Collections.Concurrent;

namespace Converter.Service
{
    public class ConverterService : IConverterService
    {
        private const int PARALLELISM_LIMIT = 4;
        private readonly ILogger<IConverterService> _logger;

        public ConverterService(ILogger<IConverterService> logger)
        {
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task<bool> ConvertImage(string sourcePath, string targetFolder, ImageType targetType)
        {
            try
            {
                if (!IsValidImageExtension(Path.GetExtension(sourcePath)))
                {
                    return false;
                }

                EnsureDirectoryExists(targetFolder);

                string targetFile = GetTargetFilePath(sourcePath, targetFolder, targetType);
                await ConvertAndSaveImageAsync(sourcePath, targetFile, targetType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong during {nameof(ConvertImage)}");
                throw;
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<string>> ConvertImages(IEnumerable<IFormFile> imageFiles, string targetFolder, ImageType targetType)
        {
            try
            {
                var convertedFilePaths = new ConcurrentBag<string>();
                EnsureDirectoryExists(targetFolder);

                await Parallel.ForEachAsync(imageFiles, new ParallelOptions { MaxDegreeOfParallelism = PARALLELISM_LIMIT }, async (imageFile, _) =>
                {
                    var tempSourcePath = await SaveTemporaryFileAsync(imageFile, targetFolder);
                    var conversionSuccess = await ConvertImage(tempSourcePath, targetFolder, targetType);

                    if (conversionSuccess)
                    {
                        var targetFile = GetTargetFilePath(tempSourcePath, targetFolder, targetType);
                        convertedFilePaths.Add(targetFile);
                    }

                    File.Delete(tempSourcePath);
                });
                return convertedFilePaths;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong during {nameof(ConvertImages)}");
                throw;
            }

        }

        private static async Task<string> SaveTemporaryFileAsync(IFormFile file, string targetFolder)
        {
            var tempSourcePath = Path.Combine(targetFolder, file.FileName);
            using (var fileStream = new FileStream(tempSourcePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return tempSourcePath;
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static string GetTargetFilePath(string sourcePath, string targetFolder, ImageType targetType)
        {
            return Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(sourcePath) + "." + targetType.ToString().ToLower());
        }

        private static bool IsValidImageExtension(string extension)
        {
            return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".tiff", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task ConvertAndSaveImageAsync(string sourcePath, string targetFile, ImageType targetType)
        {
            using var image = await Image.LoadAsync(sourcePath);
            var encoder = GetEncoder(targetType);
            await image.SaveAsync(targetFile, encoder);
        }

        private static IImageEncoder GetEncoder(ImageType targetType)
        {
            return targetType switch
            {
                ImageType.Bmp => new BmpEncoder(),
                ImageType.Jpeg => new JpegEncoder(),
                ImageType.Gif => new GifEncoder(),
                ImageType.Tiff => new TiffEncoder(),
                ImageType.Png => new PngEncoder(),
                ImageType.Webp => new WebpEncoder(),
                _ => throw new NotSupportedException($"Image type {targetType} is not supported.")
            };
        }
    }
}
