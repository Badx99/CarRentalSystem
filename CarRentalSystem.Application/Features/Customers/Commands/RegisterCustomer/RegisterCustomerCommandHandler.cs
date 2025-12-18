using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Domain.Entities;
using CarRentalSystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalSystem.Application.Features.Customers.Commands.RegisterCustomer
{
    public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand,RegisterCustomerResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        public readonly IEmailService _emailService;

        public RegisterCustomerCommandHandler(IUserRepository userRepository,IPasswordHasher passwordHasher,IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher=passwordHasher;
            _emailService = emailService;
        }

        public async Task<RegisterCustomerResponse> Handle(
            RegisterCustomerCommand request,
            CancellationToken cancellationToken)
        {
            // Step 1: Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(
                request.Email,
                cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    $"A user with email '{request.Email}' already exists.");
            }

            // Step 2: Hash the password
            var passwordHash = _passwordHasher.Hash(request.Password);

            // Step 3: Create the customer entity
            var customer = Customer.Create(
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                passwordHash: passwordHash,
                phoneNumber: request.PhoneNumber,
                address: request.Address,
                licenseNumber: request.LicenseNumber,
                dateOfBirth: request.DateOfBirth
            );

            // Step 4: Save to database
            await _userRepository.AddAsync(customer, cancellationToken);

            // SEND WELCOME EMAIL
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(
                        customer.Email,
                        customer.FullName,
                        cancellationToken);
                }
                catch
                {
                    // Ignore email errors
                }
            }, cancellationToken);

            // Step 5: Return the response
            return new RegisterCustomerResponse
            {
                Id = customer.Id,
                Email = customer.Email,
                FullName = customer.FullName,
                CreatedAt = customer.CreatedAt
            };
        }
    }
}
