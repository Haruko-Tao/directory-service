using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasConversion(name => name.Value, value => Name.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Slug)
            .HasColumnName("slug")
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value!)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => new { d.Slug }).IsUnique();

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasConversion(path => path.Value, value => Path.Create(value, null).Value!)
            .IsRequired();

        builder.HasIndex(d => new { d.Path }).IsUnique();

        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}