namespace CatalogDomain.ValueObjects;

/// <summary>
/// Value Object representando valores monetários.
/// Garante precisão de 2 casas decimais e operações seguras.
/// </summary>
public class Dinheiro : ValueObject
{
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; }

    public static readonly Dinheiro Zero = new(0);
    private const string MOEDA_PADRAO = "BRL";

    private Dinheiro()
    {
        Valor = 0;
        Moeda = MOEDA_PADRAO;
    }

    /// <summary>
    /// Cria um novo valor monetário
    /// </summary>
    /// <param name="valor">Valor em reais</param>
    /// <param name="moeda">Código da moeda (padrão: BRL)</param>
    public Dinheiro(decimal valor, string moeda = MOEDA_PADRAO)
    {
        Validar(valor);
        Valor = Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        Moeda = moeda.ToUpperInvariant();
    }

    private static void Validar(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo.", nameof(valor));
    }

    public Dinheiro Somar(Dinheiro outro)
    {
        ValidarMesmaModeda(outro);
        return new Dinheiro(Valor + outro.Valor, Moeda);
    }

    public Dinheiro Subtrair(Dinheiro outro)
    {
        ValidarMesmaModeda(outro);
        var resultado = Valor - outro.Valor;
        if (resultado < 0)
            throw new InvalidOperationException("Operação resultaria em valor negativo.");
        return new Dinheiro(resultado, Moeda);
    }

    public Dinheiro Multiplicar(decimal fator)
    {
        if (fator < 0)
            throw new ArgumentException("Fator não pode ser negativo.", nameof(fator));
        return new Dinheiro(Valor * fator, Moeda);
    }

    public Dinheiro AplicarDesconto(decimal percentual)
    {
        if (percentual < 0 || percentual > 100)
            throw new ArgumentException("Percentual deve estar entre 0 e 100.", nameof(percentual));

        var valorDesconto = Valor * (percentual / 100);
        return new Dinheiro(Valor - valorDesconto, Moeda);
    }

    public Dinheiro AplicarAcrescimo(decimal percentual)
    {
        if (percentual < 0)
            throw new ArgumentException("Percentual não pode ser negativo.", nameof(percentual));

        var valorAcrescimo = Valor * (percentual / 100);
        return new Dinheiro(Valor + valorAcrescimo, Moeda);
    }

    private void ValidarMesmaModeda(Dinheiro outro)
    {
        if (Moeda != outro.Moeda)
            throw new InvalidOperationException($"Não é possível operar moedas diferentes: {Moeda} e {outro.Moeda}");
    }

    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => a.Somar(b);
    public static Dinheiro operator -(Dinheiro a, Dinheiro b) => a.Subtrair(b);
    public static Dinheiro operator *(Dinheiro a, decimal fator) => a.Multiplicar(fator);

    public static bool operator >(Dinheiro a, Dinheiro b)
    {
        a.ValidarMesmaModeda(b);
        return a.Valor > b.Valor;
    }

    public static bool operator <(Dinheiro a, Dinheiro b)
    {
        a.ValidarMesmaModeda(b);
        return a.Valor < b.Valor;
    }

    public static bool operator >=(Dinheiro a, Dinheiro b) => a > b || a == b;
    public static bool operator <=(Dinheiro a, Dinheiro b) => a < b || a == b;

    public static implicit operator decimal(Dinheiro dinheiro) => dinheiro.Valor;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
        yield return Moeda;
    }

    public override string ToString() => $"{Moeda} {Valor:N2}";

    /// <summary>
    /// Formata o valor como moeda brasileira
    /// </summary>
    public string FormatarComoReal() => Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
}
