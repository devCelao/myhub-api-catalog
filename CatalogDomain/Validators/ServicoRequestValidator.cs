using CatalogDomain.Dtos;
using FluentValidation;

namespace CatalogDomain.Validators;

/// <summary>
/// Validador para requisições de criação/atualização de Serviços
/// </summary>
public class ServicoRequestValidator : AbstractValidator<ServicoRequest>
{
    public ServicoRequestValidator()
    {
        RuleFor(x => x.CodServico)
            .NotEmpty()
            .WithMessage("Código do serviço é obrigatório.")
            .MinimumLength(2)
            .WithMessage("Código do serviço deve ter no mínimo 2 caracteres.")
            .MaximumLength(20)
            .WithMessage("Código do serviço não pode exceder 20 caracteres.")
            .Matches(@"^[A-Za-z0-9_-]+$")
            .WithMessage("Código do serviço deve conter apenas letras, números, hífen ou underscore.");

        RuleFor(x => x.NomeServico)
            .NotEmpty()
            .WithMessage("Nome do serviço é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome do serviço deve ter no mínimo 3 caracteres.")
            .MaximumLength(100)
            .WithMessage("Nome do serviço não pode exceder 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }
}
