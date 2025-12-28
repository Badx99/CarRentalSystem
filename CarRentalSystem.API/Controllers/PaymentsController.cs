using CarRentalSystem.Application.Features.Payments.Commands.CompletePayment;
using CarRentalSystem.Application.Features.Payments.Commands.CreatePayment;
using CarRentalSystem.Application.Features.Payments.Commands.RefundPayment;
using CarRentalSystem.Application.Features.Payments.Queries.GetPaymentById;
using CarRentalSystem.Application.Features.Payments.Queries.GetPaymentsByReservation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IEnumerable<Application.Features.Payments.Queries.GetAllPayments.PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Application.Features.Payments.Queries.GetAllPayments.PaymentDto>>> GetAll()
    {
        var query = new Application.Features.Payments.Queries.GetAllPayments.GetAllPaymentsQuery();
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get a payment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPaymentByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPaymentByIdResponse>> GetById(Guid id)
    {
        try
        {
            var query = new GetPaymentByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all payments for a reservation
    /// </summary>
    [HttpGet("reservation/{reservationId:guid}")]
    [ProducesResponseType(typeof(GetPaymentsByReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetPaymentsByReservationResponse>> GetByReservation(Guid reservationId)
    {
        try
        {
            var query = new GetPaymentsByReservationQuery(reservationId);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Employee,Customer")]
    [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatePaymentResponse>> Create([FromBody] CreatePaymentCommand command)
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
    /// Mark a payment as completed
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,Employee")]
    [ProducesResponseType(typeof(CompletePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompletePaymentResponse>> Complete(Guid id)
    {
        try
        {
            var command = new CompletePaymentCommand(id);
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
    /// Refund a payment
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(RefundPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RefundPaymentResponse>> Refund(Guid id)
    {
        try
        {
            var command = new RefundPaymentCommand(id);
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
}
