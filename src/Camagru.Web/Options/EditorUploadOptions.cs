namespace Camagru.Web.Options;

public class EditorUploadOptions
{
    public const string SectionName = "EditorUpload";

    public long MaxBytes { get; set; } = 5 * 1024 * 1024;

    public int MaxOverlayPayloadCharacters { get; set; } = 64 * 1024;

    public string[] AllowedExtensions { get; set; } = [".png", ".jpg", ".jpeg"];

    public string[] AllowedContentTypes { get; set; } = ["image/png", "image/jpeg"];
}
