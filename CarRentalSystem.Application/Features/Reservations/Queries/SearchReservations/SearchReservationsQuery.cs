using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Reservations.Queries.SearchReservations
{
    public record SearchReservationsQuery : IRequest<PagedResult<ReservationSearchDto>>
    {
        public string? SearchTerm { get; init; } // Customer name or vehicle
        public ReservationStatus? Status { get; init; }
        public DateTime? StartDateFrom { get; init; }
        public DateTime? StartDateTo { get; init; }
        public decimal? MinAmount { get; init; }
        public decimal? MaxAmount { get; init; }
        public string? SortBy { get; init; } // StartDate, TotalAmount, CreatedAt
        public bool SortDescending { get; init; } = true;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public record ReservationSearchDto
    {
        public Guid Id { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string VehicleInfo { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal TotalAmount { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
