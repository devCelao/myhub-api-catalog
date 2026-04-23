using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using FluentAssertions;
using Moq;

namespace CatalogTest.Integration;

public class PlanoControllerTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;
    private readonly CatalogApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public PlanoControllerTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/catalogo/planos

    [Fact]
    public async Task ListarPlanos_DeveRetornar200()
    {
        _factory.PlanoRepoMock.Setup(r => r.ListarPlanosComServicosAsync())
            .ReturnsAsync([new PlanoDto
            {
                CodPlano = "PLN-001", NomePlano = "Teste", IndAtivo = true,
                IndGeraCobranca = true, ValorBase = 99.90m, Servicos = []
            }]);

        var response = await _client.GetAsync("/api/catalogo/planos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PlanoDto>>>(JsonOpts);
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(1);
    }

    #endregion

    #region GET /api/catalogo/planos/ativos

    [Fact]
    public async Task ListarPlanosAtivos_DeveRetornar200()
    {
        _factory.PlanoRepoMock.Setup(r => r.ListarPlanosAtivosComServicosAsync())
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/api/catalogo/planos/ativos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/catalogo/planos/{codPlano}

    [Fact]
    public async Task ObterPlano_Existente_DeveRetornar200()
    {
        _factory.SetupPlanoExistente();

        var response = await _client.GetAsync("/api/catalogo/planos/PLN-001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PlanoDto>>(JsonOpts);
        body!.Success.Should().BeTrue();
        body.Data!.CodPlano.Should().Be("PLN-001");
    }

    [Fact]
    public async Task ObterPlano_NaoExistente_DeveRetornar404()
    {
        _factory.SetupPlanoNaoExiste();

        var response = await _client.GetAsync("/api/catalogo/planos/PLN-999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/catalogo/planos

    [Fact]
    public async Task CriarPlano_DadosValidos_DeveRetornar201()
    {
        _factory.SetupPlanoNaoExiste();
        _factory.PlanoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(new PlanoDto
            {
                CodPlano = "PLN-NEW", NomePlano = "Novo Plano", IndAtivo = true,
                IndGeraCobranca = true, ValorBase = 50m, Servicos = []
            });

        var request = new PlanoRequest
        {
            CodPlano = "PLN-NEW",
            NomePlano = "Novo Plano",
            ValorBase = 50m,
            IndAtivo = true,
            IndGeraCobranca = true
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/planos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CriarPlano_NomeVazio_DeveRetornar400()
    {
        var request = new PlanoRequest
        {
            CodPlano = "PLN-TEST",
            NomePlano = "",
            ValorBase = 10m,
            IndAtivo = true,
            IndGeraCobranca = true
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/planos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPlano_CodigoComEspacos_DeveRetornar400()
    {
        var request = new PlanoRequest
        {
            CodPlano = "PLN 001",
            NomePlano = "Plano Invalido",
            ValorBase = 10m,
            IndAtivo = true,
            IndGeraCobranca = true
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/planos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/catalogo/planos/{codPlano}

    [Fact]
    public async Task AtualizarPlano_DadosValidos_DeveRetornar200()
    {
        _factory.SetupPlanoExistente();

        var request = new PlanoRequest
        {
            CodPlano = "PLN-001",
            NomePlano = "Plano Atualizado",
            ValorBase = 150m,
            IndAtivo = true,
            IndGeraCobranca = true
        };

        var response = await _client.PutAsJsonAsync("/api/catalogo/planos/PLN-001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AtualizarPlano_CodigoDiferenteNaUrl_DeveRetornar400()
    {
        var request = new PlanoRequest
        {
            CodPlano = "PLN-001",
            NomePlano = "Plano",
            ValorBase = 10m,
            IndAtivo = true,
            IndGeraCobranca = true
        };

        var response = await _client.PutAsJsonAsync("/api/catalogo/planos/PLN-OUTRO", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/catalogo/planos/{codPlano}

    [Fact]
    public async Task ExcluirPlano_Existente_DeveRetornar204()
    {
        var plano = new Plano(new CodigoPlano("PLN-DEL"), "Excluir", new Dinheiro(10), "admin");
        _factory.PlanoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);

        var response = await _client.DeleteAsync("/api/catalogo/planos/PLN-DEL");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExcluirPlano_NaoExistente_DeveRetornar404()
    {
        _factory.PlanoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);

        var response = await _client.DeleteAsync("/api/catalogo/planos/PLN-999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
