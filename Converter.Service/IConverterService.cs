using Converter.Service.Helper;
using Microsoft.AspNetCore.Http;

namespace Converter.Service
{
    /// <summary>
    /// Provides methods for converting images to a specified type.
    /// </summary>
    public interface IConverterService
    {
        /// <summary>
        /// Converts a single image from a source path to a specified image type and saves it in the target folder.
        /// </summary>
        /// <param name="sourcePath">The path to the source image to be converted.</param>
        /// <param name="targetFolder">The folder where the converted image will be saved.</param>
        /// <param name="targetType">The type to which the image will be converted.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value indicating success or failure.
        /// </returns>
        public Task<bool> ConvertImage(string sourcePath, string targetFolder, ImageType targetType);

        /// <summary>
        /// Converts multiple images from the provided collection to a specified image type and saves them in the target folder.
        /// </summary>
        /// <param name="files">The collection of IFormFile objects representing the images to be converted.</param>
        /// <param name="targetFolder">The folder where the converted images will be saved.</param>
        /// <param name="targetType">The type to which the images will be converted.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is an IEnumerable of strings, representing the paths of the converted images.
        /// </returns>
        Task<IEnumerable<string>> ConvertImages(IEnumerable<IFormFile> files, string targetFolder, ImageType targetType);
    }

}