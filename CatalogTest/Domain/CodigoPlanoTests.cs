using CatalogDomain.ValueObjects;
using FluentAssertions;

namespace CatalogTest.Domain;

public class CodigoPlanoTests
{
    [Theory]
    [InlineData("PLN-001")]
    [InlineData("BASIC")]
    [InlineData("premium_v2")]
    [InlineData("AB")]
    public void Criar_CodigoValido_DeveCriarComSucesso(string valor)
    {
        var codigo = new CodigoPlano(valor);

        codigo.Valor.Should().Be(valor.ToUpperInvariant().Trim());
    }

    [Fact]
    public void Criar_CodigoNuloOuVazio_DeveLancarArgumentException()
    {
        var act1 = () => new CodigoPlano(null!);
        var act2 = () => new CodigoPlano("");
        var act3 = () => new CodigoPlano("   ");

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_CodigoMenorQue2Caracteres_DeveLancarArgumentException()
    {
        var act = () => new CodigoPlano("A");

        act.Should().Throw<ArgumentException>().WithMessage("*2 caracteres*");
    }

    [Fact]
    public void Criar_CodigoMaiorQue20Caracteres_DeveLancarArgumentException()
    {
        var act = () => new CodigoPlano("A".PadRight(21, 'X'));

        act.Should().Throw<ArgumentException>().WithMessage("*20 caracteres*");
    }

    [Theory]
    [InlineData("PLN 001")]
    [InlineData("PLN@001")]
    [InlineData("PLN.001")]
    public void Criar_CodigoComCaracteresInvalidos_DeveLancarArgumentException(string valor)
    {
        var act = () => new CodigoPlano(valor);

        act.Should().Throw<ArgumentException>().WithMessage("*letras*");
    }

    [Fact]
    public void ImplicitOperator_DeveConverterParaString()
    {
        var codigo = new CodigoPlano("BASIC");

        string resultado = codigo;

        resultado.Should().Be("BASIC");
    }

    [Fact]
    public void Igualdade_MesmoValor_DeveSerIgual()
    {
        var codigo1 = new CodigoPlano("BASIC");
        var codigo2 = new CodigoPlano("basic");

        codigo1.Should().Be(codigo2);
    }
}
