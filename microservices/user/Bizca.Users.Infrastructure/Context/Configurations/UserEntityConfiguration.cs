using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<Entities.User>
{
	public void Configure(EntityTypeBuilder<Entities.User> builder)
	{
		builder.HasKey(e => e.UserId).HasName("pk_user");

		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.RowVersion).IsRowVersion().IsConcurrencyToken();

		builder.HasOne(d => d.BirthCountry).WithMany(p => p.Users).HasConstraintName("fk_user_birthCountryId");

		builder.HasOne(d => d.Civility).WithMany(p => p.Users).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_user_civilityId");

		builder.HasOne(d => d.EconomicActivity).WithMany(p => p.Users).HasConstraintName("fk_user_economicActivityId");

		builder.HasOne(d => d.Partner).WithMany(p => p.Users).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_user_partnerId");
	}
}