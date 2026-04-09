using KMFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KMFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Email)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.HasData(
            new User
            {
                Id = Guid.Parse("cdcd0049-1a0f-411d-8d66-10c4575094c5"),
                Name = "Rizky Hidayat",
                Email = "rizky@tbp.com",
                PasswordHash = "AQAAAAIAAYagAAAAEKOSaGXdRi6ySUPbyT7gecP5r56gvIsG+7/xtbjMmebyFP2z4s/FSWIDvK0/NPNGjA==",
                DeptId = Guid.Parse("82aa21c0-d3f8-4821-805f-01f2a6886991"),
                RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActive = true,
            }
        );
    }
}
