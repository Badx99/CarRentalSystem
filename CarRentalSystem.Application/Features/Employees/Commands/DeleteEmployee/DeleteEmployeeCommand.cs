using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.DeleteEmployee
{
    public record DeleteEmployeeCommand(Guid Id) : IRequest<DeleteEmployeeResponse>;

    public record DeleteEmployeeResponse
    {
        public Guid Id { get; init; }
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
