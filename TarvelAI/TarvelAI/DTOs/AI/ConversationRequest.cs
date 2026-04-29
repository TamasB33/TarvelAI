namespace TarvelAI.DTOs.AI;

public class ConversationRequest
{
    public List<ConversationTurn> Conversation { get; set; } = new();
}
