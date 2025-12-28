using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, UpdateEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public UpdateEmployeeCommandHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<UpdateEmployeeResponse> Handle(
            UpdateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID '{request.Id}' not found.");
            }

            // Check if email is being changed and if it's already taken
            if (employee.Email.ToLower() != request.Email.ToLower())
            {
                if (await _employeeRepository.EmailExistsAsync(request.Email, cancellationToken))
                {
                    throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
                }
            }

            // Check if employee number is being changed and if it's already taken
            if (employee.EmployeeNumber != request.EmployeeNumber)
            {
                if (await _employeeRepository.EmployeeNumberExistsAsync(request.EmployeeNumber, cancellationToken))
                {
                    throw new InvalidOperationException($"Employee number '{request.EmployeeNumber}' is already in use.");
                }
            }

            // Update employee through domain method
            employee.Update(request.FirstName, request.LastName, request.Email);
            employee.UpdateEmployeeInfo(request.EmployeeNumber, request.Salary);
            
            if (request.IsActive != employee.IsActive)
            {
                if (request.IsActive)
                    employee.Activate();
                else
                    employee.Deactivate();
            }

            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            return new UpdateEmployeeResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                EmployeeNumber = employee.EmployeeNumber,
                Salary = employee.Salary,
                IsActive = employee.IsActive,
                UpdatedAt = employee.UpdatedAt
            };
        }
    }
}
