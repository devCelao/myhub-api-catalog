using Microsoft.AspNetCore.Mvc;
using CatalogApplication.Services;
using CatalogDomain.Dtos;
using Microsoft.AspNetCore.Authorization;
using MicroserviceCore.Controller;

namespace CatalogAPI.Controllers;

[Authorize]
[Route("api/catalogo/planos")]
public class PlanoController(
    IPlanoApplicationService planoService,
    IPlanoServicoApplicationService planoServicoService) : BaseController
{
    private readonly IPlanoApplicationService _planoService = planoService;
    private readonly IPlanoServicoApplicationService _planoServicoService = planoServicoService;

    /// <summary>
    /// Obter todos os planos
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPlanos()
    {
        var result = await _planoService.ListarPlanosAsync();
        return ToActionResult(result);
    }

    /// <summary>
    /// Obter apenas planos ativos
    /// </summary>
    [AllowAnonymous]
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(List<PlanoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPlanosAtivos()
    {
        var result = await _planoService.ListarPlanosAtivosAsync();
        return ToActionResult(result);
    }

    /// <summary>
    /// Obter plano por codigo
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{codPlano}")]
    [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPlano(string codPlano)
    {
        var result = await _planoService.ObterPlanoAsync(codPlano);
        return ToActionResult(result);
    }

    /// <summary>
    /// Criar novo plano
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarPlano([FromBody] PlanoRequest request)
    {
        if (!ModelState.IsValid) return ValidationError();

        var result = await _planoService.CriarPlanoAsync(request);
        return CreatedResult(result, nameof(ObterPlano), new { codPlano = request.CodPlano });
    }

    /// <summary>
    /// Atualizar plano existente
    /// </summary>
    [HttpPut("{codPlano}")]
    [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarPlano(
        string codPlano,
        [FromBody] PlanoRequest request)
    {
        if (!ModelState.IsValid) return ValidationError();

        if (request.CodPlano != codPlano)
        {
            return BadRequest(new
            {
                success = false,
                message = "Codigo do plano na URL não corresponde ao codigo no corpo da requisicao.",
                errors = new[] { "Codigo do plano na URL não corresponde ao codigo no corpo da requisicao." }
            });
        }

        var result = await _planoService.AtualizarPlanoAsync(request);
        return ToActionResult(result);
    }

    /// <summary>
    /// Excluir plano
    /// </summary>
    [HttpDelete("{codPlano}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExcluirPlano(string codPlano)
    {
        var result = await _planoService.ExcluirPlanoAsync(codPlano);
        return NoContentResult(result);
    }

    /// <summary>
    /// Obter servicos de um plano
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{codPlano}/servicos")]
    [ProducesResponseType(typeof(List<ServicoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterServicosDoPlano(string codPlano)
    {
        var result = await _planoServicoService.ListarServicosDoPlanoAsync(codPlano);
        return ToActionResult(result);
    }

    /// <summary>
    /// Vincular servicos a um plano
    /// </summary>
    [HttpPut("{codPlano}/servicos")]
    [ProducesResponseType(typeof(PlanoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VincularServicosAoPlano(
        string codPlano,
        [FromBody] List<string> codServicos)
    {
        if (codServicos == null || codServicos.Count == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Lista de servicos não pode ser vazia.",
                errors = new[] { "Lista de servicos não pode ser vazia." }
            });
        }

        var request = new PlanoServicosRequest
        {
            CodPlano = codPlano,
            CodServicos = codServicos
        };

        var result = await _planoServicoService.VincularServicosAoPlanoAsync(request);
        return ToActionResult(result);
    }
}
