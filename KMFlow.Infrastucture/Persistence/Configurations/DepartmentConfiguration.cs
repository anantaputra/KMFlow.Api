using KMFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KMFlow.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeptName)
            .IsRequired();

        builder.Property(x => x.DeptSlug)
            .IsRequired();

        builder.HasData(
            new Department
            {
                Id = Guid.Parse("82aa21c0-d3f8-4821-805f-01f2a6886991"),
                DeptName = "Information Technology",
                DeptSlug = "it",
            }
        );
    }
}
