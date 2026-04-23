using Microsoft.AspNetCore.SignalR;

namespace TarvelAI.Hubs;

/// <summary>
/// Central SignalR hub for all real-time TravelAI features.
/// Clients connect once; the server pushes destination cards and AI tokens as they are ready.
/// </summary>
public class TravelHub : Hub
{
    // ── Destination streaming ────────────────────────────────────────────────

    /// <summary>
    /// Called by a client to request a fresh batch of AI-curated destinations.
    /// The server simulates progressive discovery and pushes cards one-by-one.
    /// Replace the body with a real AI/search call when ready.
    /// </summary>
    public async Task RequestDestinations(string? query)
    {
        await Clients.Caller.SendAsync("StreamStarted", "destinations");

        var pool = DestinationPool;
        var rng = Random.Shared;
        var picks = pool.OrderBy(_ => rng.Next()).Take(6).ToList();

        foreach (var d in picks)
        {
            // Simulate progressive loading delay (remove in production)
            await Task.Delay(rng.Next(200, 600));

            var imageUrl = $"https://picsum.photos/seed/{d.Query}-{rng.Next(10000)}/800/600";
            await Clients.Caller.SendAsync("ReceiveDestination", new
            {
                d.Title,
                d.Description,
                ImageUrl = imageUrl,
                d.Tag
            });
        }

        await Clients.Caller.SendAsync("StreamEnded", "destinations");
    }

    // ── AI answer streaming ──────────────────────────────────────────────────

    /// <summary>
    /// Called by a client to ask the AI planner a question.
    /// Pushes tokens one-by-one for a typewriter effect.
    /// Replace the lorem-ipsum body with a real LLM streaming call.
    /// </summary>
    public async Task AskAI(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return;

        await Clients.Caller.SendAsync("StreamStarted", "ai");

        // ── Placeholder: simulate a streamed AI answer ───────────────────────
        // Swap this section for: await foreach (var token in _llmService.StreamAsync(prompt)) { ... }
        var fakeAnswer =
            $"Great question about \"{prompt}\"! " +
            "Based on current travel trends, seasonal weather patterns, and millions of curated reviews, " +
            "here are my top recommendations for your journey. " +
            "I suggest starting in the coastal regions where temperatures are mild, " +
            "then moving inland to explore cultural hotspots. " +
            "Would you like me to build a full day-by-day itinerary?";

        var words = fakeAnswer.Split(' ');
        foreach (var word in words)
        {
            await Task.Delay(Random.Shared.Next(40, 120));
            await Clients.Caller.SendAsync("ReceiveToken", word + " ");
        }
        // ────────────────────────────────────────────────────────────────────

        await Clients.Caller.SendAsync("StreamEnded", "ai");
    }

    // ── Seed data ────────────────────────────────────────────────────────────

    private record DestinationData(string Title, string Description, string Query, string Tag);

    private static readonly DestinationData[] DestinationPool =
    [
        new("Kyoto Modernism",   "Ancient shrines meet minimalist boutique hotels.",    "kyoto+temple",      "AI PICK"),
        new("Reykjavik Rim",     "Extreme luxury on Iceland's volcanic edge.",           "iceland+landscape", "TRENDING"),
        new("Amalfi Drift",      "Hidden coastal secrets revealed by AI.",               "amalfi+coast",      "SCENIC"),
        new("Serengeti Pulse",   "Ethical safari experiences mapped by AI.",             "serengeti+safari",  "ECO"),
        new("Santorini Glow",    "Sunset views over the caldera.",                       "santorini+sunset",  "POPULAR"),
        new("Patagonia Wild",    "Untamed beauty at the edge of the world.",             "patagonia+mountains","ADVENTURE"),
        new("Bali Serenity",     "Tropical wellness retreats off the beaten path.",      "bali+rice+terrace", "WELLNESS"),
        new("Swiss Alpine",      "Peaks and charm in the heart of Europe.",              "swiss+alps",        "LUXURY"),
        new("Marrakech Mosaic",  "Vibrant markets and hidden riads.",                    "marrakech+market",  "CULTURE"),
        new("Banff Majesty",     "Crystal lakes and towering Rockies.",                  "banff+lake",        "NATURE"),
        new("Maldives Drift",    "Overwater luxury in turquoise paradise.",              "maldives+ocean",    "LUXURY"),
        new("Cape Town Edge",    "Where mountains meet the sea.",                        "cape+town+mountain","SCENIC"),
    ];
}
