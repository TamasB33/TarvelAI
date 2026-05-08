using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TarvelAI.DTOs.AI;

namespace TarvelAI.Services;

public interface IOpenRouterClient
{
    Task<AiQuestionGenerationResult?> GenerateQuestionAsync(string prompt, CancellationToken cancellationToken);
}

public sealed class AiQuestionGenerationResult
{
    public bool IsComplete { get; set; }
    public string QuestionText { get; set; } = "";
    public bool AllowsMultiple { get; set; }
    public string? Reason { get; set; }
    public List<AiOptionDto> Options { get; set; } = [];
}

public sealed class OpenRouterClient(
    HttpClient httpClient,
    IOptions<OpenRouterOptions> options,
    ILogger<OpenRouterClient> logger
) : IOpenRouterClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly OpenRouterOptions _options = options.Value;

    public async Task<AiQuestionGenerationResult?> GenerateQuestionAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/chat/completions";
            var requestBody = new
            {
                model = _options.Model,
                response_format = new
                {
                    type = "json_object"
                },
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            if (!string.IsNullOrWhiteSpace(_options.Referer) &&
                Uri.TryCreate(_options.Referer, UriKind.Absolute, out var refererUri))
            {
                request.Headers.Referrer = refererUri;
            }

            if (!string.IsNullOrWhiteSpace(_options.AppTitle))
            {
                request.Headers.Add("X-Title", _options.AppTitle);
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var failureBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "OpenRouter API returned {StatusCode}. Body: {ResponsePreview}",
                    response.StatusCode,
                    TruncateForLog(failureBody, 300));
                return null;
            }

            var successBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(successBody);
            if (!TryReadOpenRouterText(doc, out var jsonText))
            {
                logger.LogWarning(
                    "OpenRouter response did not contain expected choices/message content. Body: {BodyPreview}",
                    TruncateForLog(successBody, 300));
                return null;
            }

            var parsed = JsonSerializer.Deserialize<AiQuestionGenerationResult>(jsonText, JsonOptions);
            return parsed;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenRouter call/parse failed.");
            return null;
        }
    }

    private static string TruncateForLog(string input, int maxLength) =>
        input.Length <= maxLength ? input : input[..maxLength];

    private static bool TryReadOpenRouterText(JsonDocument doc, out string text)
    {
        text = "";
        try
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("choices", out var choices))
            {
                return false;
            }
            if (choices.GetArrayLength() == 0)
            {
                return false;
            }

            var choice = choices[0];
            var message = choice.GetProperty("message");
            if (!message.TryGetProperty("content", out var contentNode))
            {
                return false;
            }

            if (contentNode.ValueKind == JsonValueKind.Null)
            {
                if (choice.TryGetProperty("text", out var choiceTextNode) &&
                    choiceTextNode.ValueKind == JsonValueKind.String)
                {
                    text = choiceTextNode.GetString() ?? "";
                    return !string.IsNullOrWhiteSpace(text);
                }

                if (message.TryGetProperty("reasoning", out var reasoningNode) &&
                    reasoningNode.ValueKind == JsonValueKind.String)
                {
                    text = reasoningNode.GetString() ?? "";
                    return !string.IsNullOrWhiteSpace(text);
                }

                return false;
            }

            if (contentNode.ValueKind == JsonValueKind.String)
            {
                text = contentNode.GetString() ?? "";
                return !string.IsNullOrWhiteSpace(text);
            }

            if (contentNode.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var sb = new StringBuilder();
            foreach (var item in contentNode.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object ||
                    !item.TryGetProperty("type", out var typeNode) ||
                    !string.Equals(typeNode.GetString(), "text", StringComparison.OrdinalIgnoreCase) ||
                    !item.TryGetProperty("text", out var textNode))
                {
                    continue;
                }

                var piece = textNode.GetString();
                if (!string.IsNullOrWhiteSpace(piece))
                {
                    sb.Append(piece);
                }
            }

            text = sb.ToString();
            return !string.IsNullOrWhiteSpace(text);
        }
        catch
        {
            return false;
        }
    }
}
