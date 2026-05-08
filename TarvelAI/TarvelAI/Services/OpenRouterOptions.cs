using System.ComponentModel.DataAnnotations;

namespace TarvelAI.Services;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    [Required]
    public string ApiKey { get; set; } = "";

    [Required]
    public string Model { get; set; } = "minimax/minimax-m2.5";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    [Url]
    public string? Referer { get; set; }

    public string? AppTitle { get; set; }
}
