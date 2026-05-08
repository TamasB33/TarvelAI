namespace TarvelAI.DTOs.AI;

public class ConversationTurn
{
    public string Role { get; set; } = ""; // "User" or "AI"
    public string Text { get; set; } = "";
    public bool IsRecommendation { get; set; }
}
