namespace TaskAspNet.Web.Interfaces;

public interface IImageService
{
    Task<string?> UploadAsync(IFormFile file, string category);
    IReadOnlyList<string> GetPredefined(string category);
}
