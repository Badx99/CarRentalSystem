using CarRentalSystem.Domain.Enums;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Vehicles.Queries.GetAllVehicles
{
    public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, GetAllVehiclesResponse>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public GetAllVehiclesQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<GetAllVehiclesResponse> Handle(
            GetAllVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleRepository.GetAllWithTypeAsync(cancellationToken);
            var vehicleList = vehicles.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<VehicleStatus>(request.Status, out var status))
            {
                vehicleList = vehicleList.Where(v => v.Status == status).ToList();
            }

            if (request.VehicleTypeId.HasValue)
            {
                vehicleList = vehicleList.Where(v => v.VehicleTypeId == request.VehicleTypeId.Value).ToList();
            }

            var totalCount = vehicleList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedVehicles = vehicleList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    LicensePlate = v.LicensePlate,
                    Color = v.Color,
                    Mileage = v.Mileage,
                    Status = v.Status.ToString(),
                    DailyRate = v.DailyRate,
                    EffectiveDailyRate = v.GetEffectiveDailyRate(),
                    ImageUrl = v.ImageUrl,
                    VehicleTypeId = v.VehicleTypeId,
                    VehicleTypeName = v.VehicleType?.Name ?? string.Empty,
                    CreatedAt = v.CreatedAt
                });

            return new GetAllVehiclesResponse
            {
                Vehicles = pagedVehicles,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
