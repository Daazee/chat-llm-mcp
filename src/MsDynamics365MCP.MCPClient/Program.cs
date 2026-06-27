var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Named HttpClient pointing at MSDynamicsCRM.API
builder.Services.AddHttpClient("MsDynamicsCRMApi", client =>
{
    var url = builder.Configuration["MsDynamicsCRMApi:Url"] ?? "http://localhost:5200";
    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(120); // allow time for LLM inference
});

var app = builder.Build();

app.UseDefaultFiles();   // serves index.html for "/"
app.UseStaticFiles();    // serves wwwroot/

app.UseAuthorization();
app.MapControllers();

app.Run();
