namespace CatalogDomain.Entities;

public class Servico : Entity, IAggregateRoot
{
    public Servico() { }

    public Servico(string codServico, string nome, string? criadoPor = null)
    {
        CodServico = codServico;
        NomeServico = nome;

        if (!string.IsNullOrWhiteSpace(criadoPor))
            DefinirCriador(criadoPor);
    }

    public string CodServico { get; private set; } = default!;
    public string NomeServico { get; private set; } = default!;
    public string? Descricao { get; private set; }

    public ICollection<PlanoServico> PlanoServicos { get; private set; } = [];
    public ICollection<Funcao> Funcoes { get; private set; } = [];

    public void ChangeName(string name, string usuario)
    {
        NomeServico = name;
        RegistrarAtualizacao(usuario);
    }

    public void ChangeDescription(string? description, string usuario)
    {
        Descricao = description;
        RegistrarAtualizacao(usuario);
    }

    public void RemoveFunction(Funcao funcao) => Funcoes.Remove(funcao);

    public void ReorderFunctions(string codFuncao, int order, string usuario)
        => Funcoes.FirstOrDefault(f => f.CodFuncao == codFuncao)?.ChangeOrder(order, usuario);
}
