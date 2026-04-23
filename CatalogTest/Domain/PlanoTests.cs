using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using FluentAssertions;

namespace CatalogTest.Domain;

public class PlanoTests
{
    private static Plano CriarPlanoValido(string cod = "PLN-001", string nome = "Plano Teste",
        decimal valor = 99.90m, string usuario = "admin")
    {
        return new Plano(new CodigoPlano(cod), nome, new Dinheiro(valor), usuario);
    }

    [Fact]
    public void Criar_DadosValidos_DeveCriarPlanoAtivo()
    {
        var plano = CriarPlanoValido();

        plano.CodPlano.Valor.Should().Be("PLN-001");
        plano.NomePlano.Should().Be("Plano Teste");
        plano.ValorBase.Valor.Should().Be(99.90m);
        plano.IndAtivo.Should().BeTrue();
        plano.IndGeraCobranca.Should().BeTrue();
        plano.CriadoPor.Should().Be("admin");
    }

    [Fact]
    public void Criar_NomeNuloOuVazio_DeveLancarArgumentException()
    {
        var act = () => new Plano(new CodigoPlano("PLN-001"), "", new Dinheiro(10), "admin");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_NomeMenorQue3Caracteres_DeveLancarArgumentException()
    {
        var act = () => new Plano(new CodigoPlano("PLN-001"), "AB", new Dinheiro(10), "admin");

        act.Should().Throw<ArgumentException>().WithMessage("*3 caracteres*");
    }

    [Fact]
    public void Criar_NomeMaiorQue100Caracteres_DeveLancarArgumentException()
    {
        var nomeGrande = new string('A', 101);
        var act = () => new Plano(new CodigoPlano("PLN-001"), nomeGrande, new Dinheiro(10), "admin");

        act.Should().Throw<ArgumentException>().WithMessage("*100 caracteres*");
    }

    [Fact]
    public void Criar_CodigoNulo_DeveLancarArgumentNullException()
    {
        var act = () => new Plano(null!, "Plano Teste", new Dinheiro(10), "admin");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangeStatus_DeAtivoParaInativo_DeveAlterarStatus()
    {
        var plano = CriarPlanoValido();

        plano.ChangeStatus(false, "admin");

        plano.IndAtivo.Should().BeFalse();
        plano.AtualizadoPor.Should().Be("admin");
    }

    [Fact]
    public void ChangeStatus_MesmoValor_NaoDeveRegistrarAtualizacao()
    {
        var plano = CriarPlanoValido();

        plano.ChangeStatus(true, "admin");

        plano.AtualizadoPor.Should().BeNull();
    }

    [Fact]
    public void SetBaseValue_NovoValor_DeveAtualizar()
    {
        var plano = CriarPlanoValido();
        var novoValor = new Dinheiro(199.90m);

        plano.SetBaseValue(novoValor, "admin");

        plano.ValorBase.Valor.Should().Be(199.90m);
        plano.AtualizadoPor.Should().Be("admin");
    }

    [Fact]
    public void ChangeName_NomeValido_DeveAtualizarNome()
    {
        var plano = CriarPlanoValido();

        plano.ChangeName("Novo Nome", "admin");

        plano.NomePlano.Should().Be("Novo Nome");
        plano.AtualizadoPor.Should().Be("admin");
    }

    [Fact]
    public void AddService_ServicoNovo_DeveAdicionarAoPlano()
    {
        var plano = CriarPlanoValido();
        var ps = new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" };

        plano.AddService(ps);

        plano.PlanoServicos.Should().HaveCount(1);
    }

    [Fact]
    public void AddService_ServicoDuplicado_NaoDeveAdicionarNovamente()
    {
        var plano = CriarPlanoValido();
        var ps1 = new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" };
        var ps2 = new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" };

        plano.AddService(ps1);
        plano.AddService(ps2);

        plano.PlanoServicos.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveService_ServicoExistente_DeveRemover()
    {
        var plano = CriarPlanoValido();
        var ps = new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" };
        plano.AddService(ps);

        plano.RemoveService("SVC-001");

        plano.PlanoServicos.Should().BeEmpty();
    }

    [Fact]
    public void HasLinkedServices_SemServicos_DeveRetornarFalse()
    {
        var plano = CriarPlanoValido();

        plano.HasLinkedServices().Should().BeFalse();
    }

    [Fact]
    public void HasLinkedServices_ComServicos_DeveRetornarTrue()
    {
        var plano = CriarPlanoValido();
        plano.AddService(new PlanoServico { CodPlano = plano.CodPlano, CodServico = "SVC-001" });

        plano.HasLinkedServices().Should().BeTrue();
    }
}
