using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Application.Features.Reservations.Commands.CancelReservation;
using CarRentalSystem.Application.Features.Reservations.Commands.CompleteReservation;
using CarRentalSystem.Application.Features.Reservations.Commands.ConfirmReservation;
using CarRentalSystem.Application.Features.Reservations.Commands.CreateReservation;
using CarRentalSystem.Application.Features.Reservations.Commands.GenerateReservationQRCode;
using CarRentalSystem.Application.Features.Reservations.Commands.StartReservation;
using CarRentalSystem.Application.Features.Reservations.Queries.GenerateReservationReceipt;
using CarRentalSystem.Application.Features.Reservations.Queries.GetAllReservations;
using CarRentalSystem.Application.Features.Reservations.Queries.GetReservationById;
using CarRentalSystem.Application.Features.Reservations.Queries.GetReservationsByCustomer;
using CarRentalSystem.Application.Features.Reservations.Queries.SearchReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all reservations with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(GetAllReservationsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAllReservationsResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = new GetAllReservationsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status
        };
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get a reservation by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetReservationByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetReservationByIdResponse>> GetById(Guid id)
    {
        try
        {
            var query = new GetReservationByIdQuery(id);
            var response = await _mediator.Send(query);
            if (response == null)
                return NotFound(new { error = "Reservation not found" });
            
            //CHECK AUTHORIZATION - CUSTOMER CAN ONLY SEE THEIR OWN RESERVATIONS
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if(userRole=="Customer" &&
                Guid.TryParse(userIdClaim, out var userId) &&
                userId != response.CustomerId)
            {
                return Forbid();
            }

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Download reservation receipt PDF
    /// </summary>
    [HttpGet("{id:guid}/receipt")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        try
        {
            // 1. Verify existence and ownership
            var reservationQuery = new GetReservationByIdQuery(id);
            var reservation = await _mediator.Send(reservationQuery);

            if (reservation == null)
                return NotFound(new { error = "Reservation not found" });

            // Ensure customer can only access their own receipt
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole == "Customer" && 
                Guid.TryParse(userIdClaim, out var userId) && 
                userId != reservation.CustomerId)
            {
                return Forbid();
            }

            // 2. Generate Receipt
            var receiptQuery = new GenerateReservationReceiptQuery(id);
            var result = await _mediator.Send(receiptQuery);

            return File(result.PdfBytes, "application/pdf", result.FileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while generating the receipt." });
        }
    }

    /// <summary>
    /// Get reservations by customer ID
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(GetReservationsByCustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetReservationsByCustomerResponse>> GetByCustomer(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetReservationsByCustomerQuery
            {
                CustomerId = customerId,
                Page = page,
                PageSize = pageSize
            };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current customer's reservations
    /// </summary>
    [HttpGet("my-reservations")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(GetReservationsByCustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetReservationsByCustomerResponse>> GetMyReservations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if (!Guid.TryParse(userIdClaim, out var userId))
         {
             return Unauthorized();
         }

         var query = new GetReservationsByCustomerQuery
         {
             CustomerId = userId,
             Page = page,
             PageSize = pageSize
         };
         var response = await _mediator.Send(query);
         return Ok(response);
    }

    /// <summary>
    /// Create a new reservation BY CUSTOMER
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(CreateReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateReservationResponse>> CreateForCustomer([FromBody] CreateReservationCommand command)
    {
        try
        {
            //ENSURE CUSTOMER IS AUTHORIZED TO CREATE THEIR OWN RESERVATION
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userIdClaim) ||
                !Guid.TryParse(userIdClaim, out var userId) ||
                userId != command.CustomerId)
            {
                return Forbid();
            }

            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
    /// Create a new reservation BY ADMINISTRATOR OR EMPLOYEE
    /// </summary>
    [HttpPost("customer/{customerId:guid}")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(CreateReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateReservationResponse>> CreateForAdmin(Guid customerId,[FromBody] CreateReservationCommand command)
    {
        if(command.CustomerId != customerId)
            return BadRequest(new { error = "Customer ID does not match" });
        
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
    /// Confirm a pending reservation
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(ConfirmReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfirmReservationResponse>> Confirm(Guid id)
    {
        try
        {
            var command = new ConfirmReservationCommand(id);
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
    }

    /// <summary>
    /// Start a rental (confirmed reservation)
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(StartReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartReservationResponse>> Start(Guid id)
    {
        try
        {
            var command = new StartReservationCommand(id);
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
    }

    /// <summary>
    /// Complete a rental
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(CompleteReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompleteReservationResponse>> Complete(
        Guid id, 
        [FromBody] CompleteReservationCommand command)
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
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelReservationResponse>> Cancel(Guid id)
    {
        try
        {
            var command = new CancelReservationCommand(id);
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
    }

    /// <summary>
    /// Generate QR code for a reservation
    /// </summary>
    [HttpPost("{id}/generate-qrcode")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(GenerateReservationQRCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateReservationQRCodeResponse>> GenerateQRCode(Guid id)
    {
        try
        {
            var command = new GenerateReservationQRCodeCommand(id);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }



    /// <summary>
    /// Search reservations with filters and pagination
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(PagedResult<ReservationSearchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReservationSearchDto>>> Search([FromQuery] SearchReservationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
