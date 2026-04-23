using CatalogApplication.Services;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;
using DomainObjects.Data;
using FluentAssertions;
using Moq;

namespace CatalogTest.Application;

public class FuncaoApplicationServiceTests
{
    private readonly Mock<IServicoRepository> _servicoRepoMock;
    private readonly Mock<IFuncaoRepository> _funcaoRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly FuncaoApplicationService _service;

    public FuncaoApplicationServiceTests()
    {
        _servicoRepoMock = new Mock<IServicoRepository>();
        _funcaoRepoMock = new Mock<IFuncaoRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _uowMock = new Mock<IUnitOfWork>();

        _funcaoRepoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);
        _currentUserMock.Setup(u => u.NomeUsuario).Returns("test-user");

        _service = new FuncaoApplicationService(
            _servicoRepoMock.Object, _funcaoRepoMock.Object, _currentUserMock.Object);
    }

    private static FuncaoRequest CriarRequest(string cod = "FN-001", string label = "Funcao Teste",
        int ordem = 1, bool ativo = true)
    {
        return new FuncaoRequest
        {
            CodFuncao = cod,
            Label = label,
            Descricao = "Descricao",
            Icone = "fa-home",
            NumOrdem = ordem,
            IndAtivo = ativo
        };
    }

    #region CriarFuncaoAsync

    [Fact]
    public async Task CriarFuncao_DadosValidos_DeveRetornarSucesso()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoAsync("FN-001"))
            .ReturnsAsync((Funcao?)null);
        _funcaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([]);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.CriarFuncaoAsync("SVC-001", request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.CodFuncao.Should().Be("FN-001");
        result.Data.Label.Should().Be("Funcao Teste");
    }

    [Fact]
    public async Task CriarFuncao_ServicoNaoExiste_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var result = await _service.CriarFuncaoAsync("SVC-999", CriarRequest());

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    [Fact]
    public async Task CriarFuncao_CodigoJaExiste_DeveRetornarFalha()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoAsync("FN-001"))
            .ReturnsAsync(new Funcao("FN-001", "SVC-001", "Existente", "admin"));

        var result = await _service.CriarFuncaoAsync("SVC-001", CriarRequest());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("existe"));
    }

    [Fact]
    public async Task CriarFuncao_OrdemDuplicada_DeveRealocarOrdem()
    {
        var request = CriarRequest(ordem: 1);
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoAsync("FN-001"))
            .ReturnsAsync((Funcao?)null);
        _funcaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([new FuncaoDto { CodFuncao = "FN-X", CodServico = "SVC-001", Label = "X", NumOrdem = 1 }]);

        Funcao? funcaoCriada = null;
        _funcaoRepoMock.Setup(r => r.AdicionarAsync(It.IsAny<Funcao>()))
            .Callback<Funcao>(f => funcaoCriada = f);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        await _service.CriarFuncaoAsync("SVC-001", request);

        funcaoCriada.Should().NotBeNull();
        funcaoCriada!.NumOrdem.Should().Be(2);
    }

    #endregion

    #region AtualizarFuncaoAsync

    [Fact]
    public async Task AtualizarFuncao_DadosValidos_DeveRetornarSucesso()
    {
        var request = CriarRequest(label: "Label Atualizado");
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-001"))
            .ReturnsAsync(new Funcao("FN-001", "SVC-001", "Original", "admin"));
        _funcaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([]);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.AtualizarFuncaoAsync("SVC-001", request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Label.Should().Be("Label Atualizado");
    }

    [Fact]
    public async Task AtualizarFuncao_FuncaoNaoPertenceAoServico_DeveRetornarFalha()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-001"))
            .ReturnsAsync(new Funcao("FN-001", "SVC-OUTRO", "Label", "admin"));

        var result = await _service.AtualizarFuncaoAsync("SVC-001", request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("pertence"));
    }

    [Fact]
    public async Task AtualizarFuncao_FuncaoNaoEncontrada_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<string>()))
            .ReturnsAsync((Funcao?)null);

        var result = await _service.AtualizarFuncaoAsync("SVC-001", CriarRequest());

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ListarFuncoesDoServicoAsync

    [Fact]
    public async Task ListarFuncoes_ServicoExistente_DeveRetornarLista()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ListarFuncoesDoServicoAsync("SVC-001"))
            .ReturnsAsync([new FuncaoDto { CodFuncao = "FN-001", CodServico = "SVC-001", Label = "A" }]);

        var result = await _service.ListarFuncoesDoServicoAsync("SVC-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListarFuncoes_ServicoNaoExiste_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var result = await _service.ListarFuncoesDoServicoAsync("SVC-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ExcluirFuncaoAsync

    [Fact]
    public async Task ExcluirFuncao_DadosValidos_DeveRetornarSucesso()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-001"))
            .ReturnsAsync(funcao);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.ExcluirFuncaoAsync("SVC-001", "FN-001");

        result.IsSuccess.Should().BeTrue();
        _funcaoRepoMock.Verify(r => r.Remover(funcao), Times.Once);
    }

    [Fact]
    public async Task ExcluirFuncao_FuncaoNaoPertenceAoServico_DeveRetornarFalha()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _funcaoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("FN-001"))
            .ReturnsAsync(new Funcao("FN-001", "SVC-OUTRO", "Label", "admin"));

        var result = await _service.ExcluirFuncaoAsync("SVC-001", "FN-001");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("pertence"));
    }

    #endregion
}
