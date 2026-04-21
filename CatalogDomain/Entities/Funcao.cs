namespace CatalogDomain.Entities;

public class Funcao : Entity
{
    public Funcao() { } // EF
    
    public Funcao(string codFuncao, string codServico, string label, string? criadoPor = null)
    {
        CodFuncao = codFuncao;
        CodServico = codServico;
        Label = label;
        IndAtivo = true;
        NumOrdem = 0;

        if (!string.IsNullOrWhiteSpace(criadoPor))
            DefinirCriador(criadoPor);
    }

    public string CodFuncao { get; private set; } = default!;
    public string CodServico { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public string? Descricao { get; private set; }
    public string? Icone { get; private set; }
    public int NumOrdem { get; private set; }
    public bool IndAtivo { get; private set; }

    // Navigation Property
    public Servico Servico { get; private set; } = default!;

    public void ChangeLabel(string label, string usuario)
    {
        Label = label;
        RegistrarAtualizacao(usuario);
    }

    public void ChangeDescription(string? description, string usuario)
    {
        Descricao = description;
        RegistrarAtualizacao(usuario);
    }

    public void ChangeIcon(string? icon, string usuario)
    {
        Icone = icon;
        RegistrarAtualizacao(usuario);
    }

    public void ChangeOrder(int order, string usuario)
    {
        NumOrdem = order;
        RegistrarAtualizacao(usuario);
    }

    public void ChangeStatus(bool active, string usuario)
    {
        IndAtivo = active;
        RegistrarAtualizacao(usuario);
    }
}

