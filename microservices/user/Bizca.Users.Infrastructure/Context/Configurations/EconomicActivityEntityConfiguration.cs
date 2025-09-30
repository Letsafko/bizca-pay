using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class EconomicActivityEntityConfiguration : IEntityTypeConfiguration<EconomicActivity>
{
	public void Configure(EntityTypeBuilder<EconomicActivity> builder)
	{
		builder.HasKey(e => e.EconomicActivityId).HasName("pk_economicActivity");

		builder.Property(e => e.EconomicActivityId).ValueGeneratedNever();
		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
	}
}