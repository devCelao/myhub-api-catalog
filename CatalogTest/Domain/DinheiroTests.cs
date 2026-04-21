using CatalogDomain.ValueObjects;
using FluentAssertions;

namespace CatalogTest.Domain;

public class DinheiroTests
{
    [Fact]
    public void Criar_ValorPositivo_DeveCriarComSucesso()
    {
        var dinheiro = new Dinheiro(100.50m);

        dinheiro.Valor.Should().Be(100.50m);
        dinheiro.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Criar_ValorZero_DeveCriarComSucesso()
    {
        var dinheiro = new Dinheiro(0);

        dinheiro.Valor.Should().Be(0);
    }

    [Fact]
    public void Criar_ValorNegativo_DeveLancarArgumentException()
    {
        var act = () => new Dinheiro(-1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_DeveArredondarPara2CasasDecimais()
    {
        var dinheiro = new Dinheiro(10.555m);

        dinheiro.Valor.Should().Be(10.56m);
    }

    [Fact]
    public void Somar_MesmaMoeda_DeveRetornarSomaCorreta()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(20);

        var resultado = a + b;

        resultado.Valor.Should().Be(30);
    }

    [Fact]
    public void Subtrair_ResultadoPositivo_DeveRetornarDiferenca()
    {
        var a = new Dinheiro(30);
        var b = new Dinheiro(10);

        var resultado = a - b;

        resultado.Valor.Should().Be(20);
    }

    [Fact]
    public void Subtrair_ResultadoNegativo_DeveLancarException()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(30);

        var act = () => a - b;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiplicar_FatorPositivo_DeveRetornarProduto()
    {
        var dinheiro = new Dinheiro(10);

        var resultado = dinheiro * 3;

        resultado.Valor.Should().Be(30);
    }

    [Fact]
    public void AplicarDesconto_Percentual50_DeveReduzirPelaMetade()
    {
        var dinheiro = new Dinheiro(100);

        var resultado = dinheiro.AplicarDesconto(50);

        resultado.Valor.Should().Be(50);
    }

    [Fact]
    public void AplicarAcrescimo_Percentual10_DeveAumentar10Porcento()
    {
        var dinheiro = new Dinheiro(100);

        var resultado = dinheiro.AplicarAcrescimo(10);

        resultado.Valor.Should().Be(110);
    }

    [Fact]
    public void Igualdade_MesmoValorEMoeda_DeveSerIgual()
    {
        var a = new Dinheiro(100);
        var b = new Dinheiro(100);

        a.Should().Be(b);
    }

    [Fact]
    public void ImplicitOperator_DeveConverterParaDecimal()
    {
        var dinheiro = new Dinheiro(99.90m);

        decimal valor = dinheiro;

        valor.Should().Be(99.90m);
    }
}
