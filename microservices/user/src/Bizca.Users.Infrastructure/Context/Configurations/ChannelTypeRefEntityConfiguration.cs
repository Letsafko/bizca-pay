using System;
using System.Linq;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class ChannelTypeRefEntityConfiguration : IEntityTypeConfiguration<ChannelTypeRef>
{
	public void Configure(EntityTypeBuilder<ChannelTypeRef> builder)
	{
		builder.ToTable("channelType", DatabaseConstants.Schema);
		builder.HasKey(static e => e.Id).HasName(Constants.PkChannelTypeRef);
		builder
			.Property(static x => x.Id)
			.HasConversion(static x => (int)x, static x => (ChannelType)x)
			.HasColumnName("channelTypeId");

		builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
		builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");

		var enums = Enum.GetValues<ChannelType>()
			.Select(e => new ChannelTypeRef { Id = e, Label = e.ToString(), Description = e.ToString() })
			.ToArray();
		builder.HasData(enums);
	}

	private static class Constants
	{
		internal const string PkChannelTypeRef = "pk_channelType_ref";
	}
}
