using System.Security.Claims;
using System.Text.Encodings.Web;
using CatalogApplication.Services;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogInfrastructure.Context;
using CatalogInfrastructure.Repositories;
using DomainObjects.Data;
using MessageBus.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CatalogTest.Integration;

public class CatalogApiFactory : WebApplicationFactory<Program>
{
    public Mock<IPlanoRepository> PlanoRepoMock { get; } = new();
    public Mock<IServicoRepository> ServicoRepoMock { get; } = new();
    public Mock<IFuncaoRepository> FuncaoRepoMock { get; } = new();
    public Mock<IPlanoServicoRepository> PlanoServicoRepoMock { get; } = new();
    public Mock<IUnitOfWork> UnitOfWorkMock { get; } = new();

    public CatalogApiFactory()
    {
        PlanoRepoMock.Setup(r => r.UnitOfWork).Returns(UnitOfWorkMock.Object);
        ServicoRepoMock.Setup(r => r.UnitOfWork).Returns(UnitOfWorkMock.Object);
        FuncaoRepoMock.Setup(r => r.UnitOfWork).Returns(UnitOfWorkMock.Object);
        PlanoServicoRepoMock.Setup(r => r.UnitOfWork).Returns(UnitOfWorkMock.Object);
        UnitOfWorkMock.Setup(u => u.Commit()).ReturnsAsync(true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Connection:ConnectionString"] = "Server=localhost;Database=test;User=root;Password=test;",
                ["MessageQueueConnection:MessageBus"] = "amqp://guest:guest@localhost:5672/test",
                ["Jwt:Audience"] = "test-audience"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var svc in hostedServices)
                services.Remove(svc);

            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(CatalogContext)
                          || d.ServiceType == typeof(DbContextOptions<CatalogContext>)
                          || d.ServiceType.FullName?.Contains("DbContextPool") == true
                          || d.ServiceType.FullName?.Contains("DbContextOptions") == true)
                .ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<CatalogContext>(opts =>
                opts.UseInMemoryDatabase("CatalogTestDb"));

            services.RemoveAll<IBusMessage>();
            services.AddSingleton(new Mock<IBusMessage>().Object);

            services.RemoveAll<IPlanoRepository>();
            services.RemoveAll<IServicoRepository>();
            services.RemoveAll<IFuncaoRepository>();
            services.RemoveAll<IPlanoServicoRepository>();

            services.AddSingleton(PlanoRepoMock.Object);
            services.AddSingleton(ServicoRepoMock.Object);
            services.AddSingleton(FuncaoRepoMock.Object);
            services.AddSingleton(PlanoServicoRepoMock.Object);

            services.RemoveAll<ICurrentUserService>();
            services.AddScoped<ICurrentUserService>(_ =>
            {
                var mock = new Mock<ICurrentUserService>();
                mock.Setup(u => u.NomeUsuario).Returns("integration-test-user");
                mock.Setup(u => u.IsAuthenticated).Returns(true);
                mock.Setup(u => u.UsuarioId).Returns(Guid.NewGuid());
                return mock.Object;
            });

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(o =>
            {
                o.DefaultAuthenticateScheme = "Test";
                o.DefaultChallengeScheme = "Test";
            });
        });
    }

    public void SetupPlanoExistente(string cod = "PLN-001", string nome = "Plano Teste",
        decimal valor = 99.90m)
    {
        var codigo = new CodigoPlano(cod);
        var plano = new Plano(codigo, nome, new Dinheiro(valor), "admin");
        var dto = new PlanoDto
        {
            CodPlano = cod.ToUpperInvariant(),
            NomePlano = nome,
            IndAtivo = true,
            IndGeraCobranca = true,
            ValorBase = valor,
            Servicos = []
        };

        PlanoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        PlanoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        PlanoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(dto);
    }

    public void SetupPlanoNaoExiste()
    {
        PlanoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);
        PlanoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);
        PlanoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((PlanoDto?)null);
    }

    public void SetupServicoExistente(string cod = "SVC-001", string nome = "Servico Teste")
    {
        var servico = new Servico(cod, nome, "admin");
        var dto = new ServicoDto { CodServico = cod, NomeServico = nome };

        ServicoRepoMock.Setup(r => r.ObterPorCodigoAsync(cod)).ReturnsAsync(servico);
        ServicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(cod)).ReturnsAsync(servico);
        ServicoRepoMock.Setup(r => r.ObterPorNomeAsync(It.IsAny<string>())).ReturnsAsync((Servico?)null);
        ServicoRepoMock.Setup(r => r.ObterServicoDtoAsync(cod)).ReturnsAsync(dto);
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "integration-test-user"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
