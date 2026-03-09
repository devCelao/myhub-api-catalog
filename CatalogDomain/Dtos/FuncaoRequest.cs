namespace CatalogDomain.Dtos;

public class FuncaoRequest
{
    public string CodFuncao { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Icone { get; set; } = null!;
    public int NumOrdem { get; set; }
    public bool IndAtivo { get; set; } 
}