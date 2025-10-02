using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;
using Bizca.Users.Infrastructure.Context.Extensions;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class UserChannelEntityConfiguration : IEntityTypeConfiguration<UserChannel>
{
	public void Configure(EntityTypeBuilder<UserChannel> builder)
	{
		builder.ToTable("userChannel", "usr");
		builder.HasKey(static e => e.Id).HasName(Constants.PkUserChannel);
		builder
			.Property(static x => x.Id)
			.HasColumnName(Constants.UserChannelIdColumnName)
			.ValueGeneratedOnAdd()
			.HasConversion<IntValueObjectConverter<UserChannelId>>()
			.HasValueGenerator<IntValueObjectValueGenerator<UserChannelId>>();

		builder.Property(static e => e.ChannelValue)
				.HasConversion(static x => (string)x, static x => ChannelValue.Create(x).Value)
				.HasMaxLength(100)
				.HasColumnName("channelValue");

		builder.Property(static e => e.ChannelTypeId)
				.HasConversion(static x => (int)x, static x => (ChannelType)x)
				.HasColumnName("channelTypeId");

		builder.Property(static e => e.Confirmed).HasColumnName("confirmed");
		builder.AddAuditingProperties().IgnoreAuditingProperties();

		builder
			.HasMany(static x => x.UserChannelConfirmations)
			.WithOne()
			.HasForeignKey(Constants.UserChannelIdColumnName)
			.OnDelete(DeleteBehavior.ClientCascade)
			.IsRequired()
			.HasConstraintName(Constants.FkUserChannelConfirmation)
			.Metadata
			.PrincipalToDependent
			?.SetPropertyAccessMode(PropertyAccessMode.Field);

		builder.HasOne<ChannelTypeRef>()
				.WithMany()
				.IsRequired()
				.HasForeignKey(static e => e.ChannelTypeId)
				.HasConstraintName(Constants.FkUserChannelChannelTypeId);

		builder.HasIndex(static e => e.ChannelTypeId).HasDatabaseName(Constants.IxUserChannelChannelTypeId);
	}

	private static class Constants
	{
		internal const string PkUserChannel = "pk_userChannel";
		internal const string UserChannelIdColumnName = "userChannelId";
		internal const string FkUserChannelChannelTypeId = "fk_userChannel_channelTypeId";
		internal const string IxUserChannelChannelTypeId = "ix_userChannel_channelTypeId";
		internal const string FkUserChannelConfirmation = "fk_userChannel_userChannelConfirmation";
	}
}