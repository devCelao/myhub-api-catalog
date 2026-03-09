using CatalogInfrastructure.Context;
using CatalogInfrastructure.Repositories;
using CatalogApplication.Services;
using Microsoft.OpenApi.Models;
using MicroserviceCore.Configuration;
using MicroserviceCore.Middleware;
using MessageBus.Configuration;

var builder = WebApplication.CreateBuilder(args);
bool isDevelopment = builder.Environment.IsDevelopment();
builder.AddJsonFile();

if (isDevelopment)
{
    builder.Configuration.AddUserSecrets<Program>();
}
// Add o contexto de banco de dados
builder.Services.AddContextCustomConfiguration<CatalogContext>(builder.Configuration);

// Add os controllers
builder.Services.AddExtensionConfiguration();

// Add configurações do cors
builder.Services.AddCustomCors();

// Add configurações do swagger
var infoApi = new OpenApiInfo
{
    Version = "v1",
    Title = "Catalog API",
    Description = "Catalogo de planos e serviços",
    Contact = new()
    {
        Name = "",
        Email = "",
        Url = new Uri(uriString: "https://www.linkedin.com/in/fernandes-marcelo/")
    },
    License = new()
    {
        Name = "MIT",
        Url = new Uri(uriString: "https://opensorce.org/licenses/MIT")
    }
};
builder.Services.AddSwaggerConfiguration(infoApi);

// ========== Autenticação JWT com JWKS (API Consumer) ==========
builder.Services.AddJwtAuthenticationConsumer(builder.Configuration);

// ========== MessageBus para comunicação com AuthAPI ==========
builder.Services.AddMessageBusRequest(builder.Configuration);

// Add os services usados na aplicação
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();

builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
var app = builder.Build();

app.UseSwaggerConfiguration(isDevelopment);

app.UseCustomCors();

app.UseHttpsRedirection();

// Autenticação com refresh automático de token via cookie
app.UseAuthenticationWithTokenRefresh();

app.MapControllers();

app.UseCustomError();

app.Run();

// Torna a classe Program acessível para testes de integração
public partial class Program { }