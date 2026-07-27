using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasConversion(name => name.Value, value => Name.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(200);

        builder.ComplexProperty(l => l.Address,
            address =>
            {
                address.Property(a => a.City).HasColumnName("city").IsRequired().HasMaxLength(100);
                address.Property(a => a.Street).HasColumnName("street").IsRequired().HasMaxLength(200);
                address.Property(a => a.House).HasColumnName("house").IsRequired().HasMaxLength(20);
                address.Property(a => a.Apartment).HasColumnName("apartment").HasMaxLength(20);
            });
        
            

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}