using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class PlanoRequest
{
    [Required(ErrorMessage = "Código do plano é obrigatório.")]
    public string CodPlano { get; set; } = default!;

    [Required(ErrorMessage = "Nome do plano é obrigatório.")]
    [StringLength(255, ErrorMessage = "Nome do plano deve ter no máximo 255 caracteres.")]
    public string NomePlano { get; set; } = default!;

    [Required(ErrorMessage = "Indicador de geração de cobrança é obrigatório.")]
    public bool IndGeraCobranca { get; set; }

    [Required(ErrorMessage = "Valor base é obrigatório.")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor base deve ser maior ou igual a zero.")]
    public decimal ValorBase { get; set; }

    [Required(ErrorMessage = "Indicador de plano ativo é obrigatório.")]
    public bool IndAtivo { get; set; }
}
