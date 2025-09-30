using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public sealed class PasswordEntityConfiguration : IEntityTypeConfiguration<Password>
{
	void IEntityTypeConfiguration<Password>.Configure(EntityTypeBuilder<Password> builder)
	{
		builder.HasKey(e => e.PasswordId).HasName("pk_password");

		builder.HasIndex(e => new
		{
			e.UserId,
			e.Active
		}).HasDatabaseName("ix_password_userId_active").IsUnique().HasFilter("[active]=(1)");

		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");

		builder.HasOne(d => d.User).WithMany(p => p.Passwords).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_password_userId");
	}
}