using Microsoft.AspNetCore.Mvc;
using MSDynamicsCRM.API.Models;
using MSDynamicsCRM.API.Services;

namespace MSDynamicsCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the Dynamics 365 CRM AI assistant.
    /// The agent uses Ollama (Llama) and calls MCP server tools to answer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty." });

        _logger.LogInformation("Chat request received: {Message}", request.Message);

        var response = await _chatService.ChatAsync(request, cancellationToken);

        if (!response.IsSuccess)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);

        return Ok(response);
    }

    /// <summary>Health check endpoint.</summary>
    [HttpGet("health")]
    public IActionResult Health() =>
        Ok(new { status = "ok", timestamp = DateTime.UtcNow });
}
