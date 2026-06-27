namespace MSDynamicsCRM.API.Models;

public record ChatRequest(
    string Message,
    string? SessionId = null,
    string? SystemPrompt = null);
