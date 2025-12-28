using CarRentalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalSystem.Infrastructure.Data.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("Reservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(r => r.EndDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(r => r.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Notes)
                .HasMaxLength(1500);

            builder.Property(r => r.QRCode)
                .HasColumnType("text");

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired(false);

            // Foreign key relationship with Customer
            builder.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign key relationship with Vehicle
            builder.HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for common queries
            builder.HasIndex(r => r.CustomerId)
                .HasDatabaseName("IX_Reservations_CustomerId");

            builder.HasIndex(r => r.VehicleId)
                .HasDatabaseName("IX_Reservations_VehicleId");

            builder.HasIndex(r => r.Status)
                .HasDatabaseName("IX_Reservations_Status");

            builder.HasIndex(r => new { r.VehicleId, r.StartDate, r.EndDate })
                .HasDatabaseName("IX_Reservations_VehicleId_DateRange");
        }
    }
}
