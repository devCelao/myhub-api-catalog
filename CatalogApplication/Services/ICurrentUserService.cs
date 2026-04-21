namespace CatalogApplication.Services;

public interface ICurrentUserService
{
    string NomeUsuario { get; }
    Guid UsuarioId { get; }
    bool IsAuthenticated { get; }
}
