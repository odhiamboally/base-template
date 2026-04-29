namespace BT.Application.Configuration;
public class FileSettings
{
    public string? Path { get; set; }
    public string? UploadPath { get; set; }
    public string? UploadUrl { get; set; }
    public string TempPath { get; set; } = "";
    public string ImagesPath { get; set; } = "";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public List<string> AllowedExtensions { get; set; } = [];
}
