namespace TarvelAI.DTOs.AI;

public sealed class AiOptionDto
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
}

public sealed class AiQuestionNodeDto
{
    public string QuestionId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public bool AllowsMultiple { get; set; }
    public bool IsComplete { get; set; }
    public string? Reason { get; set; }
    public List<AiOptionDto> Options { get; set; } = [];
}

public sealed class AiQuestionnaireStartRequest
{
    public string? Goal { get; set; }
}

public sealed class AiQuestionnaireAnswerRequest
{
    public string SessionId { get; set; } = "";
    public string QuestionId { get; set; } = "";
    public List<string> SelectedOptionIds { get; set; } = [];
}

public sealed class AiRecommendationDto
{
    public int TripId { get; set; }
    public string Name { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal BasePrice { get; set; }
    public int DurationDays { get; set; }
    public double MatchScore { get; set; }
    public string MatchReason { get; set; } = "";
}

public sealed class AiQuestionnaireResponseDto
{
    public string SessionId { get; set; } = "";
    public bool IsComplete { get; set; }
    public string Message { get; set; } = "";
    public AiQuestionNodeDto? Question { get; set; }
    public List<AiRecommendationDto> Recommendations { get; set; } = [];
}
