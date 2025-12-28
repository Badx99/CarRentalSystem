using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Queries.GetEmployeeById
{
    public record GetEmployeeByIdQuery(Guid Id) : IRequest<GetEmployeeByIdResponse>;

    public record GetEmployeeByIdResponse
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
        public DateTime? UpdatedAt { get; init; }
    }
}
