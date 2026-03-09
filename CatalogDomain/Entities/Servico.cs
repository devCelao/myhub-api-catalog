namespace CatalogDomain.Entities;

public class Servico
{
    public Servico() { } // EF
    public Servico(string codServico, string nome)
    {
        CodServico = codServico;
        NomeServico = nome;
    }
    public string CodServico { get; set; } = default!;
    public string NomeServico { get; set; } = default!;
    public string? Descricao { get; set; }

    // Navigation Properties para relação N:N
    public ICollection<PlanoServico> PlanoServicos { get; set; } = [];
    
    // Navigation Property para Funcoes
    public ICollection<Funcao> Funcoes { get; set; } = [];
    
    public void AlterarNome(string nome) => NomeServico = nome;
    public void AlterarDescricao(string? descricao) => Descricao = descricao;
}
