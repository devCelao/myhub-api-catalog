using DomainObjects.Data;
using CatalogDomain.Entities;
using CatalogDomain.Dtos;
using CatalogInfrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CatalogInfrastructure.Repositories;
public interface IServicoRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Servico?> ObterPorCodigoAsync(string codigo);
    Task<Servico?> ObterPorCodigoParaEdicaoAsync(string codigo);
    Task<Servico?> ObterPorNomeAsync(string nome);
    Task<ServicoDto?> ObterServicoDtoAsync(string codigo);
    Task<List<ServicoDto>> ListarServicosAsync();
    Task AdicionarAsync(Servico servico);
    void Atualizar(Servico servico);
    void Remover(Servico servico);
}
public class ServicoRepository(CatalogContext context) : IServicoRepository
{
    private readonly CatalogContext _context = context;
    public IUnitOfWork UnitOfWork => _context;

    public async Task<Servico?> ObterPorCodigoAsync(string codigo)
        => await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CodServico == codigo);

    public async Task<Servico?> ObterPorCodigoParaEdicaoAsync(string codigo)
        => await _context.Servicos
            .FirstOrDefaultAsync(s => s.CodServico == codigo);

    public async Task<Servico?> ObterPorNomeAsync(string nome)
        => await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.NomeServico == nome);

    public async Task<ServicoDto?> ObterServicoDtoAsync(string codigo)
    {
        var servico = await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CodServico == codigo);

        if (servico == null) return null;

        return new ServicoDto
        {
            CodServico = servico.CodServico,
            NomeServico = servico.NomeServico,
            Descricao = servico.Descricao
        };
    }

    public async Task<List<ServicoDto>> ListarServicosAsync()
    {
        var servicos = await _context.Servicos
            .AsNoTracking()
            .ToListAsync();

        return [.. servicos.Select(s => new ServicoDto
        {
            CodServico = s.CodServico,
            NomeServico = s.NomeServico,
            Descricao = s.Descricao
        })];
    }

    public async Task AdicionarAsync(Servico servico)
        => await _context.Servicos.AddAsync(servico);

    public void Atualizar(Servico servico)
        => _context.Servicos.Update(servico);

    public void Remover(Servico servico)
        => _context.Servicos.Remove(servico);

    public void Dispose()
        => _context?.Dispose();
}
