using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using FluentAssertions;
using Moq;

namespace CatalogTest.Integration;

public class ServicoControllerTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;
    private readonly CatalogApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ServicoControllerTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/catalogo/servicos

    [Fact]
    public async Task ListarServicos_DeveRetornar200()
    {
        _factory.ServicoRepoMock.Setup(r => r.ListarServicosAsync())
            .ReturnsAsync([
                new ServicoDto { CodServico = "SVC-001", NomeServico = "Servico A" },
                new ServicoDto { CodServico = "SVC-002", NomeServico = "Servico B" }
            ]);

        var response = await _client.GetAsync("/api/catalogo/servicos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ServicoDto>>>(JsonOpts);
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(2);
    }

    #endregion

    #region GET /api/catalogo/servicos/{codServico}

    [Fact]
    public async Task ObterServico_Existente_DeveRetornar200()
    {
        _factory.SetupServicoExistente();

        var response = await _client.GetAsync("/api/catalogo/servicos/SVC-001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObterServico_NaoExistente_DeveRetornar404()
    {
        _factory.ServicoRepoMock.Setup(r => r.ObterServicoDtoAsync("SVC-999"))
            .ReturnsAsync((ServicoDto?)null);

        var response = await _client.GetAsync("/api/catalogo/servicos/SVC-999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/catalogo/servicos

    [Fact]
    public async Task CriarServico_DadosValidos_DeveRetornar201()
    {
        _factory.ServicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-NEW"))
            .ReturnsAsync((Servico?)null);
        _factory.ServicoRepoMock.Setup(r => r.ObterPorNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var request = new ServicoRequest
        {
            CodServico = "SVC-NEW",
            NomeServico = "Novo Servico",
            Descricao = "Descricao"
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/servicos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CriarServico_NomeVazio_DeveRetornar400()
    {
        var request = new ServicoRequest
        {
            CodServico = "SVC-BAD",
            NomeServico = "",
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/servicos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/catalogo/servicos/{codServico}

    [Fact]
    public async Task AtualizarServico_CodigoDiferente_DeveRetornar400()
    {
        var request = new ServicoRequest
        {
            CodServico = "SVC-001",
            NomeServico = "Servico Atualizado"
        };

        var response = await _client.PutAsJsonAsync("/api/catalogo/servicos/SVC-OUTRO", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarServico_Existente_DeveRetornar200()
    {
        _factory.SetupServicoExistente();

        var request = new ServicoRequest
        {
            CodServico = "SVC-001",
            NomeServico = "Servico Atualizado"
        };

        var response = await _client.PutAsJsonAsync("/api/catalogo/servicos/SVC-001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /api/catalogo/servicos/{codServico}

    [Fact]
    public async Task ExcluirServico_Existente_DeveRetornar204()
    {
        var servico = new Servico("SVC-DEL", "Para excluir", "admin");
        _factory.ServicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-DEL"))
            .ReturnsAsync(servico);

        var response = await _client.DeleteAsync("/api/catalogo/servicos/SVC-DEL");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ExcluirServico_NaoExistente_DeveRetornar404()
    {
        _factory.ServicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-999"))
            .ReturnsAsync((Servico?)null);

        var response = await _client.DeleteAsync("/api/catalogo/servicos/SVC-999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
