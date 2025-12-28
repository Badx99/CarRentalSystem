using CarRentalSystem.Application.Common.Interfaces;
using CarRentalSystem.Application.Common.Models;
using CarRentalSystem.Domain.Interfaces;
using CarRentalSystem.Infrastructure.Data;
using CarRentalSystem.Infrastructure.Repositories;
using CarRentalSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace CarRentalSystem.Infrastructure
{
    public static class DependencyInjection
    {

        //
        //EXTENSION METHOD
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) //'CONFIGURATION' GIVES ACCESS TO APP CONFIGURATIONS(APPSETTINGS.JSON)
        {

            //REGISTERS CARRENTALDBCONTEXT WITH THE DEPENDENCY INJECTION CONTAINER (DI) AND CONFIGURE IT WITH THE OPTIONS GIVEN NEXT
            services.AddDbContext<CarRentalDbContext>(options => 
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(CarRentalDbContext).Assembly.FullName))); // TELLS EF CORE WHICH PROJECT/ASSEMBLY CONTAINS THE MIGRATION FILES.

            // Repositories will be registered here later
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            
            // Register other infrastructure services here
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IQRCodeService, QRCodeService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IExportService, ExportService>();

            // Configure Email Settings
            services.Configure<EmailSettings>(
                configuration.GetSection(EmailSettings.SectionName));

            //JWT SETTINGS CONFIGURATION
            services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

            // Configure JWT Authentication
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set");

            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings configuration is missing");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                Console.WriteLine("DIDIDIDIDIDIDI=======================================================||||||||||||||||||||||||||||||+========================++++++++++++++++++=========================");
                Console.WriteLine($"SECRET USED TO VALIDATE === {jwtSecret}");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };
            });

            Console.WriteLine("DIDIDIDIDIDIDI=======================================================||||||||||||||||||||||||||||||+========================++++++++++++++++++=========================");
            Console.WriteLine($"SECRET USED TO VALIDATE === {jwtSecret}");
            return services;
        }
    }
}
