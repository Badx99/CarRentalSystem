using CarRentalSystem.Domain.Entities;
using CarRentalSystem.Domain.Interfaces;
using CarRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly CarRentalDbContext _context;

        public EmployeeRepository(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber, cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .AnyAsync(e => e.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .AnyAsync(e => e.EmployeeNumber == employeeNumber, cancellationToken);
        }

        public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            await _context.Set<Employee>().AddAsync(employee, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return employee;
        }

        public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            _context.Set<Employee>().Update(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await GetByIdAsync(id, cancellationToken);
            if (employee != null)
            {
                _context.Set<Employee>().Remove(employee);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
