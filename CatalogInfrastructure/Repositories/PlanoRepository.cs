using DomainObjects.Data;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogDomain.Dtos;
using CatalogInfrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CatalogInfrastructure.Repositories;

public interface IPlanoRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Plano?> ObterPorCodigoAsync(CodigoPlano codigo);
    Task<Plano?> ObterPorCodigoParaEdicaoAsync(CodigoPlano codigo);
    Task<PlanoDto?> ObterPlanoComServicosAsync(CodigoPlano codigo);
    Task<List<PlanoDto>> ListarPlanosComServicosAsync();
    Task<List<PlanoDto>> ListarPlanosAtivosComServicosAsync();
    Task AdicionarAsync(Plano plano);
    void Atualizar(Plano plano);
    void Remover(Plano plano);
}

public class PlanoRepository(CatalogContext context) : IPlanoRepository
{
    private readonly CatalogContext _context = context;
    public IUnitOfWork UnitOfWork => _context;

    public async Task<Plano?> ObterPorCodigoAsync(CodigoPlano codigo)
        => await _context.Planos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodPlano == codigo);

    public async Task<Plano?> ObterPorCodigoParaEdicaoAsync(CodigoPlano codigo)
        => await _context.Planos
            .FirstOrDefaultAsync(p => p.CodPlano == codigo);

    public async Task<PlanoDto?> ObterPlanoComServicosAsync(CodigoPlano codigo)
    {
        var plano = await _context.Planos
            .Include(p => p.PlanoServicos)
            .ThenInclude(ps => ps.Servico)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodPlano == codigo);

        if (plano == null) return null;

        return new PlanoDto
        {
            CodPlano = plano.CodPlano,
            NomePlano = plano.NomePlano,
            IndAtivo = plano.IndAtivo,
            IndGeraCobranca = plano.IndGeraCobranca,
            ValorBase = plano.ValorBase.Valor,
            Servicos = [.. plano.PlanoServicos.Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })]
        };
    }

    public async Task<List<PlanoDto>> ListarPlanosComServicosAsync()
    {
        var planos = await _context.Planos
            .Include(p => p.PlanoServicos)
            .ThenInclude(ps => ps.Servico)
            .AsNoTracking()
            .ToListAsync();

        return [.. planos.Select(plano => new PlanoDto
        {
            CodPlano = plano.CodPlano,
            NomePlano = plano.NomePlano,
            IndAtivo = plano.IndAtivo,
            IndGeraCobranca = plano.IndGeraCobranca,
            ValorBase = plano.ValorBase.Valor,
            Servicos = [.. plano.PlanoServicos.Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })]
        })];
    }

    public async Task<List<PlanoDto>> ListarPlanosAtivosComServicosAsync()
    {
        var planos = await _context.Planos
            .Include(p => p.PlanoServicos)
            .ThenInclude(ps => ps.Servico)
            .Where(p => p.IndAtivo)
            .AsNoTracking()
            .ToListAsync();

        return [.. planos.Select(plano => new PlanoDto
        {
            CodPlano = plano.CodPlano,
            NomePlano = plano.NomePlano,
            IndAtivo = plano.IndAtivo,
            IndGeraCobranca = plano.IndGeraCobranca,
            ValorBase = plano.ValorBase.Valor,
            Servicos = [.. plano.PlanoServicos.Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })]
        })];
    }

    public async Task AdicionarAsync(Plano plano)
        => await _context.Planos.AddAsync(plano);

    public void Atualizar(Plano plano)
        => _context.Planos.Update(plano);

    public void Remover(Plano plano)
        => _context.Planos.Remove(plano);
}
