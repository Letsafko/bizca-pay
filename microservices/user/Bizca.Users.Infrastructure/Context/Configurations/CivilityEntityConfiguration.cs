using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class CivilityEntityConfiguration : IEntityTypeConfiguration<Civility>
{
	public void Configure(EntityTypeBuilder<Civility> builder)
	{
		builder.HasKey(e => e.CivilityId).HasName("pk_civility");

		builder.Property(e => e.CivilityId).ValueGeneratedNever();
		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
	}
}