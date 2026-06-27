using MSDynamicsCRM.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MSDynamicsCRM AI API", Version = "v1" });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ChatService uses Ollama (via IChatClient / OllamaSharp) and connects to MCPServer
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MSDynamicsCRM AI API v1"));
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
