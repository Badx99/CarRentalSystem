using MediatR;

namespace CarRentalSystem.Application.Features.Vehicles.Commands.UpdateVehicleStatus
{
    public record UpdateVehicleStatusCommand : IRequest<UpdateVehicleStatusResponse>
    {
        public Guid Id { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public record UpdateVehicleStatusResponse
    {
        public Guid Id { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime? UpdatedAt { get; init; }
    }
}
