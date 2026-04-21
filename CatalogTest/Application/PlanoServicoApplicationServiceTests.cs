using CatalogApplication.Services;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogInfrastructure.Repositories;
using DomainObjects.Data;
using FluentAssertions;
using Moq;

namespace CatalogTest.Application;

public class PlanoServicoApplicationServiceTests
{
    private readonly Mock<IPlanoRepository> _planoRepoMock;
    private readonly Mock<IServicoRepository> _servicoRepoMock;
    private readonly Mock<IPlanoServicoRepository> _planoServicoRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PlanoServicoApplicationService _service;

    public PlanoServicoApplicationServiceTests()
    {
        _planoRepoMock = new Mock<IPlanoRepository>();
        _servicoRepoMock = new Mock<IServicoRepository>();
        _planoServicoRepoMock = new Mock<IPlanoServicoRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _planoServicoRepoMock.Setup(r => r.UnitOfWork).Returns(_uowMock.Object);

        _service = new PlanoServicoApplicationService(
            _planoRepoMock.Object, _servicoRepoMock.Object, _planoServicoRepoMock.Object);
    }

    #region VincularServicosAoPlanoAsync

    [Fact]
    public async Task VincularServicos_PlanoExistenteServicosNovos_DeveRetornarSucesso()
    {
        var request = new PlanoServicosRequest { CodPlano = "PLN-001", CodServicos = ["SVC-001"] };
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");

        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _planoServicoRepoMock.Setup(r => r.ObterServicosDoPlanoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync([]);
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(new PlanoDto { CodPlano = "PLN-001", NomePlano = "Teste", Servicos = [] });

        var result = await _service.VincularServicosAoPlanoAsync(request);

        result.IsSuccess.Should().BeTrue();
        _planoServicoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<PlanoServico>()), Times.Once);
    }

    [Fact]
    public async Task VincularServicos_PlanoNãoEncontrado_DeveRetornarNotFound()
    {
        var request = new PlanoServicosRequest { CodPlano = "PLN-999", CodServicos = ["SVC-001"] };
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);

        var result = await _service.VincularServicosAoPlanoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    [Fact]
    public async Task VincularServicos_SemAlteracoes_DeveRetornarSucessoSemModificar()
    {
        var request = new PlanoServicosRequest { CodPlano = "PLN-001", CodServicos = ["SVC-001"] };
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");
        var servicosAtuais = new List<ServicoDto>
        {
            new() { CodServico = "SVC-001", NomeServico = "Servico" }
        };

        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _planoServicoRepoMock.Setup(r => r.ObterServicosDoPlanoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(servicosAtuais);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(new PlanoDto { CodPlano = "PLN-001", NomePlano = "Teste", Servicos = servicosAtuais });

        var result = await _service.VincularServicosAoPlanoAsync(request);

        result.IsSuccess.Should().BeTrue();
        _planoServicoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<PlanoServico>()), Times.Never);
    }

    [Fact]
    public async Task VincularServicos_ServicoNãoExiste_DeveRetornarFalha()
    {
        var request = new PlanoServicosRequest { CodPlano = "PLN-001", CodServicos = ["SVC-INVALIDO"] };
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");

        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _planoServicoRepoMock.Setup(r => r.ObterServicosDoPlanoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync([]);
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-INVALIDO"))
            .ReturnsAsync((Servico?)null);

        var result = await _service.VincularServicosAoPlanoAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("encontrad"));
    }

    #endregion

    #region LimparServicosDoPlanoAsync

    [Fact]
    public async Task LimparServicos_PlanoComServicos_DeveRemoverTodos()
    {
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _planoServicoRepoMock.Setup(r => r.ObterServicosDoPlanoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync([new ServicoDto { CodServico = "SVC-001", NomeServico = "A" }]);
        _uowMock.Setup(u => u.Commit()).ReturnsAsync(true);
        _planoRepoMock.Setup(r => r.ObterPlanoComServicosAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(new PlanoDto { CodPlano = "PLN-001", NomePlano = "Teste", Servicos = [] });

        var result = await _service.LimparServicosDoPlanoAsync("PLN-001");

        result.IsSuccess.Should().BeTrue();
        _planoServicoRepoMock.Verify(r =>
            r.RemoverTodosServicosDoPlanoAsync(It.IsAny<CodigoPlano>()), Times.Once);
    }

    [Fact]
    public async Task LimparServicos_PlanoNãoEncontrado_DeveRetornarNotFound()
    {
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync((Plano?)null);

        var result = await _service.LimparServicosDoPlanoAsync("PLN-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion

    #region ListarServicosDoPlanoAsync

    [Fact]
    public async Task ListarServicosDoPlano_PlanoExistente_DeveRetornarServicos()
    {
        var plano = new Plano(new CodigoPlano("PLN-001"), "Teste", new Dinheiro(10), "admin");
        _planoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync(plano);
        _planoServicoRepoMock.Setup(r => r.ObterServicosDoPlanoAsync(It.IsAny<CodigoPlano>()))
            .ReturnsAsync([new ServicoDto { CodServico = "SVC-001", NomeServico = "A" }]);

        var result = await _service.ListarServicosDoPlanoAsync("PLN-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    #endregion

    #region ListarPlanosDoServicoAsync

    [Fact]
    public async Task ListarPlanosDoServico_ServicoExistente_DeveRetornarPlanos()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync("SVC-001"))
            .ReturnsAsync(new Servico("SVC-001", "Servico", "admin"));
        _planoServicoRepoMock.Setup(r => r.ObterPlanosDoServicoAsync("SVC-001"))
            .ReturnsAsync([new PlanoDto { CodPlano = "PLN-001", NomePlano = "A", Servicos = [] }]);

        var result = await _service.ListarPlanosDoServicoAsync("SVC-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListarPlanosDoServico_ServicoNãoExiste_DeveRetornarNotFound()
    {
        _servicoRepoMock.Setup(r => r.ObterPorCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Servico?)null);

        var result = await _service.ListarPlanosDoServicoAsync("SVC-999");

        result.IsSuccess.Should().BeFalse();
        result.IsNotFound.Should().BeTrue();
    }

    #endregion
}
