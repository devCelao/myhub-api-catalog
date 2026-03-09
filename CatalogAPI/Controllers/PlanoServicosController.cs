using CatalogApplication.Services;
using CatalogDomain.Dtos;
using MicroserviceCore.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;
//[Authorize]
[Route("Catalogo/PlanoServico")]
public class PlanoServicosController(ICatalogService service) : RootController
{
    private readonly ICatalogService service = service;
    /// <summary>
    /// Vincular múltiplos serviços a um plano
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> VincularServicosAoPlano([FromBody] PlanoServicosRequest request)
    {
        if (!ModelState.IsValid) return FromModelState(ModelState);

        var result = await service.VincularServicosAoPlanoAsync(request);

        return CustomResponde(result);
    }
    /// <summary>
    /// Listar serviços de um plano
    /// </summary>
    [HttpGet("{codPlano}/servicos")]
    public async Task<IActionResult> ListarServicosDoPlano(string codPlano)
    {
        if(codPlano is null)
            return ErrorResponse([new("validation_error", "O código do plano é obrigatório.")], 400);

        var result = await service.ListarServicosDoPlanoAsync(codPlano);

        return CustomResponde(result);
    }
}
