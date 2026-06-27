using MSDynamicsCRM.API.Models;

namespace MSDynamicsCRM.API.Services;

public interface IChatService
{
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
