using CatalogApplication.Common;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogInfrastructure.Repositories;

namespace CatalogApplication.Services;

public interface IPlanoApplicationService
{
    Task<ServiceResult<PlanoDto>> CriarPlanoAsync(PlanoRequest request);
    Task<ServiceResult<PlanoDto>> AtualizarPlanoAsync(PlanoRequest request);
    Task<ServiceResult<PlanoDto>> ObterPlanoAsync(string codPlano);
    Task<ServiceResult<List<PlanoDto>>> ListarPlanosAsync();
    Task<ServiceResult<List<PlanoDto>>> ListarPlanosAtivosAsync();
    Task<ServiceResult> ExcluirPlanoAsync(string codPlano);
}

public class PlanoApplicationService(
    IPlanoRepository planoRepository,
    ICurrentUserService currentUser) : IPlanoApplicationService
{
    private readonly IPlanoRepository _planoRepository = planoRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<ServiceResult<PlanoDto>> CriarPlanoAsync(PlanoRequest request)
    {
        try
        {
            var codigoPlano = new CodigoPlano(request.CodPlano);
            var valorBase = new Dinheiro(request.ValorBase);

            var planoExistente = await _planoRepository.ObterPorCodigoAsync(codigoPlano);
            if (planoExistente != null)
                return ServiceResult<PlanoDto>.Failure($"Plano {request.CodPlano} já existe.");

            var usuario = _currentUser.NomeUsuario;
            var plano = new Plano(codigoPlano, request.NomePlano, valorBase, usuario);
            plano.SetBillingEnabled(request.IndGeraCobranca);
            plano.ChangeStatus(request.IndAtivo, usuario);

            await _planoRepository.AdicionarAsync(plano);

            if (!await _planoRepository.UnitOfWork.Commit())
                return ServiceResult<PlanoDto>.Failure("Erro ao salvar plano.");

            var planoDto = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
            return ServiceResult<PlanoDto>.Success(planoDto!, $"Plano {request.NomePlano} criado com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<PlanoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<PlanoDto>.Failure("Erro interno ao criar plano.");
        }
    }

    public async Task<ServiceResult<PlanoDto>> AtualizarPlanoAsync(PlanoRequest request)
    {
        try
        {
            var codigoPlano = new CodigoPlano(request.CodPlano);
            var valorBase = new Dinheiro(request.ValorBase);

            var plano = await _planoRepository.ObterPorCodigoParaEdicaoAsync(codigoPlano);
            if (plano == null)
                return ServiceResult<PlanoDto>.NotFound($"Plano {request.CodPlano} não encontrado.");

            var usuario = _currentUser.NomeUsuario;
            plano.ChangeName(request.NomePlano, usuario);
            plano.SetBaseValue(valorBase, usuario);
            plano.SetBillingEnabled(request.IndGeraCobranca);
            plano.ChangeStatus(request.IndAtivo, usuario);

            _planoRepository.Atualizar(plano);

            if (!await _planoRepository.UnitOfWork.Commit())
                return ServiceResult<PlanoDto>.Failure("Erro ao atualizar plano.");

            var planoDto = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
            return ServiceResult<PlanoDto>.Success(planoDto!, $"Plano {request.NomePlano} atualizado com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<PlanoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<PlanoDto>.Failure("Erro interno ao atualizar plano.");
        }
    }

    public async Task<ServiceResult<PlanoDto>> ObterPlanoAsync(string codPlano)
    {
        try
        {
            var codigoPlanoVO = new CodigoPlano(codPlano);
            var planoDto = await _planoRepository.ObterPlanoComServicosAsync(codigoPlanoVO);

            if (planoDto == null)
                return ServiceResult<PlanoDto>.NotFound($"Plano {codPlano} não encontrado.");

            return ServiceResult<PlanoDto>.Success(planoDto);
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<PlanoDto>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<List<PlanoDto>>> ListarPlanosAsync()
    {
        var planos = await _planoRepository.ListarPlanosComServicosAsync();
        return ServiceResult<List<PlanoDto>>.Success(planos);
    }

    public async Task<ServiceResult<List<PlanoDto>>> ListarPlanosAtivosAsync()
    {
        var planos = await _planoRepository.ListarPlanosAtivosComServicosAsync();
        return ServiceResult<List<PlanoDto>>.Success(planos);
    }

    public async Task<ServiceResult> ExcluirPlanoAsync(string codPlano)
    {
        try
        {
            var codigoPlanoVO = new CodigoPlano(codPlano);
            var plano = await _planoRepository.ObterPorCodigoParaEdicaoAsync(codigoPlanoVO);

            if (plano == null)
                return ServiceResult.NotFound($"Plano {codPlano} não encontrado.");

            if (plano.HasLinkedServices())
                return ServiceResult.Failure($"Não é possivel excluir o plano {codPlano} pois possui serviços vinculados.");

            _planoRepository.Remover(plano);

            if (!await _planoRepository.UnitOfWork.Commit())
                return ServiceResult.Failure("Erro ao excluir plano.");

            return ServiceResult.Success($"Plano {codPlano} excluido com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }
}
