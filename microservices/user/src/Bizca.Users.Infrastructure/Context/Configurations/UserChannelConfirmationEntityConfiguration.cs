using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.ValueObjects;
using Bizca.Users.Infrastructure.Context.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class UserChannelConfirmationEntityConfiguration : IEntityTypeConfiguration<UserChannelConfirmation>
{
	public void Configure(EntityTypeBuilder<UserChannelConfirmation> builder)
	{
		builder.ToTable("userChannelConfirmation", DatabaseConstants.Schema);
		builder.HasKey(static e => e.Id).HasName(Constants.PkChannelConfirmation);
		builder
			.Property(static x => x.Id)
			.ValueGeneratedOnAdd()
			.HasValueGenerator<IntValueObjectValueGenerator<UserChannelConfirmationId>>()
			.ToIntValueObjectConverter(Constants.ChannelConfirmationIdColumnName);

		builder.Property(static e => e.ConfirmationCode).HasMaxLength(50).HasColumnName("confirmationCode");
		builder.Property(static e => e.ExpirationDatetime).HasColumnName("expirationDate");
		builder
			.AddAuditingProperties()
			.IgnoreAuditingProperties(static b => b.Ignore(static e => e.LastModifiedDatetime));
	}

	private static class Constants
	{
		internal const string ChannelConfirmationIdColumnName = "userChannelConfirmationId";
		internal const string PkChannelConfirmation = "pk_channelConfirmation";
	}
}
