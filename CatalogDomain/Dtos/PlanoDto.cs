using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class PlanoDto
{
    public string CodPlano { get; set; } = default!;
    public string NomePlano { get; set; } = default!;
    public bool IndAtivo { get; set; }
    public bool IndGeraCobranca { get; set; }
    public decimal ValorBase { get; set; }
    public List<ServicoDto> Servicos { get; set; } = [];
}

public class PlanoRequest
{
    [Required(ErrorMessage = "Codigo do plano e obrigatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Codigo do plano deve ter entre 2 e 20 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Codigo do plano deve conter apenas letras, numeros, hifen ou underscore.")]
    public string CodPlano { get; set; } = default!;

    [Required(ErrorMessage = "Nome do plano e obrigatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome do plano deve ter entre 3 e 100 caracteres.")]
    public string NomePlano { get; set; } = default!;

    [Required(ErrorMessage = "Indicador de geracao de cobranca e obrigatorio.")]
    public bool IndGeraCobranca { get; set; }

    [Required(ErrorMessage = "Valor base e obrigatorio.")]
    [Range(0, 999999.99, ErrorMessage = "Valor base deve ser entre 0 e 999.999,99.")]
    public decimal ValorBase { get; set; }

    [Required(ErrorMessage = "Indicador de plano ativo e obrigatorio.")]
    public bool IndAtivo { get; set; }
}
