using CarRentalSystem.Domain.Entities;

namespace CarRentalSystem.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken = default);
        Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);
        Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
