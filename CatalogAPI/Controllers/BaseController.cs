using CatalogApplication.Common;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }

        if (result.IsNotFound)
        {
            return NotFound(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    protected IActionResult ToActionResult(ServiceResult result)
    {
        if (result.IsSuccess)
        {
            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        if (result.IsNotFound)
        {
            return NotFound(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message,
            errors = result.Errors
        });
    }

    protected IActionResult CreatedResult<T>(ServiceResult<T> result, string actionName, object routeValues)
    {
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(actionName, routeValues, new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    protected IActionResult NoContentResult(ServiceResult result)
    {
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return NoContent();
    }

    protected IActionResult ValidationError()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return BadRequest(new
        {
            success = false,
            message = "Erro de validacao.",
            errors
        });
    }
}
