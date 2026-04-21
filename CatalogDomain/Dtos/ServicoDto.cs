using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class ServicoDto
{
    public string CodServico { get; set; } = default!;
    public string NomeServico { get; set; } = default!;
    public string? Descricao { get; set; }
}

public class ServicoRequest
{
    [Required(ErrorMessage = "Codigo do servico e obrigatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Codigo do servico deve ter entre 2 e 20 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Codigo do servico deve conter apenas letras, numeros, hifen ou underscore.")]
    public string CodServico { get; set; } = default!;

    [Required(ErrorMessage = "Nome do servico e obrigatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome do servico deve ter entre 3 e 100 caracteres.")]
    public string NomeServico { get; set; } = default!;

    [StringLength(500, ErrorMessage = "Descricao deve ter no maximo 500 caracteres.")]
    public string? Descricao { get; set; }
}
