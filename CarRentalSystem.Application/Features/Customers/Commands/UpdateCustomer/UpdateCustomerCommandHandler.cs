using CarRentalSystem.Domain.Interfaces;
using MediatR;

namespace CarRentalSystem.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null) return false;

            customer.Update(request.FirstName, request.LastName, customer.Email);
            customer.UpdateContactInfo(request.PhoneNumber, request.Address, request.LicenseNumber);

            await _customerRepository.UpdateAsync(customer, cancellationToken);
            // Repository handles SaveChangesAsync

            return true;
        }
    }
}
