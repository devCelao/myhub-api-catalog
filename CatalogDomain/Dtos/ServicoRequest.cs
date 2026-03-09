using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class ServicoRequest
{
    [Required(ErrorMessage = "Código do serviço é obrigatório.")]
    public string CodServico { get; set; } = default!;

    [Required(ErrorMessage = "Nome do serviço é obrigatório.")]
    [StringLength(255, ErrorMessage = "Nome do serviço deve ter no máximo 255 caracteres.")]
    public string NomeServico { get; set; } = default!;

    [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }
}
