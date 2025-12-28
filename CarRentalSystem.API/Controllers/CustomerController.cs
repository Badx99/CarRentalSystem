using CarRentalSystem.Application.Features.Customers.Commands.RegisterCustomer;
using CarRentalSystem.Application.Features.Customers.Commands.UpdateCustomer;
using CarRentalSystem.Application.Features.Customers.Queries.GetAllCustomers;
using CarRentalSystem.Application.Features.Customers.Queries.GetCustomerById;
using CarRentalSystem.API.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterCustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterCustomerResponse>> Register([FromBody] RegisterCustomerCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CustomerDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailsResponse>> GetMyProfile()
    {
        var userId = User.GetUserId();
        var response = await _mediator.Send(new GetCustomerByIdQuery(userId));
        
        if (response == null) return NotFound();
        
        return Ok(response);
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerCommand command)
    {
        var userId = User.GetUserId();
        
        // Ensure user can only update their own profile
        if (userId != command.Id) return BadRequest(new { error = "Unauthorized update attempt." });

        var success = await _mediator.Send(command);
        
        if (!success) return NotFound();
        
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")] // ← Only admins/employees can access
    [ProducesResponseType(typeof(GetAllCustomersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAllCustomersResponse>> GetAllCustomers()
    {
        var response = await _mediator.Send(new GetAllCustomersQuery());
        return Ok(response);
    }
}

