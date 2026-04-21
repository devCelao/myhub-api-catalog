using DomainObjects.Data;
using CatalogDomain.Entities;
using CatalogDomain.Dtos;
using CatalogInfrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CatalogInfrastructure.Repositories;
public interface IFuncaoRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Funcao?> ObterPorCodigoAsync(string codigo);
    Task<Funcao?> ObterPorCodigoParaEdicaoAsync(string codigo);
    Task<List<FuncaoDto>> ListarFuncoesDoServicoAsync(string codServico);
    Task AdicionarAsync(Funcao funcao);
    void Atualizar(Funcao funcao);
    void Remover(Funcao funcao);
}
public class FuncaoRepository(CatalogContext context) : IFuncaoRepository
{
    private readonly CatalogContext _context = context;
    public IUnitOfWork UnitOfWork => _context;

    public async Task<Funcao?> ObterPorCodigoAsync(string codigo)
        => await _context.Funcoes
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.CodFuncao == codigo);

    public async Task<Funcao?> ObterPorCodigoParaEdicaoAsync(string codigo)
        => await _context.Funcoes
            .FirstOrDefaultAsync(f => f.CodFuncao == codigo);

    public async Task<List<FuncaoDto>> ListarFuncoesDoServicoAsync(string codServico)
    {
        var funcoes = await _context.Funcoes
            .Where(f => f.CodServico == codServico)
            .AsNoTracking()
            .ToListAsync();

        return [.. funcoes.Select(f => new FuncaoDto
        {
            CodFuncao = f.CodFuncao,
            CodServico = f.CodServico,
            Label = f.Label,
            Descricao = f.Descricao,
            Icone = f.Icone,
            NumOrdem = f.NumOrdem,
            IndAtivo = f.IndAtivo
        })];
    }

    public async Task AdicionarAsync(Funcao funcao)
        => await _context.Funcoes.AddAsync(funcao);

    public void Atualizar(Funcao funcao)
        => _context.Funcoes.Update(funcao);

    public void Remover(Funcao funcao)
        => _context.Funcoes.Remove(funcao);

    public void Dispose()
        => _context?.Dispose();
}
