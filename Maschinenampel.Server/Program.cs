using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JSON Serializer
builder.Services.AddControllers().AddNewtonsoftJson(options=>
options.SerializerSettings.ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore).AddNewtonsoftJson(options=>options.SerializerSettings.ContractResolver=new DefaultContractResolver());

//|DataDirectory|-Platzhalter für Relativen-Datenbankpfad in appsettings.json initalisieren
AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory()); //C:\\Users\\fynnb\\source\\repos\\Maschinenampel\\Maschinenampel.Server


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

//Enable CORS
app.UseCors(c=>c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//Aktiviert die Middleware für den Zugriff auf statische Dateien
app.UseStaticFiles(new StaticFileOptions
{
    // Definiert einen FileProvider, der auf das Verzeichnis "Datenbank/img" verweist
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Datenbank/img")
    ),
    // Legt fest, unter welchem Pfad die Dateien im Web verfügbar sind
    RequestPath = "/Datenbank/img"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
