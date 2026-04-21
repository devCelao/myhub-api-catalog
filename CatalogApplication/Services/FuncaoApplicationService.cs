using CatalogApplication.Common;
using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;

namespace CatalogApplication.Services;

public interface IFuncaoApplicationService
{
    Task<ServiceResult<FuncaoDto>> CriarFuncaoAsync(string codServico, FuncaoRequest request);
    Task<ServiceResult<FuncaoDto>> AtualizarFuncaoAsync(string codServico, FuncaoRequest request);
    Task<ServiceResult<List<FuncaoDto>>> ListarFuncoesDoServicoAsync(string codServico);
    Task<ServiceResult> ExcluirFuncaoAsync(string codServico, string codFuncao);
}

public class FuncaoApplicationService(
    IServicoRepository servicoRepository,
    IFuncaoRepository funcaoRepository,
    ICurrentUserService currentUser) : IFuncaoApplicationService
{
    private readonly IServicoRepository _servicoRepository = servicoRepository;
    private readonly IFuncaoRepository _funcaoRepository = funcaoRepository;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<ServiceResult<FuncaoDto>> CriarFuncaoAsync(string codServico, FuncaoRequest request)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
            if (servico == null)
                return ServiceResult<FuncaoDto>.NotFound($"Serviço {codServico} não encontrado.");

            var funcaoExistente = await _funcaoRepository.ObterPorCodigoAsync(request.CodFuncao);
            if (funcaoExistente != null)
                return ServiceResult<FuncaoDto>.Failure($"Função {request.CodFuncao} já existe.");
            var funcoesDoServico = await _funcaoRepository.ListarFuncoesDoServicoAsync(codServico);
            var ordensEmUso = funcoesDoServico.Select(f => f.NumOrdem).OrderBy(o => o).ToList();

            int ordemFinal = request.NumOrdem;
            if (ordensEmUso.Contains(ordemFinal))
            {
                ordemFinal = 1;
                while (ordensEmUso.Contains(ordemFinal))
                {
                    ordemFinal++;
                }
            }

            var usuario = _currentUser.NomeUsuario;
            var funcao = new Funcao(request.CodFuncao, codServico, request.Label, usuario);
            funcao.ChangeDescription(request.Descricao, usuario);
            funcao.ChangeIcon(request.Icone, usuario);
            funcao.ChangeOrder(ordemFinal, usuario);
            funcao.ChangeStatus(request.IndAtivo, usuario);

            await _funcaoRepository.AdicionarAsync(funcao);

            if (!await _funcaoRepository.UnitOfWork.Commit())
                return ServiceResult<FuncaoDto>.Failure("Erro ao criar função.");

            var funcaoDto = new FuncaoDto
            {
                CodFuncao = funcao.CodFuncao,
                CodServico = funcao.CodServico,
                Label = funcao.Label,
                Descricao = funcao.Descricao,
                Icone = funcao.Icone,
                NumOrdem = funcao.NumOrdem,
                IndAtivo = funcao.IndAtivo
            };

            return ServiceResult<FuncaoDto>.Success(funcaoDto, $"Função {request.Label} criada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<FuncaoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<FuncaoDto>.Failure("Erro interno ao criar função.");
        }
    }

    public async Task<ServiceResult<FuncaoDto>> AtualizarFuncaoAsync(string codServico, FuncaoRequest request)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
            if (servico == null)
                return ServiceResult<FuncaoDto>.NotFound($"Serviço {codServico} não encontrado.");

            var funcao = await _funcaoRepository.ObterPorCodigoParaEdicaoAsync(request.CodFuncao);
            if (funcao == null)
                return ServiceResult<FuncaoDto>.NotFound($"Função {request.CodFuncao} não encontrada.");
            if (funcao.CodServico != codServico)
                return ServiceResult<FuncaoDto>.Failure($"Função {request.CodFuncao} não pertence ao serviço {codServico}.");

            var funcoesDoServico = await _funcaoRepository.ListarFuncoesDoServicoAsync(codServico);
            var ordensEmUso = funcoesDoServico
                .Where(f => f.CodFuncao != request.CodFuncao)
                .Select(f => f.NumOrdem)
                .OrderBy(o => o)
                .ToList();

            int ordemFinal = request.NumOrdem;
            if (ordensEmUso.Contains(ordemFinal))
            {
                ordemFinal = 1;
                while (ordensEmUso.Contains(ordemFinal))
                {
                    ordemFinal++;
                }
            }

            var usuario = _currentUser.NomeUsuario;
            funcao.ChangeLabel(request.Label, usuario);
            funcao.ChangeDescription(request.Descricao, usuario);
            funcao.ChangeIcon(request.Icone, usuario);
            funcao.ChangeOrder(ordemFinal, usuario);
            funcao.ChangeStatus(request.IndAtivo, usuario);

            _funcaoRepository.Atualizar(funcao);

            if (!await _funcaoRepository.UnitOfWork.Commit())
                return ServiceResult<FuncaoDto>.Failure("Erro ao atualizar função.");

            var funcaoDto = new FuncaoDto
            {
                CodFuncao = funcao.CodFuncao,
                CodServico = funcao.CodServico,
                Label = funcao.Label,
                Descricao = funcao.Descricao,
                Icone = funcao.Icone,
                NumOrdem = funcao.NumOrdem,
                IndAtivo = funcao.IndAtivo
            };

            return ServiceResult<FuncaoDto>.Success(funcaoDto, $"Função {request.Label} atualizada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<FuncaoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<FuncaoDto>.Failure("Erro interno ao atualizar função.");
        }
    }

    public async Task<ServiceResult<List<FuncaoDto>>> ListarFuncoesDoServicoAsync(string codServico)
    {
        var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
        if (servico == null)
            return ServiceResult<List<FuncaoDto>>.NotFound($"Serviço {codServico} não encontrado.");

        var funcoes = await _funcaoRepository.ListarFuncoesDoServicoAsync(codServico);
        return ServiceResult<List<FuncaoDto>>.Success(funcoes);
    }

    public async Task<ServiceResult> ExcluirFuncaoAsync(string codServico, string codFuncao)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoParaEdicaoAsync(codServico);
            if (servico == null)
                return ServiceResult.NotFound($"Serviço {codServico} não encontrado.");

            var funcao = await _funcaoRepository.ObterPorCodigoParaEdicaoAsync(codFuncao);
            if (funcao == null)
                return ServiceResult.NotFound($"Função {codFuncao} não encontrada.");
            if (funcao.CodServico != codServico)
                return ServiceResult.Failure($"Função {codFuncao} não pertence ao serviço {codServico}.");

            _funcaoRepository.Remover(funcao);
            servico.RemoveFunction(funcao);

            if (!await _funcaoRepository.UnitOfWork.Commit())
                return ServiceResult.Failure("Erro ao excluir função.");

            return ServiceResult.Success($"Função {codFuncao} removida com sucesso.");
        }
        catch (Exception)
        {
            return ServiceResult.Failure("Erro interno ao excluir função.");
        }
    }
}
