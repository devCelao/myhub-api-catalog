using Microsoft.AspNetCore.Mvc;
using CatalogApplication.Services;
using CatalogDomain.Dtos;
using Microsoft.AspNetCore.Authorization;
using MicroserviceCore.Controller;

namespace CatalogAPI.Controllers;

[Authorize]
[Route("api/catalogo/servicos")]
public class ServicoController(
    IServicoApplicationService servicoService,
    IPlanoServicoApplicationService planoServicoService) : BaseController
{
    private readonly IServicoApplicationService _servicoService = servicoService;
    private readonly IPlanoServicoApplicationService _planoServicoService = planoServicoService;

    /// <summary>
    /// Obter todos os servicos
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarServicos()
    {
        var result = await _servicoService.ListarServicosAsync();
        return ToActionResult(result);
    }

    /// <summary>
    /// Obter servico por codigo
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{codServico}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterServico(string codServico)
    {
        var result = await _servicoService.ObterServicoAsync(codServico);
        return ToActionResult(result);
    }

    /// <summary>
    /// Criar novo servico
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarServico([FromBody] ServicoRequest request)
    {
        if (!ModelState.IsValid) return ValidationError();

        var result = await _servicoService.CriarServicoAsync(request);
        return CreatedResult(result, nameof(ObterServico), new { codServico = request.CodServico });
    }

    /// <summary>
    /// Atualizar servico existente
    /// </summary>
    [HttpPut("{codServico}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarServico(string codServico, [FromBody] ServicoRequest request)
    {
        if (!ModelState.IsValid) return ValidationError();

        if (request.CodServico != codServico)
        {
            return BadRequest(new ApiResponse<ServicoDto>
            {
                Success = false,
                Message = "Codigo do servico na URL não corresponde ao codigo no corpo da requisicao.",
                Errors = [new ApiError("VALIDATION", "Codigo do servico na URL não corresponde ao codigo no corpo da requisicao.")]
            });
        }

        var result = await _servicoService.AtualizarServicoAsync(request);
        return ToActionResult(result);
    }

    /// <summary>
    /// Excluir servico
    /// </summary>
    [HttpDelete("{codServico}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirServico(string codServico)
    {
        var result = await _servicoService.ExcluirServicoAsync(codServico);
        return NoContentResult(result);
    }

    /// <summary>
    /// Obter planos que possuem este servico
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{codServico}/planos")]
    [ProducesResponseType(typeof(List<PlanoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPlanosDoServico(string codServico)
    {
        var result = await _planoServicoService.ListarPlanosDoServicoAsync(codServico);
        return ToActionResult(result);
    }
}
