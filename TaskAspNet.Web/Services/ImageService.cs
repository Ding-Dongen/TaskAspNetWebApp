using System.Text.RegularExpressions;
using TaskAspNet.Web.Interfaces;

// Created with the help of ChatGPT4.5
// Uploads the provided file into uploads/
// Creates the folder if it doesn't exist, prefixes filename with a GUID, and returns the URL
// Build the path under wwwroot
// Generate a unique filename and save it
// Retrieves the filenames of all predefined images in images
// Only returns files ending in .png, .jpg, .jpeg, or .svg case-insensitive

public sealed class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    public ImageService(IWebHostEnvironment env) => _env = env;

    public async Task<string?> UploadAsync(IFormFile file, string cat)
    {
        if (file == null || file.Length == 0) return null;

        var folder = Path.Combine(_env.WebRootPath, "uploads", cat);
        Directory.CreateDirectory(folder);

        var name = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(folder, name);

        await using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);

        return $"/uploads/{cat}/{name}";
    }

    public IReadOnlyList<string> GetPredefined(string cat)
        => Directory.GetFiles(Path.Combine(_env.WebRootPath, "images", cat))
                    .Where(f => Regex.IsMatch(f, @"\.(png|jpe?g|svg)$", RegexOptions.IgnoreCase))
                    .Select(Path.GetFileName)!
                    .ToList();
}
