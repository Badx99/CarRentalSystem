using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.DeleteEmployee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, DeleteEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<DeleteEmployeeResponse> Handle(
            DeleteEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID '{request.Id}' not found.");
            }

            await _employeeRepository.DeleteAsync(request.Id, cancellationToken);

            return new DeleteEmployeeResponse
            {
                Id = request.Id,
                Success = true,
                Message = $"Employee '{employee.FirstName} {employee.LastName}' has been deleted."
            };
        }
    }
}
