using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class UserChannelEntityConfiguration : IEntityTypeConfiguration<UserChannel>
{
	public void Configure(EntityTypeBuilder<UserChannel> builder)
	{
		builder.HasKey(e => new
		{
			e.UserId,
			e.ChannelMask
		}).HasName("pk_userChannel");

		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.LastUpdate).HasDefaultValueSql("(getdate())");
		builder.Property(e => e.PartnerId).HasDefaultValue((short)1);

		builder.HasOne(d => d.Partner).WithMany(p => p.UserChannels).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_userChannel_partnerId");

		builder.HasOne(d => d.User).WithMany(p => p.UserChannels).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_userChannel_userId");
	}
}