using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SortSchedule.Domain.Entities;

namespace SortSchedule.Infrastructure.Persistence.Configurations;

public static class DefaultRoleIds
{
    public static readonly Guid Admin = Guid.Parse("6A8EBEA7-0D17-4178-AD20-9EA0F0C2F771");
    public static readonly Guid Lecturer = Guid.Parse("4F7D6A97-B228-4A62-A308-91530C223A84");
    public static readonly Guid Student = Guid.Parse("2F5B4F8E-7659-4955-BAD6-B526639C3CDB");
}

public sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("AppRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(
            new AppRole { Id = DefaultRoleIds.Admin, Name = "Admin" },
            new AppRole { Id = DefaultRoleIds.Lecturer, Name = "Lecturer" },
            new AppRole { Id = DefaultRoleIds.Student, Name = "Student" });
    }
}
