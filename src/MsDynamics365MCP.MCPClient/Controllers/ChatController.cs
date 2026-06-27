using Microsoft.AspNetCore.Mvc;
using MsDynamics365MCP.MCPClient.Models;
using System.Text;
using System.Text.Json;

namespace MsDynamics365MCP.MCPClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ChatController(IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Proxy endpoint: receives the user message from the browser and forwards it to
    /// MSDynamicsCRM.API for AI processing (Ollama + MCP tool invocation).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message cannot be empty." });

        _logger.LogInformation("Forwarding chat message to MSDynamicsCRM.API: {Message}", request.Message);

        var client = _httpClientFactory.CreateClient("MsDynamicsCRMApi");

        var payload = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        try
        {
            var apiResponse = await client.PostAsync("api/chat", payload, cancellationToken);
            var body = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("MSDynamicsCRM.API returned {Status}: {Body}",
                    (int)apiResponse.StatusCode, body);
                return StatusCode((int)apiResponse.StatusCode,
                    new { error = "AI service error. Check that Ollama and MSDynamicsCRM.API are running.", detail = body });
            }

            // Pass the response body through directly as JSON
            return Content(body, "application/json");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not reach MSDynamicsCRM.API");
            return StatusCode(503, new
            {
                error = "The AI service (MSDynamicsCRM.API) is not reachable. Please start it on http://localhost:5200.",
                response = (string?)null,
                isSuccess = false
            });
        }
    }
}
