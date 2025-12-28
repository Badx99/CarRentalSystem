using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.CreateEmployee
{
    public record CreateEmployeeCommand : IRequest<CreateEmployeeResponse>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public decimal Salary { get; init; }
        public DateTime? HireDate { get; init; }
    }

    public record CreateEmployeeResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public decimal Salary { get; init; }
        public DateTime HireDate { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
