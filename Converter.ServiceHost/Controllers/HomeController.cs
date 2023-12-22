using Converter.Service;
using Converter.Service.Helper;
using Converter.ServiceHost.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Compression;

namespace Converter.ServiceHost.Controllers
{
    public class HomeController : Controller
    {
        private const string CONVERTED_FOLDER_PATH = "converted_images";
        private readonly ILogger<HomeController> _logger;
        private readonly IConverterService _converterService;

        public HomeController(ILogger<HomeController> logger, IConverterService converterService)
        {
            _logger = logger;
            _converterService = converterService;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(List<IFormFile> files, ImageType targetType)
        {
            var targetFolder = CreateTargetFolder();

            try
            {
                var convertedFiles = await _converterService.ConvertImages(files, targetFolder, targetType);
                return HandleFiles(convertedFiles, targetFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong during {nameof(Convert)}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            finally
            {
                CleanupFolders(targetFolder);
            }
        }

        private string CreateTargetFolder()
        {
            var dateTimeFormat = DateTime.Now.ToString("yyyyMMddHHmm");
            var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp", $"converted_{dateTimeFormat}");
            var convertedFolder = Path.Combine(targetFolder, CONVERTED_FOLDER_PATH);
            Directory.CreateDirectory(convertedFolder);
            return targetFolder;
        }

        private IActionResult HandleFiles(IEnumerable<string> convertedImagePaths, string targetFolder)
        {
            var convertedFolder = Path.Combine(targetFolder, CONVERTED_FOLDER_PATH);
            var zipFileName = "converted_images.zip";
            var zipFilePath = Path.Combine(targetFolder, zipFileName);

            foreach (var convertedImagePath in convertedImagePaths)
            {
                var imageName = Path.GetFileName(convertedImagePath);
                System.IO.File.Move(convertedImagePath, Path.Combine(convertedFolder, imageName));
            }

            ZipFile.CreateFromDirectory(convertedFolder, zipFilePath);
            var content = System.IO.File.ReadAllBytes(zipFilePath);
            var contentType = "application/zip";
            return File(content, contentType);
        }

        private void CleanupFolders(string targetFolder)
        {
            var convertedFolder = Path.Combine(targetFolder, CONVERTED_FOLDER_PATH);
            if (Directory.Exists(convertedFolder))
            {
                Directory.Delete(convertedFolder, true);
            }
            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
