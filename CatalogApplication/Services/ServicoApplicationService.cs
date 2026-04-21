using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;
using MicroserviceCore.Respostas;

namespace CatalogApplication.Services;

public interface IServicoApplicationService
{
    Task<ServiceResult<ServicoDto>> CriarServicoAsync(ServicoRequest request);
    Task<ServiceResult<ServicoDto>> AtualizarServicoAsync(ServicoRequest request);
    Task<ServiceResult<ServicoDto>> ObterServicoAsync(string codServico);
    Task<ServiceResult<List<ServicoDto>>> ListarServicosAsync();
    Task<ServiceResult> ExcluirServicoAsync(string codServico);
}

public class ServicoApplicationService(
    IServicoRepository servicoRepository,
    ICurrentUserService currentUser) : IServicoApplicationService
{
    private readonly IServicoRepository _servicoRepository = servicoRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<ServiceResult<ServicoDto>> CriarServicoAsync(ServicoRequest request)
    {
        try
        {
            var servicoPorCodigo = await _servicoRepository.ObterPorCodigoAsync(request.CodServico);
            if (servicoPorCodigo != null)
                return ServiceResult<ServicoDto>.Failure($"Serviço {request.CodServico} já existe.");

            var servicoPorNome = await _servicoRepository.ObterPorNomeAsync(request.NomeServico);
            if (servicoPorNome != null)
                return ServiceResult<ServicoDto>.Failure($"Serviço com nome {request.NomeServico} já existe.");

            var usuario = _currentUser.NomeUsuario;
            var servico = new Servico(request.CodServico, request.NomeServico, usuario);
            if (!string.IsNullOrWhiteSpace(request.Descricao))
                servico.ChangeDescription(request.Descricao, usuario);

            await _servicoRepository.AdicionarAsync(servico);

            if (!await _servicoRepository.UnitOfWork.Commit())
                return ServiceResult<ServicoDto>.Failure("Erro ao salvar serviço.");

            var servicoDto = new ServicoDto
            {
                CodServico = servico.CodServico,
                NomeServico = servico.NomeServico,
                Descricao = servico.Descricao
            };

            return ServiceResult<ServicoDto>.Success(servicoDto, $"Serviço {request.NomeServico} criado com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ServicoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<ServicoDto>.Failure("Erro interno ao criar serviço.");
        }
    }

    public async Task<ServiceResult<ServicoDto>> AtualizarServicoAsync(ServicoRequest request)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoParaEdicaoAsync(request.CodServico);
            if (servico == null)
                return ServiceResult<ServicoDto>.NotFound($"Serviço {request.CodServico} não encontrado.");

            var usuario = _currentUser.NomeUsuario;
            servico.ChangeName(request.NomeServico, usuario);
            servico.ChangeDescription(request.Descricao, usuario);

            _servicoRepository.Atualizar(servico);

            if (!await _servicoRepository.UnitOfWork.Commit())
                return ServiceResult<ServicoDto>.Failure("Erro ao atualizar serviço.");

            var servicoDto = new ServicoDto
            {
                CodServico = servico.CodServico,
                NomeServico = servico.NomeServico,
                Descricao = servico.Descricao
            };

            return ServiceResult<ServicoDto>.Success(servicoDto, $"Serviço {request.NomeServico} atualizado com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ServicoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<ServicoDto>.Failure("Erro interno ao atualizar serviço.");
        }
    }

    public async Task<ServiceResult<ServicoDto>> ObterServicoAsync(string codServico)
    {
        var servicoDto = await _servicoRepository.ObterServicoDtoAsync(codServico);

        if (servicoDto == null)
            return ServiceResult<ServicoDto>.NotFound($"Serviço {codServico} não encontrado.");

        return ServiceResult<ServicoDto>.Success(servicoDto);
    }

    public async Task<ServiceResult<List<ServicoDto>>> ListarServicosAsync()
    {
        var servicos = await _servicoRepository.ListarServicosAsync();
        return ServiceResult<List<ServicoDto>>.Success(servicos);
    }

    public async Task<ServiceResult> ExcluirServicoAsync(string codServico)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoParaEdicaoAsync(codServico);
            if (servico == null)
                return ServiceResult.NotFound($"Serviço {codServico} não encontrado.");

            _servicoRepository.Remover(servico);

            if (!await _servicoRepository.UnitOfWork.Commit())
                return ServiceResult.Failure("Erro ao excluir serviço.");

            return ServiceResult.Success($"Serviço {servico.NomeServico} removido com sucesso.");
        }
        catch (Exception)
        {
            return ServiceResult.Failure("Erro interno ao excluir serviço.");
        }
    }
}
