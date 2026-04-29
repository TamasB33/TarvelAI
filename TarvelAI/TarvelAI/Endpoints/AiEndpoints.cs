using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TarvelAI.DTOs.AI;
using TarvelAI.Repositories;

namespace TarvelAI.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/converse", async (
            List<ConversationTurn> conversation,
            IHotelRepository hotels,
            IFlightRepository flights,
            ITripRepository trips
        ) =>
        {
            // Extract preferences from conversation (simple demo logic)
            var prefs = new ConversationPreferences();
            int userAnswers = 0;
            foreach (var turn in conversation)
            {
                if (turn.Role == "User")
                {
                    userAnswers++;
                    switch (userAnswers)
                    {
                        case 1: prefs.Weather = turn.Text; break;
                        case 2: prefs.LocationType = turn.Text; break;
                        case 3:
                            if (decimal.TryParse(turn.Text, out var budget)) prefs.Budget = budget;
                            break;
                        case 4: prefs.DestinationType = turn.Text; break;
                        case 5:
                            // Very basic date parsing (expand as needed)
                            var parts = turn.Text.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2 && DateTime.TryParse(parts[0], out var start) && DateTime.TryParse(parts[1], out var end))
                            {
                                prefs.StartDate = start;
                                prefs.EndDate = end;
                            }
                            break;
                    }
                }
            }

            bool isRecommendation = userAnswers >= 5;

            if (isRecommendation)
            {
                // Filter hotels, flights, trips based on preferences (very basic demo logic)
                var allHotels = await hotels.GetAllAsync();
                var allFlights = await flights.GetAllAsync();
                var allTrips = await trips.GetAllAsync();

                var filteredHotels = allHotels.Where(h =>
                    (string.IsNullOrEmpty(prefs.LocationType) || h.City?.ToLower().Contains(prefs.LocationType.ToLower()) == true || h.Country?.ToLower().Contains(prefs.LocationType.ToLower()) == true)
                ).Take(5).ToList();

                var filteredFlights = allFlights.Where(f =>
                    (prefs.Budget == null || f.FlightNumber != null) // Replace with real budget logic
                ).Take(5).ToList();

                var filteredTrips = allTrips.Where(t =>
                    (string.IsNullOrEmpty(prefs.DestinationType) || t.Destination?.ToLower().Contains(prefs.DestinationType.ToLower()) == true)
                ).Take(5).ToList();

                var text = $"Here is your recommendation:\n" +
                    $"Hotels: {string.Join(", ", filteredHotels.Select(h => h.Name))}\n" +
                    $"Flights: {string.Join(", ", filteredFlights.Select(f => f.FlightNumber))}\n" +
                    $"Trips: {string.Join(", ", filteredTrips.Select(t => t.Name))}";
                return Results.Ok(new ConversationTurn { Role = "AI", Text = text, IsRecommendation = true });
            }
            else
            {
                string[] questions = new[]
                {
                    "What kind of weather do you prefer? (Warm, Mild, Cold)",
                    "Do you prefer a city or a village?",
                    "What is your budget?",
                    "What type of destination do you want? (Beach, Mountain, City Break, Countryside)",
                    "What are your travel dates? (e.g. 2024-07-01 to 2024-07-10)"
                };
                var nextQ = questions.ElementAtOrDefault(userAnswers) ?? "Thank you!";
                return Results.Ok(new ConversationTurn { Role = "AI", Text = nextQ, IsRecommendation = false });
            }
        });
    }
}
