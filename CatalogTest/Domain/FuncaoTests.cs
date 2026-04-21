using CatalogDomain.Entities;
using FluentAssertions;

namespace CatalogTest.Domain;

public class FuncaoTests
{
    [Fact]
    public void Criar_DadosValidos_DeveCriarFuncaoAtivaComOrdemZero()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Minha Funcao", "admin");

        funcao.CodFuncao.Should().Be("FN-001");
        funcao.CodServico.Should().Be("SVC-001");
        funcao.Label.Should().Be("Minha Funcao");
        funcao.IndAtivo.Should().BeTrue();
        funcao.NumOrdem.Should().Be(0);
        funcao.CriadoPor.Should().Be("admin");
    }

    [Fact]
    public void ChangeLabel_LabelValido_DeveAtualizar()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Original", "admin");

        funcao.ChangeLabel("Novo Label", "editor");

        funcao.Label.Should().Be("Novo Label");
        funcao.AtualizadoPor.Should().Be("editor");
    }

    [Fact]
    public void ChangeDescription_ValorValido_DeveAtualizar()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");

        funcao.ChangeDescription("Descricao da funcao", "editor");

        funcao.Descricao.Should().Be("Descricao da funcao");
    }

    [Fact]
    public void ChangeIcon_ValorValido_DeveAtualizar()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");

        funcao.ChangeIcon("fa-home", "editor");

        funcao.Icone.Should().Be("fa-home");
    }

    [Fact]
    public void ChangeOrder_NovaOrdem_DeveAtualizar()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");

        funcao.ChangeOrder(5, "editor");

        funcao.NumOrdem.Should().Be(5);
    }

    [Fact]
    public void ChangeStatus_ParaInativo_DeveDesativar()
    {
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");

        funcao.ChangeStatus(false, "editor");

        funcao.IndAtivo.Should().BeFalse();
        funcao.AtualizadoPor.Should().Be("editor");
    }
}
