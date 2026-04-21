using CatalogDomain.Dtos;
using FluentValidation;

namespace CatalogDomain.Validators;

public class FuncaoRequestValidator : AbstractValidator<FuncaoRequest>
{
    public FuncaoRequestValidator()
    {
        RuleFor(x => x.CodFuncao)
            .NotEmpty()
            .WithMessage("Código da função é obrigatório.")
            .MaximumLength(50)
            .WithMessage("Código da função não pode exceder 50 caracteres.");

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label da função é obrigatório.")
            .MaximumLength(100)
            .WithMessage("Label não pode exceder 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));

        RuleFor(x => x.NumOrdem)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Número de ordem não pode ser negativo.");
    }
}
