using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class ChannelEntityConfiguration : IEntityTypeConfiguration<Channel>
{
	public void Configure(EntityTypeBuilder<Channel> builder)
	{
		builder.HasKey(e => e.ChannelId).HasName("pk_channel");
		builder.Property(e => e.ChannelId).ValueGeneratedNever();
		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
	}
}