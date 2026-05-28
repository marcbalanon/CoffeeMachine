using CoffeeMachine.Api.Interfaces;
using CoffeeMachine.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeMachine.Api.Controllers;

/// <summary>
/// Exposes the coffee machine endpoint.
/// <see cref="ICoffeeMachineService"/> and only translates the result
/// into the appropriate HTTP response.
/// </summary>
[ApiController]
[Route("")]
public sealed class CoffeeController : ControllerBase
{
    private readonly ICoffeeMachineService _machine;

    public CoffeeController(ICoffeeMachineService machine)
        => _machine = machine;

    /// <summary>Brew a coffee.</summary>
    /// <response code="200">Coffee is ready.</response>
    /// <response code="418">April 1st — I'm a teapot.</response>
    /// <response code="503">Every 5th call — machine is out of coffee.</response>
    [HttpGet("brew-coffee")]
    [ProducesResponseType(typeof(BrewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status418ImATeapot)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> BrewCoffee(CancellationToken ct)
    {
        var result = await _machine.Brew();

        switch (result.Outcome)
        {
            case BrewOutcome.Ready:
                return Ok(result.Response);
            case BrewOutcome.OutOfCoffee:
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            case BrewOutcome.Teapot:
                return StatusCode(StatusCodes.Status418ImATeapot);
            default:
                throw new InvalidOperationException($"Unhandled outcome: {result.Outcome}");
        };
    }
}
