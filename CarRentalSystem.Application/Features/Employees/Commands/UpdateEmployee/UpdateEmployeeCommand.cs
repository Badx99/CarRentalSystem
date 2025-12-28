using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.UpdateEmployee
{
    public record UpdateEmployeeCommand : IRequest<UpdateEmployeeResponse>
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public decimal Salary { get; init; }
        public bool IsActive { get; init; }
    }

    public record UpdateEmployeeResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public decimal Salary { get; init; }
        public bool IsActive { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
