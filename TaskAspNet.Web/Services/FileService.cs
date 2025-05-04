
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;

namespace TaskAspNet.Business.Services;

// Created with the help of ChatGPT4.5
// If no DTO provided, nothing to save
// Check if new files is uploaded check which upload folder to use under wwwroot
// Create a unique filename using a GUID
// Copy the uploaded file to disk
// Return the public URL path to the saved file
// If an existing image was selected instead of uploading map singular folderName to the appropriate icon folder

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string?> SaveImageAsync(UploadSelectImgDto imageData, string folderName)
    {
        if (imageData == null) return null;

        if (imageData.UploadedImage != null)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageData.UploadedImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageData.UploadedImage.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }
        else if (!string.IsNullOrEmpty(imageData.SelectedImage))
               {
            
                   var iconsFolder = folderName.TrimEnd('s') + "icon";
                   return $"/images/{iconsFolder}/{imageData.SelectedImage}";
               }

        return null;
    }
}
