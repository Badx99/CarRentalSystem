using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Employees.Queries.GetAllEmployees
{
    public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, GetAllEmployeesResponse>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<GetAllEmployeesResponse> Handle(
            GetAllEmployeesQuery request,
            CancellationToken cancellationToken)
        {
            var employees = await _employeeRepository.GetAllAsync(cancellationToken);
            var employeeList = employees.ToList();

            var totalCount = employeeList.Count;
            var skip = (request.Page - 1) * request.PageSize;
            var pagedEmployees = employeeList.Skip(skip).Take(request.PageSize);

            var employeeDtos = pagedEmployees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                EmployeeNumber = e.EmployeeNumber,
                Salary = e.Salary,
                HireDate = e.HireDate,
                UserType = e.UserType.ToString(),
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            });

            return new GetAllEmployeesResponse
            {
                Employees = employeeDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }
    }
}
