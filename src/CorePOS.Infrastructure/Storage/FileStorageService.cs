using Microsoft.Extensions.Logging;

namespace CorePOS.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(byte[] imageBytes, string fileName, string subFolder = "products");
    Task<bool>   DeleteFileAsync(string filePath);
    string       GetAbsolutePath(string relativePath);
    bool         FileExists(string relativePath);
    Task<byte[]?> ReadFileAsync(string relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger  = logger;
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CorePOS", "Storage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveImageAsync(
        byte[] imageBytes, string fileName, string subFolder = "products")
    {
        try
        {
            var folder = Path.Combine(_basePath, subFolder);
            Directory.CreateDirectory(folder);

            // Ensure unique filename
            var ext     = Path.GetExtension(fileName);
            var name    = Path.GetFileNameWithoutExtension(fileName);
            var unique  = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var fullPath= Path.Combine(folder, unique);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            // Return relative path
            return Path.Combine(subFolder, unique);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image: {FileName}", fileName);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string relativePath)
    {
        try
        {
            var full = GetAbsolutePath(relativePath);
            if (File.Exists(full))
            {
                File.Delete(full);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", relativePath);
            return Task.FromResult(false);
        }
    }

    public string GetAbsolutePath(string relativePath)
        => Path.Combine(_basePath, relativePath);

    public bool FileExists(string relativePath)
        => File.Exists(GetAbsolutePath(relativePath));

    public async Task<byte[]?> ReadFileAsync(string relativePath)
    {
        try
        {
            var full = GetAbsolutePath(relativePath);
            return File.Exists(full) ? await File.ReadAllBytesAsync(full) : null;
        }
        catch { return null; }
    }
}
