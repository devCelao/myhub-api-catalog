using CatalogDomain.Dtos;
using FluentValidation;

namespace CatalogDomain.Validators;

/// <summary>
/// Validador para requisições de criação/atualização de Planos
/// </summary>
public class PlanoRequestValidator : AbstractValidator<PlanoRequest>
{
    public PlanoRequestValidator()
    {
        RuleFor(x => x.CodPlano)
            .NotEmpty()
            .WithMessage("Código do plano é obrigatório.")
            .MinimumLength(2)
            .WithMessage("Código do plano deve ter no mínimo 2 caracteres.")
            .MaximumLength(20)
            .WithMessage("Código do plano não pode exceder 20 caracteres.")
            .Matches(@"^[A-Za-z0-9_-]+$")
            .WithMessage("Código do plano deve conter apenas letras, números, hífen ou underscore.");

        RuleFor(x => x.NomePlano)
            .NotEmpty()
            .WithMessage("Nome do plano é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome do plano deve ter no mínimo 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("Nome do plano não pode exceder 100 caracteres.");

        RuleFor(x => x.ValorBase)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor base não pode ser negativo.")
            .LessThan(1000000)
            .WithMessage("Valor base não pode exceder R$ 999.999,99.");

        When(x => x.ValorBase > 10000, () =>
        {
            RuleFor(x => x.IndGeraCobranca)
                .Equal(true)
                .WithMessage("Planos com valor acima de R$ 10.000,00 devem gerar cobrança.");
        });
    }
}
