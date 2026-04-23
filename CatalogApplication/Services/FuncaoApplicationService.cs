using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogInfrastructure.Repositories;
using MicroserviceCore.Respostas;

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
                return ServiceResult<FuncaoDto>.NotFound($"Servico {codServico} nao encontrado.");

            var funcaoExistente = await _funcaoRepository.ObterPorCodigoAsync(request.CodFuncao);
            if (funcaoExistente != null)
                return ServiceResult<FuncaoDto>.Failure($"Funcao {request.CodFuncao} ja existe.");
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
                return ServiceResult<FuncaoDto>.Failure("Erro ao criar funcao.");

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

            return ServiceResult<FuncaoDto>.Success(funcaoDto, $"Funcao {request.Label} criada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<FuncaoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<FuncaoDto>.Failure("Erro interno ao criar funcao.");
        }
    }

    public async Task<ServiceResult<FuncaoDto>> AtualizarFuncaoAsync(string codServico, FuncaoRequest request)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
            if (servico == null)
                return ServiceResult<FuncaoDto>.NotFound($"Servico {codServico} nao encontrado.");

            var funcao = await _funcaoRepository.ObterPorCodigoParaEdicaoAsync(request.CodFuncao);
            if (funcao == null)
                return ServiceResult<FuncaoDto>.NotFound($"Funcao {request.CodFuncao} nao encontrada.");
            if (funcao.CodServico != codServico)
                return ServiceResult<FuncaoDto>.Failure($"Funcao {request.CodFuncao} nao pertence ao servico {codServico}.");

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
                return ServiceResult<FuncaoDto>.Failure("Erro ao atualizar funcao.");

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

            return ServiceResult<FuncaoDto>.Success(funcaoDto, $"Funcao {request.Label} atualizada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<FuncaoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<FuncaoDto>.Failure("Erro interno ao atualizar funcao.");
        }
    }

    public async Task<ServiceResult<List<FuncaoDto>>> ListarFuncoesDoServicoAsync(string codServico)
    {
        var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
        if (servico == null)
            return ServiceResult<List<FuncaoDto>>.NotFound($"Servico {codServico} nao encontrado.");

        var funcoes = await _funcaoRepository.ListarFuncoesDoServicoAsync(codServico);
        return ServiceResult<List<FuncaoDto>>.Success(funcoes);
    }

    public async Task<ServiceResult> ExcluirFuncaoAsync(string codServico, string codFuncao)
    {
        try
        {
            var servico = await _servicoRepository.ObterPorCodigoParaEdicaoAsync(codServico);
            if (servico == null)
                return ServiceResult.NotFound($"Servico {codServico} nao encontrado.");

            var funcao = await _funcaoRepository.ObterPorCodigoParaEdicaoAsync(codFuncao);
            if (funcao == null)
                return ServiceResult.NotFound($"Funcao {codFuncao} nao encontrada.");
            if (funcao.CodServico != codServico)
                return ServiceResult.Failure($"Funcao {codFuncao} nao pertence ao servico {codServico}.");

            _funcaoRepository.Remover(funcao);
            servico.RemoveFunction(funcao);

            if (!await _funcaoRepository.UnitOfWork.Commit())
                return ServiceResult.Failure("Erro ao excluir funcao.");

            return ServiceResult.Success($"Funcao {codFuncao} removida com sucesso.");
        }
        catch (Exception)
        {
            return ServiceResult.Failure("Erro interno ao excluir funcao.");
        }
    }
}
