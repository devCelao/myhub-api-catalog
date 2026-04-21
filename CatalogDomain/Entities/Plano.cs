using CatalogDomain.ValueObjects;

namespace CatalogDomain.Entities;

public class Plano : Entity, IAggregateRoot
{
    protected Plano() { }

    public CodigoPlano CodPlano { get; private set; } = default!;
    public string NomePlano { get; private set; } = default!;
    public bool IndAtivo { get; private set; }
    public bool IndGeraCobranca { get; private set; }
    public Dinheiro ValorBase { get; private set; } = default!;

    private readonly List<PlanoServico> _planoServicos = [];
    public IReadOnlyCollection<PlanoServico> PlanoServicos => _planoServicos.AsReadOnly();

    public Plano(CodigoPlano codPlano, string nome, Dinheiro valor, string? criadoPor = null)
    {
        ValidateName(nome);

        CodPlano = codPlano ?? throw new ArgumentNullException(nameof(codPlano));
        NomePlano = nome;
        ValorBase = valor ?? throw new ArgumentNullException(nameof(valor));
        IndAtivo = true;
        IndGeraCobranca = true;

        if (!string.IsNullOrWhiteSpace(criadoPor))
            DefinirCriador(criadoPor);
    }

    public void ChangeStatus(bool active, string usuario)
    {
        if (IndAtivo == active)
            return;

        IndAtivo = active;
        RegistrarAtualizacao(usuario);
    }

    public void SetBaseValue(Dinheiro newValue, string usuario)
    {
        ArgumentNullException.ThrowIfNull(newValue);
        ValorBase = newValue;
        RegistrarAtualizacao(usuario);
    }

    public void SetBillingEnabled(bool billingEnabled) => IndGeraCobranca = billingEnabled;

    public void ChangeName(string name, string usuario)
    {
        ValidateName(name);
        NomePlano = name;
        RegistrarAtualizacao(usuario);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do plano e obrigatorio.", nameof(name));

        if (name.Length < 3)
            throw new ArgumentException("Nome do plano deve ter no minimo 3 caracteres.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Nome do plano não pode exceder 100 caracteres.", nameof(name));
    }

    public void AddService(PlanoServico planoServico)
    {
        ArgumentNullException.ThrowIfNull(planoServico);

        if (_planoServicos.Any(ps => ps.CodServico == planoServico.CodServico))
            return;

        _planoServicos.Add(planoServico);
    }

    public void RemoveService(string codServico)
    {
        var servico = _planoServicos.FirstOrDefault(ps => ps.CodServico == codServico);
        if (servico != null)
            _planoServicos.Remove(servico);
    }

    public void ClearServices() => _planoServicos.Clear();

    public bool HasLinkedServices() => _planoServicos.Count > 0;
}
