using CatalogDomain.Entities;
using FluentAssertions;

namespace CatalogTest.Domain;

public class ServicoTests
{
    [Fact]
    public void Criar_DadosValidos_DeveCriarServico()
    {
        var servico = new Servico("SVC-001", "Servico Teste", "admin");

        servico.CodServico.Should().Be("SVC-001");
        servico.NomeServico.Should().Be("Servico Teste");
        servico.CriadoPor.Should().Be("admin");
    }

    [Fact]
    public void ChangeName_NomeValido_DeveAtualizarNome()
    {
        var servico = new Servico("SVC-001", "Original", "admin");

        servico.ChangeName("Novo Nome", "editor");

        servico.NomeServico.Should().Be("Novo Nome");
        servico.AtualizadoPor.Should().Be("editor");
    }

    [Fact]
    public void ChangeDescription_DescricaoValida_DeveAtualizar()
    {
        var servico = new Servico("SVC-001", "Servico", "admin");

        servico.ChangeDescription("Nova descricao do servico", "editor");

        servico.Descricao.Should().Be("Nova descricao do servico");
        servico.AtualizadoPor.Should().Be("editor");
    }

    [Fact]
    public void ChangeDescription_Null_DevePermitirLimpar()
    {
        var servico = new Servico("SVC-001", "Servico", "admin");
        servico.ChangeDescription("Descricao", "admin");

        servico.ChangeDescription(null, "admin");

        servico.Descricao.Should().BeNull();
    }

    [Fact]
    public void RemoveFunction_FuncaoExistente_DeveRemover()
    {
        var servico = new Servico("SVC-001", "Servico", "admin");
        var funcao = new Funcao("FN-001", "SVC-001", "Label", "admin");
        servico.Funcoes.Add(funcao);

        servico.RemoveFunction(funcao);

        servico.Funcoes.Should().BeEmpty();
    }
}
