namespace CatalogDomain.Entities;

public class Funcao
{
    public Funcao() { } // EF
    
    public Funcao(string codFuncao, string codServico, string label)
    {
        CodFuncao = codFuncao;
        CodServico = codServico;
        Label = label;
        IndAtivo = true;
        NumOrdem = 0;
    }

    public string CodFuncao { get; set; } = default!;
    public string CodServico { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public int NumOrdem { get; set; }
    public bool IndAtivo { get; set; }

    // Navigation Property
    public Servico Servico { get; set; } = default!;

    public void AlterarLabel(string label) => Label = label;
    public void AlterarDescricao(string? descricao) => Descricao = descricao;
    public void AlterarIcone(string? icone) => Icone = icone;
    public void AlterarOrdem(int ordem) => NumOrdem = ordem;
    public void AlterarStatus(bool ativo) => IndAtivo = ativo;
}

