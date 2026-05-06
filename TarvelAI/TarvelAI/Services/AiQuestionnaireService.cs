using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using TarvelAI.DTOs.AI;
using TarvelAI.DTOs.Trip;

namespace TarvelAI.Services;

public interface IAiQuestionnaireService
{
    Task<(string SessionId, AiQuestionNodeDto Question)> StartAsync(string? goal, CancellationToken cancellationToken);
    Task<AiQuestionNodeDto> NextQuestionAsync(string sessionId, string questionId, List<string> selectedOptionIds, CancellationToken cancellationToken);
    IReadOnlyList<SessionAnswer> GetHistory(string sessionId);
    bool SessionExists(string sessionId);
}

public sealed class SessionAnswer
{
    public string QuestionId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public List<AiOptionDto> Options { get; set; } = [];
    public List<string> SelectedOptionIds { get; set; } = [];
}

internal sealed class QuestionnaireSession
{
    public string SessionId { get; set; } = "";
    public string? Goal { get; set; }
    public AiQuestionNodeDto? CurrentQuestion { get; set; }
    public List<SessionAnswer> History { get; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastAccessedAtUtc { get; set; }
}

internal sealed class GeminiQuestionResult
{
    public bool IsComplete { get; set; }
    public string QuestionText { get; set; } = "";
    public bool AllowsMultiple { get; set; }
    public string? Reason { get; set; }
    public List<AiOptionDto> Options { get; set; } = [];
}

public sealed class AiQuestionnaireService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AiQuestionnaireService> logger
) : IAiQuestionnaireService
{
    private const int MaxQuestions = 7;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, QuestionnaireSession> _sessions = new();
    private readonly TimeSpan _sessionTtl = TimeSpan.FromMinutes(
        Math.Max(5, configuration.GetValue<int?>("AiQuestionnaire:SessionTtlMinutes") ?? 30)
    );
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(
        Math.Max(1, configuration.GetValue<int?>("AiQuestionnaire:CleanupIntervalMinutes") ?? 5)
    );
    private DateTime _lastCleanupUtc = DateTime.UtcNow;

    public bool SessionExists(string sessionId)
    {
        CleanupExpiredSessions();
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        if (IsExpired(session))
        {
            _sessions.TryRemove(sessionId, out _);
            return false;
        }

        TouchSession(session);
        return true;
    }

    public IReadOnlyList<SessionAnswer> GetHistory(string sessionId)
    {
        CleanupExpiredSessions();
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return [];
        }

        if (IsExpired(session))
        {
            _sessions.TryRemove(sessionId, out _);
            return [];
        }

        TouchSession(session);
        return session.History.ToList();
    }

    public async Task<(string SessionId, AiQuestionNodeDto Question)> StartAsync(string? goal, CancellationToken cancellationToken)
    {
        CleanupExpiredSessions();
        var now = DateTime.UtcNow;
        var sessionId = Guid.NewGuid().ToString("N");
        var session = new QuestionnaireSession
        {
            SessionId = sessionId,
            Goal = goal,
            CreatedAtUtc = now,
            LastAccessedAtUtc = now
        };

        var firstQuestion = await GenerateQuestionAsync(session, cancellationToken);
        session.CurrentQuestion = firstQuestion;
        _sessions[sessionId] = session;
        return (sessionId, firstQuestion);
    }

    public async Task<AiQuestionNodeDto> NextQuestionAsync(
        string sessionId,
        string questionId,
        List<string> selectedOptionIds,
        CancellationToken cancellationToken
    )
    {
        CleanupExpiredSessions();
        if (!_sessions.TryGetValue(sessionId, out var session) || session.CurrentQuestion is null)
        {
            throw new InvalidOperationException("Invalid session.");
        }

        if (IsExpired(session))
        {
            _sessions.TryRemove(sessionId, out _);
            throw new InvalidOperationException("Session expired. Please start a new questionnaire.");
        }

        if (!string.Equals(session.CurrentQuestion.QuestionId, questionId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Question does not match current session state.");
        }

        var validOptionIds = session.CurrentQuestion.Options.Select(o => o.Id).ToHashSet(StringComparer.Ordinal);
        if (selectedOptionIds.Count == 0 || selectedOptionIds.Any(id => !validOptionIds.Contains(id)))
        {
            throw new InvalidOperationException("Selected options are invalid.");
        }

        session.History.Add(new SessionAnswer
        {
            QuestionId = session.CurrentQuestion.QuestionId,
            QuestionText = session.CurrentQuestion.QuestionText,
            Options = session.CurrentQuestion.Options.ToList(),
            SelectedOptionIds = selectedOptionIds.ToList()
        });
        TouchSession(session);

        if (session.History.Count >= MaxQuestions)
        {
            return new AiQuestionNodeDto
            {
                QuestionId = $"complete-{session.History.Count}",
                QuestionText = "Questionnaire complete.",
                IsComplete = true,
                AllowsMultiple = false,
                Reason = "Reached maximum questionnaire length."
            };
        }

        var next = await GenerateQuestionAsync(session, cancellationToken);
        session.CurrentQuestion = next;
        TouchSession(session);
        return next;
    }

    private bool IsExpired(QuestionnaireSession session) =>
        DateTime.UtcNow - session.LastAccessedAtUtc > _sessionTtl;

    private static void TouchSession(QuestionnaireSession session) =>
        session.LastAccessedAtUtc = DateTime.UtcNow;

    private void CleanupExpiredSessions()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanupUtc < _cleanupInterval)
        {
            return;
        }

        foreach (var pair in _sessions)
        {
            if (now - pair.Value.LastAccessedAtUtc > _sessionTtl)
            {
                _sessions.TryRemove(pair.Key, out _);
            }
        }

        _lastCleanupUtc = now;
    }

    private async Task<AiQuestionNodeDto> GenerateQuestionAsync(QuestionnaireSession session, CancellationToken cancellationToken)
    {
        var generated = await GenerateFromGeminiAsync(session, cancellationToken);
        if (generated is null)
        {
            generated = GenerateFallbackQuestion(session);
        }

        return NormalizeQuestion(generated, session.History.Count);
    }

    private AiQuestionNodeDto NormalizeQuestion(GeminiQuestionResult raw, int step)
    {
        var options = raw.Options
            .Where(o => !string.IsNullOrWhiteSpace(o.Label))
            .Select((o, idx) => new AiOptionDto
            {
                Id = string.IsNullOrWhiteSpace(o.Id) ? $"opt_{idx + 1}" : o.Id.Trim(),
                Label = o.Label.Trim()
            })
            .DistinctBy(o => o.Id)
            .Take(6)
            .ToList();

        return new AiQuestionNodeDto
        {
            QuestionId = $"q_{step + 1}_{Guid.NewGuid().ToString("N")[..8]}",
            QuestionText = string.IsNullOrWhiteSpace(raw.QuestionText) ? "Choose what fits you best." : raw.QuestionText.Trim(),
            AllowsMultiple = raw.AllowsMultiple,
            IsComplete = raw.IsComplete,
            Reason = raw.Reason,
            Options = raw.IsComplete ? [] : options
        };
    }

    private async Task<GeminiQuestionResult?> GenerateFromGeminiAsync(QuestionnaireSession session, CancellationToken cancellationToken)
    {
        var apiKey = configuration["Gemini:ApiKey"];
        var model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        try
        {
            var client = httpClientFactory.CreateClient("gemini");
            var historyText = BuildHistoryText(session.History);
            var prompt = @"Generate the next travel preference question for an adaptive questionnaire.
Return ONLY valid JSON (no markdown).

Constraints:
- Ask one concise multiple-choice question.
- 3 to 5 options.
- Option IDs must be short snake_case tokens.
- Use isComplete=true only when enough answers exist (usually after 5-7 questions).

Output schema:
{
  ""isComplete"": false,
  ""questionText"": ""What climate do you prefer?"",
  ""allowsMultiple"": false,
  ""reason"": ""optional short reason"",
  ""options"": [
    { ""id"": ""warm"", ""label"": ""Warm and sunny"" },
    { ""id"": ""mild"", ""label"": ""Mild temperatures"" },
    { ""id"": ""cold"", ""label"": ""Cold and snowy"" }
  ]
}

User goal: " + (session.Goal ?? "find a trip they'll love") + @"
Question index: " + (session.History.Count + 1) + @"
Previous answers:
" + historyText;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    responseMimeType = "application/json"
                }
            };

            using var response = await client.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}",
                requestBody,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Gemini call failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (!TryReadGeminiText(doc, out var jsonText))
            {
                logger.LogWarning("Gemini response could not be parsed.");
                return null;
            }

            var parsed = JsonSerializer.Deserialize<GeminiQuestionResult>(jsonText, JsonOptions);
            return parsed;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini question generation failed; using fallback.");
            return null;
        }
    }

    private static bool TryReadGeminiText(JsonDocument doc, out string text)
    {
        text = "";
        try
        {
            var root = doc.RootElement;
            var candidates = root.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                return false;
            }

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            if (parts.GetArrayLength() == 0)
            {
                return false;
            }

            text = parts[0].GetProperty("text").GetString() ?? "";
            return !string.IsNullOrWhiteSpace(text);
        }
        catch
        {
            return false;
        }
    }

    private static string BuildHistoryText(IEnumerable<SessionAnswer> history)
    {
        var sb = new StringBuilder();
        foreach (var item in history)
        {
            var labels = item.Options
                .Where(o => item.SelectedOptionIds.Contains(o.Id, StringComparer.Ordinal))
                .Select(o => o.Label);
            sb.AppendLine($"Q: {item.QuestionText}");
            sb.AppendLine($"A: {string.Join(", ", labels)}");
        }

        if (sb.Length == 0)
        {
            return "- no answers yet -";
        }

        return sb.ToString();
    }

    private static GeminiQuestionResult GenerateFallbackQuestion(QuestionnaireSession session)
    {
        var bank = new List<GeminiQuestionResult>
        {
            new()
            {
                QuestionText = "What weather do you enjoy most?",
                Options =
                [
                    new() { Id = "warm", Label = "Warm and sunny" },
                    new() { Id = "mild", Label = "Mild temperatures" },
                    new() { Id = "cold", Label = "Cold and snowy" }
                ]
            },
            new()
            {
                QuestionText = "Which destination vibe sounds best?",
                Options =
                [
                    new() { Id = "beach", Label = "Beach and coast" },
                    new() { Id = "city", Label = "City and culture" },
                    new() { Id = "nature", Label = "Nature and mountains" },
                    new() { Id = "mixed", Label = "A mix of everything" }
                ]
            },
            new()
            {
                QuestionText = "What budget range are you aiming for?",
                Options =
                [
                    new() { Id = "budget", Label = "Budget friendly" },
                    new() { Id = "mid", Label = "Mid-range" },
                    new() { Id = "premium", Label = "Premium / luxury" }
                ]
            },
            new()
            {
                QuestionText = "How long should your trip be?",
                Options =
                [
                    new() { Id = "short", Label = "1-3 days" },
                    new() { Id = "medium", Label = "4-7 days" },
                    new() { Id = "long", Label = "8+ days" }
                ]
            },
            new()
            {
                IsComplete = true,
                QuestionText = "Questionnaire complete.",
                Reason = "Collected enough preferences for a recommendation."
            }
        };

        var idx = Math.Min(session.History.Count, bank.Count - 1);
        return bank[idx];
    }
}

public static class RecommendationEngine
{
    public static List<AiRecommendationDto> RankTrips(IEnumerable<TripDto> trips, IReadOnlyList<SessionAnswer> history)
    {
        var selectedTokens = history
            .SelectMany(h => h.Options.Where(o => h.SelectedOptionIds.Contains(o.Id)).Select(o => o.Label.ToLowerInvariant()))
            .SelectMany(SplitTokens)
            .Where(t => t.Length >= 3)
            .ToHashSet(StringComparer.Ordinal);

        var recommendations = trips
            .Select(trip =>
            {
                var searchable = $"{trip.Name} {trip.Destination} {trip.Description}".ToLowerInvariant();
                var tokenMatches = selectedTokens.Count(token => searchable.Contains(token, StringComparison.Ordinal));
                var budgetBoost = BudgetBoost(searchable, selectedTokens);
                var durationBoost = DurationBoost(trip.DurationDays, selectedTokens);
                var score = tokenMatches + budgetBoost + durationBoost;

                return new AiRecommendationDto
                {
                    TripId = trip.Id,
                    Name = trip.Name,
                    Destination = trip.Destination,
                    Description = trip.Description,
                    BasePrice = trip.BasePrice,
                    DurationDays = trip.DurationDays,
                    MatchScore = score,
                    MatchReason = BuildReason(tokenMatches, budgetBoost, durationBoost)
                };
            })
            .OrderByDescending(r => r.MatchScore)
            .ThenBy(r => r.BasePrice)
            .Take(5)
            .ToList();

        return recommendations;
    }

    private static IEnumerable<string> SplitTokens(string text) =>
        text.Split([' ', ',', '.', ';', ':', '/', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static int BudgetBoost(string searchable, HashSet<string> tokens)
    {
        if (tokens.Contains("budget") && (searchable.Contains("budget") || searchable.Contains("affordable")))
        {
            return 2;
        }

        if (tokens.Contains("luxury") && (searchable.Contains("luxury") || searchable.Contains("premium")))
        {
            return 2;
        }

        return 0;
    }

    private static int DurationBoost(int durationDays, HashSet<string> tokens)
    {
        if (tokens.Contains("short") && durationDays <= 3) return 2;
        if (tokens.Contains("medium") && durationDays is >= 4 and <= 7) return 2;
        if (tokens.Contains("long") && durationDays >= 8) return 2;
        return 0;
    }

    private static string BuildReason(int tokenMatches, int budgetBoost, int durationBoost)
    {
        var reasons = new List<string>();
        if (tokenMatches > 0) reasons.Add("destination/preferences match");
        if (budgetBoost > 0) reasons.Add("budget alignment");
        if (durationBoost > 0) reasons.Add("duration alignment");
        return reasons.Count == 0 ? "general relevance" : string.Join(", ", reasons);
    }
}
