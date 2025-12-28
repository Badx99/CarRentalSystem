using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Entities;
using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher)
        {
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<CreateEmployeeResponse> Handle(
            CreateEmployeeCommand request,
            CancellationToken cancellationToken)
        {
            // Check if email already exists
            if (await _employeeRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");
            }

            // Check if employee number already exists
            if (await _employeeRepository.EmployeeNumberExistsAsync(request.EmployeeNumber, cancellationToken))
            {
                throw new InvalidOperationException($"An employee with number '{request.EmployeeNumber}' already exists.");
            }

            var passwordHash = _passwordHasher.Hash(request.Password);

            var employee = Employee.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.EmployeeNumber,
                request.Salary,
                request.HireDate);

            await _employeeRepository.AddAsync(employee, cancellationToken);

            return new CreateEmployeeResponse
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                EmployeeNumber = employee.EmployeeNumber,
                Salary = employee.Salary,
                HireDate = employee.HireDate,
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
