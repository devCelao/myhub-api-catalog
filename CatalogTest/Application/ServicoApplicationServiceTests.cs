using CatalogApplication.Services;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;
using DomainObjects.Data;
using FluentAssertions;
using Moq;

namespace CatalogTest.Application;

public class ServicoApplicationServiceTests
{
    private readonly Mock<IServicoRepository> _servicoRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ServicoApplicationService _service;

    public ServicoApplicationServiceTests()
    {
        _servicoRepoMock = new Mock<IServicoRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _uowMock = new Mock<IUnitOfWork>();

        _servicoRepoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);
        _currentUserMock.Setup(u => u.NomeUsuario).Returns("test-user");

        _service = new ServicoApplicationService(_servicoRepoMock.Object, _currentUserMock.Object);
    }

    private static ServicoRequest CriarRequest(string cod = "SVC-001", string nome = "Servico Teste",
        string? desc = null)
    {
        return new ServicoRequest
        {
            CodServico = cod,
            NomeServico = nome,
            Descricao = desc
        };
    }

    #region CriarServicoAsync

    [Fact]
    public async Task CriarServico_ServicoNovo_DeveRetornarSucesso()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.ObterPorNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.CriarServicoAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CodServico.Should().Be("SVC-001");
    }

    [Fact]
    public async Task CriarServico_CodigoJaExiste_DeveRetornarFalha()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Existente", "admin"));

        var result = await _service.CriarServicoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("existe"));
    }

    [Fact]
    public async Task CriarServico_NomeJaExiste_DeveRetornarFalha()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.ObterPorNomeAsync("Servico Teste"))
            .ReturnsAsync(new Servico("SVC-002", "Servico Teste", "admin"));

        var result = await _service.CriarServicoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("nome"));
    }

    [Fact]
    public async Task CriarServico_ComDescricao_DeveDefinirDescricao()
    {
        var request = CriarRequest(desc: "Uma descricao");
        Servico? servicoCriado = null;

        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.ObterPorNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.AdicionarAsync(It.IsAny<Servico>()))
            .Callback<Servico>(s => servicoCriado = s);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        await _service.CriarServicoAsync(request);

        servicoCriado.Should().NotBeNull();
        servicoCriado!.Descricao.Should().Be("Uma descricao");
    }

    [Fact]
    public async Task CriarServico_DeveUsarUsuarioDoToken()
    {
        _currentUserMock.Setup(u => u.NomeUsuario).Returns("marcelo");
        var request = CriarRequest();
        Servico? servicoCriado = null;

        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.ObterPorNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);
        _servicoRepoMock.Setup(r => r.AdicionarAsync(It.IsAny<Servico>()))
            .Callback<Servico>(s => servicoCriado = s);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        await _service.CriarServicoAsync(request);

        servicoCriado!.CriadoPor.Should().Be("marcelo");
    }

    #endregion

    #region AtualizarServicoAsync

    [Fact]
    public async Task AtualizarServico_Existente_DeveRetornarSucesso()
    {
        var request = CriarRequest(nome: "Nome Atualizado");
        var servico = new Servico("SVC-001", "Original", "admin");
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-001"))
            .ReturnsAsync(servico);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.AtualizarServicoAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.NomeServico.Should().Be("Nome Atualizado");
    }

    [Fact]
    public async Task AtualizarServico_NaoEncontrado_DeveRetornarNotFound()
    {
        var request = CriarRequest();
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var result = await _service.AtualizarServicoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ObterServicoAsync

    [Fact]
    public async Task ObterServico_Existente_DeveRetornarServico()
    {
        var dto = new ServicoDto { CodServico = "SVC-001", NomeServico = "Teste" };
        _servicoRepoMock.Setup(r => r.ObterServicoDtoAsync("SVC-001"))
            .ReturnsAsync(dto);

        var result = await _service.ObterServicoAsync("SVC-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }

    [Fact]
    public async Task ObterServico_NaoExiste_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterServicoDtoAsync(It.IsAny<string>()))
            .ReturnsAsync((ServicoDto?)null);

        var result = await _service.ObterServicoAsync("SVC-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ListarServicosAsync

    [Fact]
    public async Task ListarServicos_DeveRetornarTodos()
    {
        var servicos = new List<ServicoDto>
        {
            new() { CodServico = "SVC-001", NomeServico = "A" },
            new() { CodServico = "SVC-002", NomeServico = "B" }
        };
        _servicoRepoMock.Setup(r => r.ListarServicosAsync()).ReturnsAsync(servicos);

        var result = await _service.ListarServicosAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    #endregion

    #region ExcluirServicoAsync

    [Fact]
    public async Task ExcluirServico_Existente_DeveRetornarSucesso()
    {
        var servico = new Servico("SVC-001", "Teste", "admin");
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync("SVC-001"))
            .ReturnsAsync(servico);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.ExcluirServicoAsync("SVC-001");

        result.IsSuccess.Should().BeTrue();
        _servicoRepoMock.Verify(r => r.Remover(servico), Times.Once);
    }

    [Fact]
    public async Task ExcluirServico_NaoEncontrado_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var result = await _service.ExcluirServicoAsync("SVC-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion
}
