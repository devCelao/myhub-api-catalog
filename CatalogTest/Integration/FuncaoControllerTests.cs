using System.Net;
using System.Net.Http.Json;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using FluentAssertions;
using Moq;

namespace CatalogTest.Integration;

public class FuncaoControllerTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;
    private readonly CatalogApiFactory _factory;

    public FuncaoControllerTests(CatalogApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/catalogo/servicos/{codServico}/funcoes

    [Fact]
    public async Task ListarFuncoes_ServicoExistente_DeveRetornar200()
    {
        _factory.SetupServicoExistente();
        _factory.FuncaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([new FuncaoDto
            {
                CodFuncao = "FN-001", CodServico = "SVC-001", Label = "Funcao A",
                NumOrdem = 1, IndAtivo = true
            }]);

        var response = await _client.GetAsync("/api/catalogo/servicos/SVC-001/funcoes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarFuncoes_ServicoNaoExiste_DeveRetornar404()
    {
        _factory.ServicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-999"))
            .ReturnsAsync((Servico?)null);

        var response = await _client.GetAsync("/api/catalogo/servicos/SVC-999/funcoes");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/catalogo/servicos/{codServico}/funcoes

    [Fact]
    public async Task CriarFuncao_DadosValidos_DeveRetornar201()
    {
        _factory.SetupServicoExistente();
        _factory.FuncaoRepoMock.Setup(r => r.ObterPorCodigoAsync("FN-NEW"))
            .ReturnsAsync((Funcao?)null);
        _factory.FuncaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([]);

        var request = new FuncaoRequest
        {
            CodFuncao = "FN-NEW",
            Label = "Nova Funcao",
            Descricao = "Desc",
            NumOrdem = 1,
            IndAtivo = true
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/servicos/SVC-001/funcoes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CriarFuncao_LabelVazio_DeveRetornar400()
    {
        var request = new FuncaoRequest
        {
            CodFuncao = "FN-BAD",
            Label = "",
            NumOrdem = 1,
            IndAtivo = true
        };

        var response = await _client.PostAsJsonAsync("/api/catalogo/servicos/SVC-001/funcoes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/catalogo/servicos/{codServico}/funcoes/{codFuncao}

    [Fact]
    public async Task AtualizarFuncao_CodigoDiferente_DeveRetornar400()
    {
        var request = new FuncaoRequest
        {
            CodFuncao = "FN-001",
            Label = "Atualizado",
            NumOrdem = 1,
            IndAtivo = true
        };

        var response = await _client.PutAsJsonAsync(
            "/api/catalogo/servicos/SVC-001/funcoes/FN-OUTRO", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AtualizarFuncao_DadosValidos_DeveRetornar200()
    {
        _factory.SetupServicoExistente();
        _factory.FuncaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-001"))
            .ReturnsAsync(new Funcao("FN-001", "SVC-001", "Original", "admin"));
        _factory.FuncaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([]);

        var request = new FuncaoRequest
        {
            CodFuncao = "FN-001",
            Label = "Atualizado",
            NumOrdem = 1,
            IndAtivo = true
        };

        var response = await _client.PutAsJsonAsync(
            "/api/catalogo/servicos/SVC-001/funcoes/FN-001", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region DELETE /api/catalogo/servicos/{codServico}/funcoes/{codFuncao}

    [Fact]
    public async Task ExcluirFuncao_Existente_DeveRetornar204()
    {
        var servico = new Servico("SVC-001", "Servico", "admin");
        var funcao = new Funcao("FN-DEL", "SVC-001", "Para excluir", "admin");
        _factory.ServicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-001"))
            .ReturnsAsync(servico);
        _factory.FuncaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-DEL"))
            .ReturnsAsync(funcao);

        var response = await _client.DeleteAsync("/api/catalogo/servicos/SVC-001/funcoes/FN-DEL");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}
