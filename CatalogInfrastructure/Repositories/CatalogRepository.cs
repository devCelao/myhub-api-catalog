using DomainObjects.Data;
using CatalogDomain.Entities;
using CatalogInfrastructure.Context;
using Microsoft.EntityFrameworkCore;
using DataTransferObjects.CatalogDomain;

namespace CatalogInfrastructure.Repositories;

public interface ICatalogRepository
{
    IUnitOfWork UnitOfWork { get; }
    
    // Plano
    Task<Plano?> ObterPlanoPorCodigo(string codPlano);
    Task<Plano?> ObterPlanoParaEdicao(string codPlano);
    Task<PlanoDto?> ObterPlanoComServicos(string codPlano);
    Task<List<PlanoDto>> ListarPlanosComServicos();
    void AdicionarPlano(Plano plano);
    void AtualizarPlano(Plano plano);
    void RemoverPlano(Plano plano);
    Task RemoverServicosDoPlano(string codPlano);
    void AdicionarPlanoServico(PlanoServico planoServico);

    // Servico
    Task<Servico?> ObterServicoPorCodigo(string codServico);
    Task<Servico?> ObterServicoParaEdicao(string codServico);
    Task<List<ServicoDto>> ListarServicos();
    Task<List<Servico>> ObterServicosPorCodigos(List<string> codServicos);
    void AdicionarServico(Servico servico);
    void AtualizarServico(Servico servico);
    void RemoverServico(Servico servico);

    // Vínculo Plano-Serviço
    Task<PlanoServico?> ObterPlanoServico(string codPlano, string codServico);
    void RemoverPlanoServico(PlanoServico planoServico);
    Task<List<ServicoDto>> ObterServicosDoPlano(string codPlano);
    Task<List<PlanoDto>> ObterPlanosDoServico(string codServico);

    // Funcao
    Task<Funcao?> ObterFuncaoPorCodigo(string codFuncao);
    Task<Funcao?> ObterFuncaoParaEdicao(string codFuncao);
    Task<List<Funcao>> ListarFuncoesDoServico(string codServico);
    void AdicionarFuncao(Funcao funcao);
    void AtualizarFuncao(Funcao funcao);
    void RemoverFuncao(Funcao funcao);
}

public class CatalogRepository(CatalogContext context) : ICatalogRepository
{
    private readonly CatalogContext context = context;
    public IUnitOfWork UnitOfWork => context;

    #region Plano

    public async Task<Plano?> ObterPlanoPorCodigo(string codPlano)
        => await context.Planos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodPlano == codPlano);

    public async Task<Plano?> ObterPlanoParaEdicao(string codPlano)
        => await context.Planos
            .FirstOrDefaultAsync(p => p.CodPlano == codPlano);

    public async Task<PlanoDto?> ObterPlanoComServicos(string codPlano)
    {
        var plano = await context.Planos
            .Include(p => p.PlanoServicos)
            .ThenInclude(ps => ps.Servico)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CodPlano == codPlano);

        if (plano == null) return null;

        return new PlanoDto
        {
            CodPlano = plano.CodPlano,
            NomePlano = plano.NomePlano,
            IndAtivo = plano.IndAtivo,
            IndGeraCobranca = plano.IndGeraCobranca,
            ValorBase = plano.ValorBase,
            Servicos = [.. plano.PlanoServicos.Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })]
        };
    }

    public async Task<List<PlanoDto>> ListarPlanosComServicos()
    {
        var planos = await context.Planos
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
            ValorBase = plano.ValorBase,
            Servicos = [.. plano.PlanoServicos.Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })]
        })];
    }

    public void AdicionarPlano(Plano plano) => context.Planos.Add(plano);
    public void AtualizarPlano(Plano plano) => context.Planos.Update(plano);
    public void RemoverPlano(Plano plano) => context.Planos.Remove(plano);

    public async Task RemoverServicosDoPlano(string codPlano)
    {
        var planoServicos = await context.PlanoServicos
            .Where(ps => ps.CodPlano == codPlano)
            .ToListAsync();
        
        context.PlanoServicos.RemoveRange(planoServicos);
    }

    public void AdicionarPlanoServico(PlanoServico planoServico) => context.PlanoServicos.Add(planoServico);

    #endregion

    #region Servico

    public async Task<Servico?> ObterServicoPorCodigo(string codServico)
        => await context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CodServico == codServico);

    public async Task<Servico?> ObterServicoParaEdicao(string codServico)
        => await context.Servicos
            .FirstOrDefaultAsync(s => s.CodServico == codServico);

    public async Task<List<ServicoDto>> ListarServicos()
    {
        var servicos = await context.Servicos
            .AsNoTracking()
            .ToListAsync();

        return [.. servicos.Select(servico => new ServicoDto
        {
            CodServico = servico.CodServico,
            NomeServico = servico.NomeServico,
            Descricao = servico.Descricao
        })];
    }

    public async Task<List<Servico>> ObterServicosPorCodigos(List<string> codServicos)
        => await context.Servicos
            .AsNoTracking()
            .Where(s => codServicos.Contains(s.CodServico))
            .ToListAsync();

    public void AdicionarServico(Servico servico) => context.Servicos.Add(servico);
    public void AtualizarServico(Servico servico) => context.Servicos.Update(servico);
    public void RemoverServico(Servico servico) => context.Servicos.Remove(servico);

    #endregion

    #region Vínculo Plano-Serviço

    public async Task<PlanoServico?> ObterPlanoServico(string codPlano, string codServico)
        => await context.PlanoServicos
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.CodPlano == codPlano && ps.CodServico == codServico);

    public void RemoverPlanoServico(PlanoServico planoServico) => context.PlanoServicos.Remove(planoServico);

    public async Task<List<ServicoDto>> ObterServicosDoPlano(string codPlano)
    {
        var servicos = await context.PlanoServicos
            .Include(ps => ps.Servico)
            .Where(ps => ps.CodPlano == codPlano)
            .AsNoTracking()
            .Select(ps => new ServicoDto
            {
                CodServico = ps.Servico.CodServico,
                NomeServico = ps.Servico.NomeServico,
                Descricao = ps.Servico.Descricao
            })
            .ToListAsync();

        return servicos;
    }

    public async Task<List<PlanoDto>> ObterPlanosDoServico(string codServico)
    {
        var planos = await context.PlanoServicos
            .Include(ps => ps.Plano)
            .Where(ps => ps.CodServico == codServico)
            .AsNoTracking()
            .Select(ps => new PlanoDto
            {
                CodPlano = ps.Plano.CodPlano,
                NomePlano = ps.Plano.NomePlano,
                IndAtivo = ps.Plano.IndAtivo,
                IndGeraCobranca = ps.Plano.IndGeraCobranca,
                ValorBase = ps.Plano.ValorBase,
                Servicos = new List<ServicoDto>() // Não incluir serviços para evitar recursão
            })
            .ToListAsync();

        return planos;
    }

    #endregion

    #region Funcao

    public async Task<Funcao?> ObterFuncaoPorCodigo(string codFuncao)
        => await context.Funcoes
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.CodFuncao == codFuncao);

    public async Task<Funcao?> ObterFuncaoParaEdicao(string codFuncao)
        => await context.Funcoes
            .FirstOrDefaultAsync(f => f.CodFuncao == codFuncao);

    public async Task<List<Funcao>> ListarFuncoesDoServico(string codServico)
        => await context.Funcoes
            .AsNoTracking()
            .Where(f => f.CodServico == codServico)
            .OrderBy(f => f.NumOrdem)
            .ToListAsync();

    public void AdicionarFuncao(Funcao funcao) => context.Funcoes.Add(funcao);
    public void AtualizarFuncao(Funcao funcao) => context.Funcoes.Update(funcao);
    public void RemoverFuncao(Funcao funcao) => context.Funcoes.Remove(funcao);

    #endregion
}
