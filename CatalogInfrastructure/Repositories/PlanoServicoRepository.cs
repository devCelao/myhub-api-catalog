using DomainObjects.Data;
using CatalogDomain.Entities;
using CatalogDomain.ValueObjects;
using CatalogDomain.Dtos;
using CatalogInfrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CatalogInfrastructure.Repositories;
public interface IPlanoServicoRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<PlanoServico?> ObterVinculoAsync(CodigoPlano codPlano, string codServico);
    Task<List<ServicoDto>> ObterServicosDoPlanoAsync(CodigoPlano codPlano);
    Task<List<PlanoDto>> ObterPlanosDoServicoAsync(string codServico);
    Task AdicionarAsync(PlanoServico planoServico);
    void Remover(PlanoServico planoServico);
    Task RemoverTodosServicosDoPlanoAsync(CodigoPlano codPlano);
}
public class PlanoServicoRepository(CatalogContext context) : IPlanoServicoRepository
{
    private readonly CatalogContext _context = context;
    public IUnitOfWork UnitOfWork => _context;

    public async Task<PlanoServico?> ObterVinculoAsync(CodigoPlano codPlano, string codServico)
        => await _context.PlanoServicos
            .FirstOrDefaultAsync(ps => ps.CodPlano == codPlano && ps.CodServico == codServico);

    public async Task<List<ServicoDto>> ObterServicosDoPlanoAsync(CodigoPlano codPlano)
    {
        var servicos = await _context.PlanoServicos
            .Where(ps => ps.CodPlano == codPlano)
            .Select(ps => ps.Servico)
            .AsNoTracking()
            .ToListAsync();

        return [.. servicos.Select(s => new ServicoDto
        {
            CodServico = s.CodServico,
            NomeServico = s.NomeServico,
            Descricao = s.Descricao
        })];
    }

    public async Task<List<PlanoDto>> ObterPlanosDoServicoAsync(string codServico)
    {
        var planos = await _context.PlanoServicos
            .Where(ps => ps.CodServico == codServico)
            .Select(ps => ps.Plano)
            .AsNoTracking()
            .ToListAsync();

        return [.. planos.Select(p => new PlanoDto
        {
            CodPlano = p.CodPlano,
            NomePlano = p.NomePlano,
            IndAtivo = p.IndAtivo,
            IndGeraCobranca = p.IndGeraCobranca,
            ValorBase = p.ValorBase.Valor,
            Servicos = []
        })];
    }

    public async Task AdicionarAsync(PlanoServico planoServico)
        => await _context.PlanoServicos.AddAsync(planoServico);

    public void Remover(PlanoServico planoServico)
        => _context.PlanoServicos.Remove(planoServico);

    public async Task RemoverTodosServicosDoPlanoAsync(CodigoPlano codPlano)
    {
        var vinculos = await _context.PlanoServicos
            .Where(ps => ps.CodPlano == codPlano)
            .ToListAsync();

        _context.PlanoServicos.RemoveRange(vinculos);
    }

    public void Dispose()
        => _context?.Dispose();
}
