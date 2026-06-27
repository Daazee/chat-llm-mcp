namespace MSDynamicsCRM.API.Models;

public record ChatResponse(
    string Response,
    bool IsSuccess,
    string? Error = null,
    IReadOnlyList<string>? ToolsInvoked = null,
    string? Model = null);
