using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class PartnerEntityConfiguration : IEntityTypeConfiguration<Partner>
{
	public void Configure(EntityTypeBuilder<Partner> builder)
	{
		builder.HasKey(e => e.PartnerId).HasName("pk_partner");

		builder.Property(e => e.PartnerId).ValueGeneratedNever();
		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
	}
}