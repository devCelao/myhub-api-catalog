using Microsoft.AspNetCore.Mvc;
using CatalogApplication.Services;
using CatalogDomain.Dtos;
using MicroserviceCore.Controller;
using Microsoft.AspNetCore.Authorization;

namespace CatalogAPI.Controllers;
[Authorize]
[Route("Catalogo/Plano")]
public class PlanoController(ICatalogService service) : RootController
{
    private readonly ICatalogService service = service;

    /// <summary>
    /// Obter todos os planos
    /// </summary>
    [HttpGet]
    //[AllowAnonymous]
    public async Task<IActionResult> ListarPlanos()
    {
        var result = await service.ListarPlanosAsync();

        return CustomResponde(result);
    }
    /// <summary>
    /// Obter plano por código
    /// </summary>
    [HttpGet("{codPlano}")]
    public async Task<IActionResult> ObterPlano(string codPlano)
    {
        if (codPlano is null)
            return ErrorResponse([new("process_error", "Código do plano obrigatório!")], 503);

        var result = await service.ObterPlanoAsync(codPlano);

        return CustomResponde(result);
    }
    /// <summary>
    /// Criar novo plano
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarPlano([FromBody] PlanoRequest request)
    {
        if (!ModelState.IsValid) return FromModelState(ModelState);

        var result = await service.CriarPlanoAsync(request);

        return CustomResponde(result);
    }
    /// <summary>
    /// Atualizar plano
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> AtualizarPlano([FromBody] PlanoRequest request)
    {
        if (!ModelState.IsValid) return FromModelState(ModelState);

        var result = await service.AtualizarPlanoAsync(request);

        return CustomResponde(result);
    }
    /// <summary>
    /// Excluir plano
    /// </summary>
    [HttpDelete("{codPlano}")]
    public async Task<IActionResult> ExcluirPlano(string codPlano)
    {
        if (codPlano is null)
            return ErrorResponse([new("process_error", "Código do plano obrigatório!")], 503);

        var result = await service.ExcluirPlanoAsync(codPlano);

        return CustomResponde(result);
    }
}