namespace Camagru.Web.Models.Shared;

public class AuthCardViewModel
{
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Intro { get; set; } = string.Empty;
    public string FormPartialPath { get; set; } = string.Empty;
    public object? FormModel { get; set; }
    public string SecondaryLinkText { get; set; } = string.Empty;
    public string SecondaryLinkUrl { get; set; } = string.Empty;
    public string SecondaryHint { get; set; } = string.Empty;
    public string AsideTitle { get; set; } = string.Empty;
    public IReadOnlyList<string> AsidePoints { get; set; } = [];
}

public class ToastMessageViewModel
{
    public string Kind { get; set; } = "info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaginationViewModel
{
    public string Label { get; set; } = "Pagination";
    public IReadOnlyList<PaginationLinkViewModel> Links { get; set; } = [];
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public string PreviousUrl { get; set; } = string.Empty;
    public string NextUrl { get; set; } = string.Empty;
}

public class PaginationLinkViewModel
{
    public int PageNumber { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class CarouselViewModel
{
    public string Id { get; set; } = string.Empty;
    public string AccessibleLabel { get; set; } = string.Empty;
    public IReadOnlyList<string> ImageUrls { get; set; } = [];
    public string AltPrefix { get; set; } = "Slide";
}

public class StatusPageViewModel
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryActionText { get; set; } = string.Empty;
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string? SecondaryActionText { get; set; }
    public string? SecondaryActionUrl { get; set; }
    public string? FeatureName { get; set; }
    public string? MissingContractName { get; set; }
}

public class FormFieldViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string? Placeholder { get; set; }
    public string? Hint { get; set; }
    public bool Disabled { get; set; }
    public bool ReadOnly { get; set; }
    public bool IsMultiline { get; set; }
    public int Rows { get; set; } = 3;
    public string? Prefix { get; set; }
}
