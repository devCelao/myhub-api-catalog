using CatalogDomain.ValueObjects;

namespace CatalogDomain.Entities;

public class PlanoServico
{
    public PlanoServico() { }
    
    public CodigoPlano CodPlano { get; set; } = default!;
    public string CodServico { get; set; } = default!;

    public Plano Plano { get; set; } = default!;
    public Servico Servico { get; set; } = default!;
}

