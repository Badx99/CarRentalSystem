using CarRentalSystem.Application.Features.Employees.Commands.CreateEmployee;
using CarRentalSystem.Application.Features.Employees.Commands.DeleteEmployee;
using CarRentalSystem.Application.Features.Employees.Commands.UpdateEmployee;
using CarRentalSystem.Application.Features.Employees.Queries.GetAllEmployees;
using CarRentalSystem.Application.Features.Employees.Queries.GetEmployeeById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all employees with pagination (Admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllEmployeesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAllEmployeesResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllEmployeesQuery
        {
            Page = page,
            PageSize = pageSize
        };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get employee by ID (Admin only)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetEmployeeByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetEmployeeByIdResponse>> GetById(Guid id)
    {
        try
        {
            var query = new GetEmployeeByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new employee (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateEmployeeResponse>> Create([FromBody] CreateEmployeeCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing employee (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateEmployeeResponse>> Update(Guid id, [FromBody] UpdateEmployeeCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { error = "ID in URL does not match ID in request body" });
        }

        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an employee (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteEmployeeResponse>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteEmployeeCommand(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
