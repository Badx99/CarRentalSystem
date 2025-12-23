using MediatR;

namespace CarRentalSystem.Application.Features.Customers.Queries.GetAllCustomers
{
    public record GetAllCustomersQuery : IRequest<GetAllCustomersResponse>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Search { get; init; }
    }

    public record CustomerDto
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string LicenseNumber { get; init; } = string.Empty;
        public string? Address { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string PhoneNumber { get; init; } = string.Empty;
        public DateTime RegisteredAt { get; init; }
        }

    
     public record GetAllCustomersResponse
     {
       public IEnumerable<CustomerDto> Customers { get; init; } = Enumerable.Empty<CustomerDto>();
       public int TotalCount { get; init; }
       public int Page { get; init; }
       public int PageSize { get; init; }
       public int TotalPages { get; init; }
     }
    
}