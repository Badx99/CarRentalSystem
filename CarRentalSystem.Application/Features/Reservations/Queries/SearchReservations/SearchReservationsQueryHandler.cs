using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Reservations.Queries.SearchReservations
{
    public class SearchReservationsQueryHandler
    : IRequestHandler<SearchReservationsQuery, PagedResult<ReservationSearchDto>>
    {
        private readonly IReservationRepository _reservationRepository;

        public SearchReservationsQueryHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<PagedResult<ReservationSearchDto>> Handle(
            SearchReservationsQuery request,
            CancellationToken cancellationToken)
        {
            var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
            var query = reservations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Customer.FullName.ToLower().Contains(searchLower) ||
                    r.Vehicle.Brand.ToLower().Contains(searchLower) ||
                    r.Vehicle.Model.ToLower().Contains(searchLower) ||
                    r.Vehicle.LicensePlate.ToLower().Contains(searchLower));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            if (request.StartDateFrom.HasValue)
            {
                query = query.Where(r => r.StartDate >= request.StartDateFrom.Value);
            }

            if (request.StartDateTo.HasValue)
            {
                query = query.Where(r => r.StartDate <= request.StartDateTo.Value);
            }

            if (request.MinAmount.HasValue)
            {
                query = query.Where(r => r.TotalAmount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(r => r.TotalAmount <= request.MaxAmount.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "startdate" => request.SortDescending
                    ? query.OrderByDescending(r => r.StartDate)
                    : query.OrderBy(r => r.StartDate),
                "totalamount" => request.SortDescending
                    ? query.OrderByDescending(r => r.TotalAmount)
                    : query.OrderBy(r => r.TotalAmount),
                "createdat" => request.SortDescending
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            // Get total count
            var totalCount = query.Count();

            // Apply pagination
            var items = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new ReservationSearchDto
                {
                    Id = r.Id,
                    CustomerName = r.Customer.FullName,
                    VehicleInfo = $"{r.Vehicle.Brand} {r.Vehicle.Model} ({r.Vehicle.LicensePlate})",
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return new PagedResult<ReservationSearchDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
