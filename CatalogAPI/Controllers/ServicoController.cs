using Microsoft.AspNetCore.Mvc;
using CatalogApplication.Services;
using CatalogDomain.Dtos;
using MicroserviceCore.Controller;
using Microsoft.AspNetCore.Authorization;

namespace CatalogAPI.Controllers;
//[Authorize]
[Route("Catalogo/Servico")]
public class ServicoController(ICatalogService service) : RootController
{
    private readonly ICatalogService service = service;

    /// <summary>
    /// Obter todos os serviços
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarServicos()
    {
        var result = await service.ListarServicosAsync();

        return CustomResponde(result);
    }
    /// <summary>
    /// Obter serviço por código
    /// </summary>
    [HttpGet("{codServico}")]
    public async Task<IActionResult> ObterServico(string codServico)
    {
        if(codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        var result = await service.ObterServicoAsync(codServico);

        return CustomResponde(result);
    }
    /// <summary>
    /// Criar novo serviço
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarServico([FromBody] ServicoRequest request)
    {
        if (!ModelState.IsValid) return FromModelState(ModelState);

        var result = await service.CriarServicoAsync(request);

        return CustomResponde(result);
    }
    /// <summary>
    /// Atualizar serviço
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> AtualizarServico([FromBody] ServicoRequest request)
    {
        if (!ModelState.IsValid) return FromModelState(ModelState);

        var result = await service.AtualizarServicoAsync(request);

        return CustomResponde(result);
    }
    /// <summary>
    /// Excluir serviço
    /// </summary>
    [HttpDelete("{codServico}")]
    public async Task<IActionResult> ExcluirServico(string codServico)
    {
        if (codServico is null)
            return ErrorResponse([new("process_error", "Código do serviço obrigatório!")], 503);

        var result = await service.ExcluirServicoAsync(codServico);

        return CustomResponde(result);
    }
}
