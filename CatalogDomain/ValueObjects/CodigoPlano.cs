using System.Text.RegularExpressions;

namespace CatalogDomain.ValueObjects;

/// <summary>
/// Value Object representando o código de um plano.
/// Garante que o código sempre esteja em formato válido.
/// </summary>
public class CodigoPlano : ValueObject
{
    public string Valor { get; private set; }

    private CodigoPlano()
    {
        Valor = string.Empty;
    }

    /// <summary>
    /// Cria um novo CodigoPlano com validações
    /// </summary>
    /// <param name="valor">Código do plano (ex: PLN-001, BASIC, PREMIUM)</param>
    public CodigoPlano(string valor)
    {
        Validar(valor);
        Valor = valor.ToUpperInvariant().Trim();
    }

    private static void Validar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Código do plano não pode ser vazio.", nameof(valor));

        if (valor.Length < 2)
            throw new ArgumentException("Código do plano deve ter no mínimo 2 caracteres.", nameof(valor));

        if (valor.Length > 20)
            throw new ArgumentException("Código do plano não pode exceder 20 caracteres.", nameof(valor));

        if (!Regex.IsMatch(valor, @"^[A-Za-z0-9_-]+$"))
            throw new ArgumentException(
                "Código do plano deve conter apenas letras, números, hífen ou underscore.",
                nameof(valor)
            );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
    }

    public static implicit operator string(CodigoPlano codigo) => codigo.Valor;

    public static explicit operator CodigoPlano(string valor) => new(valor);

    public override string ToString() => Valor;
}
