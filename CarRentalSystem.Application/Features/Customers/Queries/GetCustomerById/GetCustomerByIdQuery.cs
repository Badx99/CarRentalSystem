using MediatR;

namespace CarRentalSystem.Application.Features.Customers.Queries.GetCustomerById
{
    public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDetailsResponse?>;

    public record CustomerDetailsResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string LicenseNumber { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
        public int Age { get; init; }
    }
}
