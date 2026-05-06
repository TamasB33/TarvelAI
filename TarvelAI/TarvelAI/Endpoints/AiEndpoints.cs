using TarvelAI.DTOs.AI;
using TarvelAI.Repositories;
using TarvelAI.Services;

namespace TarvelAI.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/questionnaire/start", async (
            AiQuestionnaireStartRequest request,
            IAiQuestionnaireService questionnaireService,
            CancellationToken cancellationToken
        ) =>
        {
            var (sessionId, question) = await questionnaireService.StartAsync(request.Goal, cancellationToken);
            return Results.Ok(new AiQuestionnaireResponseDto
            {
                SessionId = sessionId,
                IsComplete = false,
                Message = "Questionnaire started.",
                Question = question
            });
        });

        app.MapPost("/api/ai/questionnaire/answer", async (
            AiQuestionnaireAnswerRequest request,
            IAiQuestionnaireService questionnaireService,
            ITripRepository trips,
            CancellationToken cancellationToken
        ) =>
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.QuestionId))
            {
                return Results.BadRequest("SessionId and QuestionId are required.");
            }

            try
            {
                var next = await questionnaireService.NextQuestionAsync(
                    request.SessionId,
                    request.QuestionId,
                    request.SelectedOptionIds,
                    cancellationToken
                );

                if (!next.IsComplete)
                {
                    return Results.Ok(new AiQuestionnaireResponseDto
                    {
                        SessionId = request.SessionId,
                        IsComplete = false,
                        Message = "Next question generated.",
                        Question = next
                    });
                }

                var history = questionnaireService.GetHistory(request.SessionId);
                var allTrips = await trips.GetAllAsync();
                var recommendations = RecommendationEngine.RankTrips(allTrips, history);

                return Results.Ok(new AiQuestionnaireResponseDto
                {
                    SessionId = request.SessionId,
                    IsComplete = true,
                    Message = "Recommendations ready.",
                    Recommendations = recommendations
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
    }
}
