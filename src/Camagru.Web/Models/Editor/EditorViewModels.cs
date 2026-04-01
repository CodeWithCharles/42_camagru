namespace Camagru.Web.Models.Editor;

public class EditorPageViewModel
{
    public string StorageKey { get; set; } = string.Empty;
    public IReadOnlyList<StickerGroupViewModel> StickerGroups { get; set; } = [];
    public EditorStageViewModel Stage { get; set; } = new();
    public IReadOnlyList<EditorThumbnailViewModel> SeedThumbnails { get; set; } = [];
}

public class StickerGroupViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<StickerViewModel> Stickers { get; set; } = [];
}

public class StickerViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Accent { get; set; } = string.Empty;
}

public class EditorStageViewModel
{
    public string EmptyTitle { get; set; } = string.Empty;
    public string EmptyDescription { get; set; } = string.Empty;
    public string PreviewNote { get; set; } = string.Empty;
}

public class EditorThumbnailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public string CapturedAtLabel { get; set; } = string.Empty;
}
