namespace CatalogDomain.Entities;

/// <summary>
/// Classe base para todas as entidades de domínio
/// Fornece identificação única e campos de auditoria
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou o registro
    /// </summary>
    public string? CriadoPor { get; protected set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; protected set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    public string? AtualizadoPor { get; protected set; }

    /// <summary>
    /// Indica se o registro foi excluído logicamente (Soft Delete)
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Data da exclusão lógica
    /// </summary>
    public DateTime? DataExclusao { get; protected set; }

    /// <summary>
    /// Usuário que excluiu o registro
    /// </summary>
    public string? ExcluidoPor { get; protected set; }

    /// <summary>
    /// Define quem criou a entidade
    /// </summary>
    public void DefinirCriador(string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            throw new ArgumentException("Usuário não pode ser vazio.", nameof(usuario));

        CriadoPor = usuario;
    }

    /// <summary>
    /// Registra uma atualização na entidade
    /// </summary>
    public void RegistrarAtualizacao(string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            throw new ArgumentException("Usuário não pode ser vazio.", nameof(usuario));

        DataAtualizacao = DateTime.UtcNow;
        AtualizadoPor = usuario;
    }

    /// <summary>
    /// Realiza exclusão lógica (Soft Delete)
    /// </summary>
    public void ExcluirLogicamente(string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            throw new ArgumentException("Usuário não pode ser vazio.", nameof(usuario));

        IsDeleted = true;
        DataExclusao = DateTime.UtcNow;
        ExcluidoPor = usuario;
    }

    /// <summary>
    /// Restaura um registro excluído logicamente
    /// </summary>
    public void Restaurar()
    {
        IsDeleted = false;
        DataExclusao = null;
        ExcluidoPor = null;
    }
}
