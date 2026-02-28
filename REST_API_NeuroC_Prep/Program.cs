using REST_API_NeuroC_Prep.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NeuroC Vision API",
        Version = "v1",
        Description = "REST-API für OpenCV-basierte Bildverarbeitung via NeuroCComVision.dll. " +
                      "Bietet Kamerasteuerung, Farb-/Gesichts-/Kreis-/Kantenerkennung."
    });
});

// VisionService als Singleton — eine Kamera, eine Instanz
builder.Services.AddSingleton<VisionService>();

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "NeuroC Vision API";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Kamera beim Beenden sauber freigeben
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    app.Services.GetRequiredService<VisionService>().Dispose();
});

app.Run();
