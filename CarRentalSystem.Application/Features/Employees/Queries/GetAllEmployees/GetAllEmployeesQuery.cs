using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Queries.GetAllEmployees
{
    public record GetAllEmployeesQuery : IRequest<GetAllEmployeesResponse>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public record EmployeeDto
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public decimal Salary { get; init; }
        public DateTime HireDate { get; init; }
        public string UserType { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record GetAllEmployeesResponse
    {
        public IEnumerable<EmployeeDto> Employees { get; init; } = Enumerable.Empty<EmployeeDto>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
    }
}
