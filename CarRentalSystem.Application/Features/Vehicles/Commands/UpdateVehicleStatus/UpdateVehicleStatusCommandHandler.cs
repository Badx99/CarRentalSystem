using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Vehicles.Commands.UpdateVehicleStatus
{
    public class UpdateVehicleStatusCommandHandler : IRequestHandler<UpdateVehicleStatusCommand, UpdateVehicleStatusResponse>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public UpdateVehicleStatusCommandHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<UpdateVehicleStatusResponse> Handle(
            UpdateVehicleStatusCommand request,
            CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID '{request.Id}' not found.");
            }

            if (!Enum.TryParse<CarRentalSystem.Domain.Enums.VehicleStatus>(request.Status, out var newStatus))
            {
                throw new ArgumentException($"Invalid status: '{request.Status}'");
            }

            var previousStatus = vehicle.Status.ToString();
            vehicle.SetStatus(newStatus);
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

            return new UpdateVehicleStatusResponse
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                PreviousStatus = previousStatus,
                NewStatus = vehicle.Status.ToString(),
                UpdatedAt = vehicle.UpdatedAt
            };
        }
    }
}
