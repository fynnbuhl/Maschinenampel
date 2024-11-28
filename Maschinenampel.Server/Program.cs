using Maschinenampel.Server.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Enable Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON Serializer settings
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ContractResolver = new DefaultContractResolver());

//|DataDirectory|-Platzhalter für Relativen-Datenbankpfad in appsettings.json initialisieren
AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());

// Füge den OPC-Service hinzu
builder.Services.AddSingleton<OPC_Service>();

var app = builder.Build();

// Enable WebSockets (WebSocket wird hier nur für WebSocket-Anfragen aktiviert)
app.UseWebSockets();

// CORS
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Aktiviert die Middleware für den Zugriff auf statische Dateien
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseDefaultFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Map controllers to endpoints
app.MapControllers();

// Fallback für die SPA (Single Page Application), falls keine API-Route gefunden wird
app.MapFallbackToFile("/index.html");

app.Run();
