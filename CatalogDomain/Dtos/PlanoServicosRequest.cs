using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class PlanoServicosRequest
{
    [Required(ErrorMessage = "Obrigatorio informar codigo do plano.")]
    public string CodPlano { get; set; } = default!;

    public List<string>? CodServicos { get; set; } = [];
}
