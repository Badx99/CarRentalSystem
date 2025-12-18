using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Application.Features.Vehicles.Commands.CreateVehicle;
using CarRentalSystem.Application.Features.Vehicles.Commands.DeleteVehicle;
using CarRentalSystem.Application.Features.Vehicles.Commands.UpdateVehicle;
using CarRentalSystem.Application.Features.Vehicles.Commands.UpdateVehicleStatus;
using CarRentalSystem.Application.Features.Vehicles.Queries.GetAllVehicles;
using CarRentalSystem.Application.Features.Vehicles.Queries.GetAvailableVehicles;
using CarRentalSystem.Application.Features.Vehicles.Queries.GetVehicleById;
using CarRentalSystem.Application.Features.Vehicles.Queries.SearchVehicles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all vehicles with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAllVehiclesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAllVehiclesResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? vehicleTypeId = null)
    {
        var query = new GetAllVehiclesQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            VehicleTypeId = vehicleTypeId
        };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get available vehicles for a date range
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(GetAvailableVehiclesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GetAvailableVehiclesResponse>> GetAvailable(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? vehicleTypeId = null)
    {
        if (startDate >= endDate)
        {
            return BadRequest(new { error = "Start date must be before end date" });
        }

        var query = new GetAvailableVehiclesQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize,
            VehicleTypeId = vehicleTypeId
        };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get a vehicle by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetVehicleByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetVehicleByIdResponse>> GetById(Guid id)
    {
        try
        {
            var query = new GetVehicleByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new vehicle
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(CreateVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateVehicleResponse>> Create([FromBody] CreateVehicleCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { error = ex.Message });
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
    /// Update an existing vehicle
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(UpdateVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateVehicleResponse>> Update(Guid id, [FromBody] UpdateVehicleCommand command)
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update vehicle status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(UpdateVehicleStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateVehicleStatusResponse>> UpdateStatus(Guid id, [FromBody] UpdateVehicleStatusCommand command)
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
    }

    /// <summary>
    /// Delete a vehicle
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(DeleteVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteVehicleResponse>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteVehicleCommand(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }


    /// <summary>
    /// Search vehicles with filters and pagination
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<VehicleSearchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VehicleSearchDto>>> Search([FromQuery] SearchVehiclesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
