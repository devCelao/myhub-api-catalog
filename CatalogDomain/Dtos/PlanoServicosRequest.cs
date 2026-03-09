using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class PlanoServicosRequest
{
    [Required(ErrorMessage = "Obrigatório informar código do serviço.")]
    public string CodPlano { get; set; } = default!;

    public List<string>? CodServicos { get; set; } = [];

    public bool HasServices => CodServicos is not null && CodServicos.Count > 0;
}


