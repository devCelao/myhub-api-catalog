using CatalogApplication.Services;
using CatalogDomain.Dtos;
using MicroserviceCore.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[Authorize]
[Route("api/catalogo/servicos/{codServico}/funcoes")]
public class FuncaoController(IFuncaoApplicationService funcaoService) : BaseController
{
    private readonly IFuncaoApplicationService _funcaoService = funcaoService;

    /// <summary>
    /// Listar funcoes de um servico
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<FuncaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarFuncoesDoServico(string codServico)
    {
        var result = await _funcaoService.ListarFuncoesDoServicoAsync(codServico);
        return ToActionResult(result);
    }

    /// <summary>
    /// Criar funcao para um servico
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FuncaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarFuncao(string codServico, [FromBody] FuncaoRequest funcao)
    {
        if (!ModelState.IsValid) return ValidationError();

        var result = await _funcaoService.CriarFuncaoAsync(codServico, funcao);
        return CreatedResult(result, nameof(ListarFuncoesDoServico), new { codServico });
    }

    /// <summary>
    /// Atualizar funcao de um servico
    /// </summary>
    [HttpPut("{codFuncao}")]
    [ProducesResponseType(typeof(FuncaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarFuncao(string codServico, string codFuncao, [FromBody] FuncaoRequest funcao)
    {
        if (!ModelState.IsValid) return ValidationError();

        if (funcao.CodFuncao != codFuncao)
        {
            return BadRequest(new ApiResponse<FuncaoDto>
            {
                Success = false,
                Message = "Codigo da funcao na URL não corresponde ao codigo no corpo da requisicao.",
                Errors = [new ApiError("VALIDATION", "Codigo da funcao na URL não corresponde ao codigo no corpo da requisicao.")]
            });
        }

        var result = await _funcaoService.AtualizarFuncaoAsync(codServico, funcao);
        return ToActionResult(result);
    }

    /// <summary>
    /// Remover funcao de um servico
    /// </summary>
    [HttpDelete("{codFuncao}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverFuncao(string codServico, string codFuncao)
    {
        var result = await _funcaoService.ExcluirFuncaoAsync(codServico, codFuncao);
        return NoContentResult(result);
    }
}
