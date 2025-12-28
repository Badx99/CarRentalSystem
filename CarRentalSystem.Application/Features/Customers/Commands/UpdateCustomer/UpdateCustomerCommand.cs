using MediatR;

namespace CarRentalSystem.Application.Features.Customers.Commands.UpdateCustomer
{
    public record UpdateCustomerCommand : IRequest<bool>
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string LicenseNumber { get; init; } = string.Empty;
    }
}
