using CatalogApplication.Services;
using CatalogDomain.Dtos;
using MicroserviceCore.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;
[Authorize]
[Route("Catalogo/Funcao")]
public class FuncaoController(ICatalogService service) : RootController
{
    private readonly ICatalogService service = service;

    [HttpGet("Servico/{codServico}")]
    public async Task<IActionResult> ListarFuncoesDoServico(string codServico)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        var resultado = await service.ListarFuncoesDoServicoAsync(codServico);
        return CustomResponde(resultado);
    }

    [HttpPost("Servico/{codServico}")]
    public async Task<IActionResult> CriarFuncao(string codServico, [FromBody] FuncaoRequest funcao)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        var resultado = await service.CriarFuncaoAsync(codServico, funcao);
        return CustomResponde(resultado);
    }

    [HttpPut("Servico/{codServico}")]
    public async Task<IActionResult> AtualizarFuncao(string codServico, [FromBody] FuncaoRequest funcao)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        var resultado = await service.AtualizarFuncaoAsync(codServico, funcao);
        return CustomResponde(resultado);
    }

    [HttpDelete("Servico/{codServico}/{codFuncao}")]
    public async Task<IActionResult> RemoverFuncao(string codServico, string codFuncao)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        if (codFuncao is null)
            return ErrorResponse([new("process_error", "Código da função obrigatório!")], 503);

        var resultado = await service.ExcluirFuncaoAsync(codServico, codFuncao);
        return CustomResponde(resultado);
    }

    [HttpPut("Servico/{codServico}/Reordenar")]
    public async Task<IActionResult> Reordenar(string codServico, [FromBody] FuncaoRequest[] funcoes)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        if (funcoes == null || funcoes.Length == 0)
            return ErrorResponse([new("process_error", "Lista de funções não pode estar vazia!")], 503);

        var resultado = await service.ReordenarFuncoesAsync(codServico, funcoes);
        return CustomResponde(resultado);
    }

}
