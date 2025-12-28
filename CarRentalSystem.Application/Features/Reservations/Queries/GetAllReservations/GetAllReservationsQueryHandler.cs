using CarRentalSystem.Domain.Enums;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GetAllReservations
{
    public class GetAllReservationsQueryHandler : IRequestHandler<GetAllReservationsQuery, GetAllReservationsResponse>
    {
        private readonly IReservationRepository _reservationRepository;

        public GetAllReservationsQueryHandler(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<GetAllReservationsResponse> Handle(
            GetAllReservationsQuery request,
            CancellationToken cancellationToken)
        {
            var reservations = await _reservationRepository.GetAllWithDetailsAsync(cancellationToken);
            var reservationList = reservations.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ReservationStatus>(request.Status, out var status))
            {
                reservationList = reservationList.Where(r => r.Status == status).ToList();
            }

            var totalCount = reservationList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedReservations = reservationList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer?.FullName ?? string.Empty,
                    CustomerEmail = r.Customer?.Email ?? string.Empty,
                    VehicleId = r.VehicleId,
                    VehicleInfo = r.Vehicle != null 
                        ? $"{r.Vehicle.Brand} {r.Vehicle.Model} ({r.Vehicle.LicensePlate})" 
                        : string.Empty,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RentalDays = r.GetRentalDays(),
                    TotalAmount = r.TotalAmount,
                    Status = r.Status.ToString(),
                    PaymentStatus = r.IsFullyPaid() ? "Paid" : (r.Payments.Any() ? "Partial" : "Pending"),
                    CreatedAt = r.CreatedAt
                });

            return new GetAllReservationsResponse
            {
                Reservations = pagedReservations,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
