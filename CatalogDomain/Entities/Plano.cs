namespace CatalogDomain.Entities;

public class Plano
{
    public Plano() { } // EF
    public string CodPlano { get; set; } = default!;
    public string NomePlano { get; set; } = default!;
    public bool IndAtivo { get; set; }
    public bool IndGeraCobranca { get; set; }
    public decimal ValorBase { get; set; }

    // Navigation Properties para relação N:N
    public ICollection<PlanoServico> PlanoServicos { get; set; } = [];
    public Plano(string codPlano, string nome, decimal valor = 0)
    {
        CodPlano = codPlano;
        NomePlano = nome;
        ValorBase = valor;
    }
    public void AlteraStatusPlano(bool ativo) => IndAtivo = ativo;
    public void DefinirValorBase(decimal valor) => ValorBase = valor;
    public void DefinirGeraCobranca(bool geraCobranca) => IndGeraCobranca = geraCobranca;
    public void AlterarNome(string nome) => NomePlano = nome;
}