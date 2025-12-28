using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Vehicles.Queries.SearchVehicles
{
    public class SearchVehiclesQueryHandler
    : IRequestHandler<SearchVehiclesQuery, PagedResult<VehicleSearchDto>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public SearchVehiclesQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<PagedResult<VehicleSearchDto>> Handle(
            SearchVehiclesQuery request,
            CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleRepository.GetAllWithTypeAsync(cancellationToken);
            var query = vehicles.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(v =>
                    v.Brand.ToLower().Contains(searchLower) ||
                    v.Model.ToLower().Contains(searchLower) ||
                    v.LicensePlate.ToLower().Contains(searchLower));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(v => v.Status == request.Status.Value);
            }

            if (request.VehicleTypeId.HasValue)
            {
                query = query.Where(v => v.VehicleTypeId == request.VehicleTypeId.Value);
            }

            if (request.MinYear.HasValue)
            {
                query = query.Where(v => v.Year >= request.MinYear.Value);
            }

            if (request.MaxYear.HasValue)
            {
                query = query.Where(v => v.Year <= request.MaxYear.Value);
            }

            if (request.MinDailyRate.HasValue)
            {
                query = query.Where(v => v.GetEffectiveDailyRate() >= request.MinDailyRate.Value);
            }

            if (request.MaxDailyRate.HasValue)
            {
                query = query.Where(v => v.GetEffectiveDailyRate() <= request.MaxDailyRate.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "brand" => request.SortDescending
                    ? query.OrderByDescending(v => v.Brand)
                    : query.OrderBy(v => v.Brand),
                "model" => request.SortDescending
                    ? query.OrderByDescending(v => v.Model)
                    : query.OrderBy(v => v.Model),
                "year" => request.SortDescending
                    ? query.OrderByDescending(v => v.Year)
                    : query.OrderBy(v => v.Year),
                "dailyrate" => request.SortDescending
                    ? query.OrderByDescending(v => v.GetEffectiveDailyRate())
                    : query.OrderBy(v => v.GetEffectiveDailyRate()),
                _ => query.OrderBy(v => v.Brand)
            };

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VehicleSearchDto
            {
                Id = v.Id,
                Brand = v.Brand ?? "",
                Model = v.Model ?? "",
                Year = v.Year,
                LicensePlate = v.LicensePlate ?? "",
                Color = v.Color ?? "",
                Status = v.Status.ToString() ?? "Unknown",
                VehicleTypeName = v.VehicleType != null ? v.VehicleType.Name ?? "Unknown" : "Unknown",
                DailyRate = v.GetEffectiveDailyRate(),
                PassengerCapacity = v.VehicleType != null ? v.VehicleType.PassengerCapacity : 0,
                Mileage = v.Mileage,
                ImageUrl = v.ImageUrl ?? ""
            })
            .ToList();


            return new PagedResult<VehicleSearchDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
