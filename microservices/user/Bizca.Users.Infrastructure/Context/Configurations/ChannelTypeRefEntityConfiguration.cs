using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class ChannelTypeRefEntityConfiguration : IEntityTypeConfiguration<ChannelTypeRef>
{
	public void Configure(EntityTypeBuilder<ChannelTypeRef> builder)
	{
		builder.ToTable("channelType", "usr");
		builder.HasKey(static e => e.Id).HasName(Constants.PkChannelTypeRef);
		builder
			.Property(static x => x.Id)
			.HasConversion(static x => (int)x, static x => (ChannelType)x)
			.HasColumnName("channelTypeId");

		builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
		builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");
		builder.HasData
			(
				new ChannelTypeRef { Id = ChannelType.Sms, Label = "SMS", Description = "SMS" },
				new ChannelTypeRef { Id = ChannelType.Email, Label = "Email", Description = "Email" },
				new ChannelTypeRef { Id = ChannelType.Whatsapp, Label = "Whatsapp", Description = "Whatsapp" }
			);
	}

	private static class Constants
	{
		internal const string PkChannelTypeRef = "pk_channelType_ref";
	}
}
