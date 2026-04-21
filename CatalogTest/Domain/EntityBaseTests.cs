using CatalogDomain.Entities;
using FluentAssertions;

namespace CatalogTest.Domain;

public class EntityBaseTests
{
    private class FakeEntity : Entity { }

    [Fact]
    public void DefinirCriador_UsuarioValido_DeveDefinir()
    {
        var entity = new FakeEntity();

        entity.DefinirCriador("admin");

        entity.CriadoPor.Should().Be("admin");
    }

    [Fact]
    public void DefinirCriador_UsuarioVazio_DeveLancarArgumentException()
    {
        var entity = new FakeEntity();

        var act = () => entity.DefinirCriador("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegistrarAtualizacao_UsuarioValido_DeveRegistrar()
    {
        var entity = new FakeEntity();

        entity.RegistrarAtualizacao("editor");

        entity.AtualizadoPor.Should().Be("editor");
        entity.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void RegistrarAtualizacao_UsuarioVazio_DeveLancarArgumentException()
    {
        var entity = new FakeEntity();

        var act = () => entity.RegistrarAtualizacao("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExcluirLogicamente_DeveMarcarComoExcluido()
    {
        var entity = new FakeEntity();

        entity.ExcluirLogicamente("admin");

        entity.IsDeleted.Should().BeTrue();
        entity.ExcluidoPor.Should().Be("admin");
        entity.DataExclusao.Should().NotBeNull();
    }

    [Fact]
    public void Restaurar_DeveRemoverMarcaDeExclusao()
    {
        var entity = new FakeEntity();
        entity.ExcluirLogicamente("admin");

        entity.Restaurar();

        entity.IsDeleted.Should().BeFalse();
        entity.ExcluidoPor.Should().BeNull();
        entity.DataExclusao.Should().BeNull();
    }
}
