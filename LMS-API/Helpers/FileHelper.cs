using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

public static class FileHelper
{
    private static readonly string RootPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    /// <summary>
    /// Uploads a file to the specified directory inside "uploads"
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folderName">The main folder inside "uploads" (e.g., "books", "users")</param>
    /// <param name="entityId">The unique ID (e.g., bookId, userId)</param>
    /// <param name="fileName">The desired file name</param>
    /// <returns>The relative path of the uploaded file</returns>
    public static async Task<string?> UploadFileAsync(IFormFile file, string folderName, string entityId, string fileName)
    {
        if (file == null || file.Length == 0) return null;

        try
        {
            // Create main directory if it doesn't exist
            string entityFolderPath = Path.Combine(RootPath, folderName, entityId);
            if (!Directory.Exists(entityFolderPath))
                Directory.CreateDirectory(entityFolderPath);

            // Determine file path
            string filePath = Path.Combine(entityFolderPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path for database storage
            return Path.Combine(folderName, entityId, fileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File upload error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes a file based on its relative path
    /// </summary>
    public static bool DeleteFile(string relativePath)
    {
        try
        {
            string fullPath = Path.Combine(RootPath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File deletion error: {ex.Message}");
        }
        return false;
    }
}
