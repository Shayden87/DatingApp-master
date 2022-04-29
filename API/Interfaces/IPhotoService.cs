using CloudinaryDotNet.Actions;

// Summary:
// Interface for the control of uploading and deleting photos from API.
namespace API.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}