using CatalogAPI.Services;
using CatalogApplication.Responders;
using CatalogApplication.Services;
using CatalogDomain.Validators;
using CatalogInfrastructure.Context;
using CatalogInfrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MessageBus.Configuration;
using MicroserviceCore.Configuration;
using MicroserviceCore.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
bool isDevelopment = builder.Environment.IsDevelopment();
builder.AddJsonFile();

if (isDevelopment)
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddContextCustomConfiguration<CatalogContext>(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<PlanoRequestValidator>();

builder.Services.AddExtensionConfiguration();

builder.Services.AddCustomCors();

var infoApi = new OpenApiInfo
{
    Version = "v1",
    Title = "Catalog API",
    Description = "Catalogo de planos e servicos",
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

builder.Services.AddJwtAuthenticationConsumer(builder.Configuration);

builder.Services.AddMessageBusRequest(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Repositorios
builder.Services.AddScoped<IPlanoRepository, PlanoRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IPlanoServicoRepository, PlanoServicoRepository>();
builder.Services.AddScoped<IFuncaoRepository, FuncaoRepository>();

// Application Services
builder.Services.AddScoped<IPlanoApplicationService, PlanoApplicationService>();
builder.Services.AddScoped<IServicoApplicationService, ServicoApplicationService>();
builder.Services.AddScoped<IPlanoServicoApplicationService, PlanoServicoApplicationService>();
builder.Services.AddScoped<IFuncaoApplicationService, FuncaoApplicationService>();

builder.Services.AddMessageBusResponder<CatalogResponder>(builder.Configuration);

var app = builder.Build();

app.UseSwaggerConfiguration(isDevelopment);

app.UseCustomCors();

app.UseHttpsRedirection();

app.UseAuthenticationWithTokenRefresh();

app.MapControllers();

app.UseCustomError();

app.Run();

public partial class Program { }
