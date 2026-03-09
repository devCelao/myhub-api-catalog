namespace CatalogDomain.Entities;

public class PlanoServico
{
    public PlanoServico() { } // EF
    public string CodPlano { get; set; } = default!;
    public string CodServico { get; set; } = default!;

    // Navigation Properties
    public Plano Plano { get; set; } = default!;
    public Servico Servico { get; set; } = default!;
}
