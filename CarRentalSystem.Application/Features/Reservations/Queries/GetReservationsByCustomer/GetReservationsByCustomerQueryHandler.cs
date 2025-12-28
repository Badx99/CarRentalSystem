using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Reservations.Queries.GetReservationsByCustomer
{
    public class GetReservationsByCustomerQueryHandler : IRequestHandler<GetReservationsByCustomerQuery, GetReservationsByCustomerResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ICustomerRepository _customerRepository;

        public GetReservationsByCustomerQueryHandler(
            IReservationRepository reservationRepository,
            ICustomerRepository customerRepository)
        {
            _reservationRepository = reservationRepository;
            _customerRepository = customerRepository;
        }

        public async Task<GetReservationsByCustomerResponse> Handle(
            GetReservationsByCustomerQuery request,
            CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found.");
            }

            var reservations = await _reservationRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
            var reservationList = reservations.ToList();

            var totalCount = reservationList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedReservations = reservationList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new CustomerReservationDto
                {
                    Id = r.Id,
                    VehicleId = r.VehicleId,
                    VehicleBrand = r.Vehicle?.Brand ?? string.Empty,
                    VehicleModel = r.Vehicle?.Model ?? string.Empty,
                    VehicleLicensePlate = r.Vehicle?.LicensePlate ?? string.Empty,
                    VehicleTypeName = r.Vehicle?.VehicleType?.Name ?? string.Empty,
                    VehicleImageUrl = r.Vehicle?.ImageUrl,
                    VehicleInfo = r.Vehicle != null 
                        ? $"{r.Vehicle.Brand} {r.Vehicle.Model} ({r.Vehicle.LicensePlate})" 
                        : string.Empty,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RentalDays = r.GetRentalDays(),
                    TotalAmount = r.TotalAmount,
                    Status = r.Status.ToString(),
                    QRCode = r.QRCode,
                    CreatedAt = r.CreatedAt
                });

            return new GetReservationsByCustomerResponse
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                Reservations = pagedReservations,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
    }
}
