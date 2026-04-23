namespace TarvelAI.Auth;

public record UserInfo(bool IsAuthenticated, string? Email, string? UserName);
