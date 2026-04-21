using CatalogDomain.Dtos;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogInfrastructure.Repositories;
using MicroserviceCore.Respostas;

namespace CatalogApplication.Services;

public interface IPlanoServicoApplicationService
{
    Task<ServiceResult<PlanoDto>> VincularServicosAoPlanoAsync(PlanoServicosRequest request);
    Task<ServiceResult<PlanoDto>> LimparServicosDoPlanoAsync(string codPlano);
    Task<ServiceResult<List<ServicoDto>>> ListarServicosDoPlanoAsync(string codPlano);
    Task<ServiceResult<List<PlanoDto>>> ListarPlanosDoServicoAsync(string codServico);
}

public class PlanoServicoApplicationService(
    IPlanoRepository planoRepository,
    IServicoRepository servicoRepository,
    IPlanoServicoRepository planoServicoRepository) : IPlanoServicoApplicationService
{
    private readonly IPlanoRepository _planoRepository = planoRepository ?? throw new ArgumentNullException(nameof(planoRepository));
    private readonly IServicoRepository _servicoRepository = servicoRepository ?? throw new ArgumentNullException(nameof(servicoRepository));
    private readonly IPlanoServicoRepository _planoServicoRepository = planoServicoRepository ?? throw new ArgumentNullException(nameof(planoServicoRepository));

    public async Task<ServiceResult<PlanoDto>> VincularServicosAoPlanoAsync(PlanoServicosRequest request)
    {
        try
        {
            var codigoPlano = new CodigoPlano(request.CodPlano);

            var plano = await _planoRepository.ObterPorCodigoAsync(codigoPlano);
            if (plano == null)
                return ServiceResult<PlanoDto>.NotFound($"Plano {request.CodPlano} não encontrado.");

            var servicosAtuais = await _planoServicoRepository.ObterServicosDoPlanoAsync(codigoPlano);
            var codigosAtuais = servicosAtuais.Select(s => s.CodServico).ToList();

            var servicosParaAdicionar = (request.CodServicos ?? [])
                .Where(cod => !codigosAtuais.Contains(cod))
                .ToList();

            var servicosParaRemover = codigosAtuais
                .Where(cod => !(request?.CodServicos?.Contains(cod) ?? false))
                .ToList();

            if (servicosParaAdicionar.Count == 0 && servicosParaRemover.Count == 0)
            {
                var planoSemAlteracao = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
                return ServiceResult<PlanoDto>.Success(planoSemAlteracao!, $"Nenhuma alteração nos serviços do plano {request.CodPlano}.");
            }

            foreach (var codServico in servicosParaAdicionar)
            {
                var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
                if (servico == null)
                    return ServiceResult<PlanoDto>.Failure($"Serviço {codServico} não encontrado.");

                var planoServico = new PlanoServico
                {
                    CodPlano = codigoPlano,
                    CodServico = codServico
                };
                await _planoServicoRepository.AdicionarAsync(planoServico);
            }

            foreach (var codServico in servicosParaRemover)
            {
                var vinculo = await _planoServicoRepository.ObterVinculoAsync(codigoPlano, codServico);
                if (vinculo != null)
                    _planoServicoRepository.Remover(vinculo);
            }

            if (!await _planoServicoRepository.UnitOfWork.Commit())
                return ServiceResult<PlanoDto>.Failure("Erro ao vincular serviços ao plano.");

            var planoAtualizado = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
            return ServiceResult<PlanoDto>.Success(planoAtualizado!, $"Serviços do plano {request.CodPlano} atualizados com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<PlanoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<PlanoDto>.Failure("Erro interno ao vincular serviços.");
        }
    }

    public async Task<ServiceResult<PlanoDto>> LimparServicosDoPlanoAsync(string codPlano)
    {
        try
        {
            var codigoPlano = new CodigoPlano(codPlano);

            var plano = await _planoRepository.ObterPorCodigoAsync(codigoPlano);
            if (plano == null)
                return ServiceResult<PlanoDto>.NotFound($"Plano {codPlano} não encontrado.");

            var servicos = await _planoServicoRepository.ObterServicosDoPlanoAsync(codigoPlano);

            if (servicos.Count == 0)
            {
                var planoSemServicos = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
                return ServiceResult<PlanoDto>.Success(planoSemServicos!, $"Plano {codPlano} não possui serviços.");
            }

            await _planoServicoRepository.RemoverTodosServicosDoPlanoAsync(codigoPlano);

            if (!await _planoServicoRepository.UnitOfWork.Commit())
                return ServiceResult<PlanoDto>.Failure("Erro ao remover serviços do plano.");

            var planoAtualizado = await _planoRepository.ObterPlanoComServicosAsync(codigoPlano);
            return ServiceResult<PlanoDto>.Success(planoAtualizado!, $"Todos os serviços do plano {codPlano} removidos.");
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<PlanoDto>.Failure(ex.Message);
        }
        catch (Exception)
        {
            return ServiceResult<PlanoDto>.Failure("Erro interno ao remover serviços.");
        }
    }

    public async Task<ServiceResult<List<ServicoDto>>> ListarServicosDoPlanoAsync(string codPlano)
    {
        try
        {
            var codigoPlano = new CodigoPlano(codPlano);

            var plano = await _planoRepository.ObterPorCodigoAsync(codigoPlano);
            if (plano == null)
                return ServiceResult<List<ServicoDto>>.NotFound($"Plano {codPlano} não encontrado.");

            var servicos = await _planoServicoRepository.ObterServicosDoPlanoAsync(codigoPlano);
            return ServiceResult<List<ServicoDto>>.Success(servicos);
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<List<ServicoDto>>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<List<PlanoDto>>> ListarPlanosDoServicoAsync(string codServico)
    {
        var servico = await _servicoRepository.ObterPorCodigoAsync(codServico);
        if (servico == null)
            return ServiceResult<List<PlanoDto>>.NotFound($"Serviço {codServico} não encontrado.");

        var planos = await _planoServicoRepository.ObterPlanosDoServicoAsync(codServico);
        return ServiceResult<List<PlanoDto>>.Success(planos);
    }
}
