using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class UserChannelConfirmationConfiguration : IEntityTypeConfiguration<UserChannelConfirmation>
{
	public void Configure(EntityTypeBuilder<UserChannelConfirmation> builder)
	{
		builder.HasKey(e => new
		{
			e.UserId,
			e.ChannelId,
			e.CreationDate
		}).HasName("pk_userChannelConfirmation");

		builder.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");

		builder.HasOne(d => d.Channel).WithMany(p => p.UserChannelConfirmations).OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_userChannelConfirmation_channelId");

		builder.HasOne(d => d.User).WithMany(p => p.UserChannelConfirmations).OnDelete(DeleteBehavior.ClientSetNull)
				.HasConstraintName("fk_userChannelConfirmation_userId");
	}
}