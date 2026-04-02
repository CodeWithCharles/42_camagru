namespace Camagru.Application.Contracts.Posts;

public class PublishMontageRequest
{
    public int UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Stream BaseImageStream { get; set; } = Stream.Null;
    public string BaseImageFileName { get; set; } = string.Empty;
    public IReadOnlyList<PublishMontageOverlayInstruction> Overlays { get; set; } = [];
}

public class PublishMontageResponse
{
    public int PostId { get; set; }
    public IReadOnlyList<string> ImageUrls { get; set; } = [];
}

public class PublishMontageOverlayInstruction
{
    public int OverlayId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int ZIndex { get; set; }
    public double RotationDegrees { get; set; }
}

public class OverlayRenderInstruction
{
    public string AssetPath { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int ZIndex { get; set; }
    public double RotationDegrees { get; set; }
}
