using CatalogApplication.Services;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogInfrastructure.Repositories;
using DomainObjects.Data;
using FluentAssertions;
using Moq;

namespace CatalogTest.Application;

public class PlanoApplicationServiceTests
{
    private readonly Mock<IPlanoRepository> _planoRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PlanoApplicationService _service;

    public PlanoApplicationServiceTests()
    {
        _planoRepoMock = new Mock<IPlanoRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _uowMock = new Mock<IUnitOfWork>();

        _planoRepoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);
        _currentUserMock.Setup(u => u.NomeUsuario).Returns("test-user");

        _service = new PlanoApplicationService(_planoRepoMock.Object, _currentUserMock.Object);
    }

    private static PlanoRequest CriarRequest(string cod = "PLN-001", string nome = "Plano Teste",
        decimal valor = 99.90m, bool ativo = true, bool cobranca = true)
    {
        return new PlanoRequest
        {
            CodPlano = cod,
            NomePlano = nome,
            ValorBase = valor,
            IndAtivo = ativo,
            IndGeraCobranca = cobranca
        };
    }

    private static PlanoDto CriarPlanoDto(string cod = "PLN-001", string nome = "Plano Teste")
    {
        return new PlanoDto
        {
            CodPlano = cod,
            NomePlano = nome,
            IndAtivo = true,
            IndGeraCobranca = true,
            ValorBase = 99.90m,
            Servicos = []
        };
    }

    #region CriarPlanoAsync

    [Fact]
    public async Task CriarPlano_PlanoNovo_DeveRetornarSucesso()
    {
        var request = CriarRequest();
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(CriarPlanoDto());

        var result = await _service.CriarPlanoAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CodPlano.Should().Be("PLN-001");
        _planoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Plano>()), Times.Once);
    }

    [Fact]
    public async Task CriarPlano_PlanoJaExiste_DeveRetornarFalha()
    {
        var request = CriarRequest();
        var planoExistente = new Plano(new CodigoPlano("PLN-001"), "Existente", new Dinheiro(10), "admin");
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(planoExistente);

        var result = await _service.CriarPlanoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("já existe"));
    }

    [Fact]
    public async Task CriarPlano_FalhaNoCommit_DeveRetornarFalha()
    {
        var request = CriarRequest();
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(false);

        var result = await _service.CriarPlanoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Erro ao salvar"));
    }

    [Fact]
    public async Task CriarPlano_DeveUsarUsuarioDoCurrentUserService()
    {
        _currentUserMock.Setup(u => u.NomeUsuario).Returns("marcelo.fernandes");
        var request = CriarRequest();
        Plano? planoCriado = null;

        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);
        _planoRepoMock.Setup(r => r.AdicionarAsync(It.IsAny<Plano>()))
            .Callback<Plano>(p => planoCriado = p);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(CriarPlanoDto());

        await _service.CriarPlanoAsync(request);

        planoCriado.Should().NotBeNull();
        planoCriado!.CriadoPor.Should().Be("marcelo.fernandes");
    }

    #endregion

    #region AtualizarPlanoAsync

    [Fact]
    public async Task AtualizarPlano_PlanoExistente_DeveRetornarSucesso()
    {
        var request = CriarRequest(nome: "Nome Atualizado");
        var plano = new Plano(new CodigoPlano("PLN-001"), "Original", new Dinheiro(50), "admin");

        _planoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(CriarPlanoDto(nome: "Nome Atualizado"));

        var result = await _service.AtualizarPlanoAsync(request);

        result.IsSuccess.Should().BeTrue();
        _planoRepoMock.Verify(r => r.Atualizar(plano), Times.Once);
    }

    [Fact]
    public async Task AtualizarPlano_PlanoNãoEncontrado_DeveRetornarNotFound()
    {
        var request = CriarRequest();
        _planoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);

        var result = await _service.AtualizarPlanoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ObterPlanoAsync

    [Fact]
    public async Task ObterPlano_PlanoExistente_DeveRetornarPlano()
    {
        var dto = CriarPlanoDto();
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(dto);

        var result = await _service.ObterPlanoAsync("PLN-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }

    [Fact]
    public async Task ObterPlano_PlanoNãoExiste_DeveRetornarNotFound()
    {
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((PlanoDto?)null);

        var result = await _service.ObterPlanoAsync("PLN-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    [Fact]
    public async Task ObterPlano_CodigoInvalido_DeveRetornarFalha()
    {
        var result = await _service.ObterPlanoAsync("A");

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ListarPlanosAsync

    [Fact]
    public async Task ListarPlanos_DeveRetornarListaComSucesso()
    {
        var planos = new List<PlanoDto> { CriarPlanoDto(), CriarPlanoDto("PLN-002", "Plano 2") };
        _planoRepoMock.Setup(r => r.ListarPlanosComServicosAsync()).ReturnsAsync(planos);

        var result = await _service.ListarPlanosAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListarPlanosAtivos_DeveRetornarListaComSucesso()
    {
        var planos = new List<PlanoDto> { CriarPlanoDto() };
        _planoRepoMock.Setup(r => r.ListarPlanosAtivosComServicosAsync()).ReturnsAsync(planos);

        var result = await _service.ListarPlanosAtivosAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    #endregion

    #region ExcluirPlanoAsync

    [Fact]
    public async Task ExcluirPlano_PlanoSemServicos_DeveRetornarSucesso()
    {
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");
        _planoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        var result = await _service.ExcluirPlanoAsync("PLN-001");

        result.IsSuccess.Should().BeTrue();
        _planoRepoMock.Verify(r => r.Remover(plano), Times.Once);
    }

    [Fact]
    public async Task ExcluirPlano_PlanoComServicos_DeveRetornarFalha()
    {
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");
        plano.AddService(new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" });
        _planoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);

        var result = await _service.ExcluirPlanoAsync("PLN-001");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("serviços vinculados"));
    }

    [Fact]
    public async Task ExcluirPlano_PlanoNãoEncontrado_DeveRetornarNotFound()
    {
        _planoRepoMock.Setup(r => r.ObterPorCodigoParaEdicaoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);

        var result = await _service.ExcluirPlanoAsync("PLN-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion
}
