using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using MSDynamicsCRM.API.Models;
using OllamaSharp;

// Alias resolves ambiguity between Microsoft.Extensions.AI.ChatResponse and our own ChatResponse model
using AiChatResponse = Microsoft.Extensions.AI.ChatResponse;
using AppChatResponse = MSDynamicsCRM.API.Models.ChatResponse;

namespace MSDynamicsCRM.API.Services;

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly string _mcpServerUrl;
    private readonly string _ollamaUrl;
    private readonly string _ollamaModel;

    private const string DefaultSystemPrompt =
        "You are a helpful Dynamics 365 CRM assistant. " +
        "Use the available tools to answer questions about contacts, accounts, " +
        "opportunities, cases, and the sales pipeline. " +
        "Always call a tool when data is requested rather than guessing. " +
        "Format numbers as currency or percentages where appropriate and keep responses concise.";

    public ChatService(IConfiguration config, ILogger<ChatService> logger)
    {
        _logger = logger;
        _mcpServerUrl = config["McpServer:Url"] ?? "http://localhost:5100/mcp";
        _ollamaUrl    = config["Ollama:BaseUrl"]  ?? "http://localhost:11434"; //sdk does not need /api/chat
        _ollamaModel  = config["Ollama:Model"]    ?? "llama3:latest";
    }

    public async Task<AppChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // ── 1. Connect to the MCP server and get CRM tools ──────────────────
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(_mcpServerUrl),
                Name     = "MsDynamics365MCPServer"
            });

            await using var mcpClient = await McpClient.CreateAsync(
                transport, cancellationToken: cancellationToken);

            var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Loaded {Count} tools from MCP server: {Names}",
                mcpTools.Count,
                string.Join(", ", mcpTools.Select(t => t.Name)));

            // ── 2. Build Ollama IChatClient with automatic function-invocation ──
            // UseFunctionInvocation() middleware drives the agent loop:
            // model response → detect tool calls → execute → append result → repeat
            IChatClient agentClient = ((IChatClient)new OllamaApiClient(new Uri(_ollamaUrl), _ollamaModel))
                .AsBuilder()
                .UseFunctionInvocation()
                .Build();

            // ── 3. Build conversation and run the agent ──────────────────────────
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, request.SystemPrompt ?? DefaultSystemPrompt),
                new(ChatRole.User,   request.Message)
            };

             //McpClientTool extends AIFunction (which extends AITool) — direct assignment works
            var chatOptions = new ChatOptions
            {
                Tools = [.. mcpTools],
                ToolMode = ChatToolMode.Auto
            };

            AiChatResponse aiResponse = await agentClient.GetResponseAsync(
                messages, chatOptions, cancellationToken);

            var responseText = aiResponse.Messages.LastOrDefault()?.Text
                ?? "(No response generated)";

            // Report which tools the model chose to call (from assistant function-call messages)
            var toolsInvoked = aiResponse.Messages
                .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
                .Select(fc => fc.Name)
                .Distinct()
                .ToList();

            return new AppChatResponse(
                Response:     responseText,
                IsSuccess:    true,
                ToolsInvoked: toolsInvoked,
                Model:        _ollamaModel);
        }
        catch (HttpRequestException ex) when (
            ex.Message.Contains("refused", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("connect", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Could not connect to Ollama at {Url}", _ollamaUrl);
            return new AppChatResponse(
                Response:  string.Empty,
                IsSuccess: false,
                Error: $"Ollama is not reachable at {_ollamaUrl}. " +
                       $"Ensure Ollama is running and the '{_ollamaModel}' model is pulled (ollama pull {_ollamaModel}).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat request failed: {Message}", request.Message);
            return new AppChatResponse(
                Response:  string.Empty,
                IsSuccess: false,
                Error:     ex.Message);
        }
    }
}
