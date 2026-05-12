using System;
using System.Linq;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class StatusRefEntityConfiguration : IEntityTypeConfiguration<StatusRef>
{
	public void Configure(EntityTypeBuilder<StatusRef> builder)
	{
		builder.ToTable("status", DatabaseConstants.Schema);
		builder.HasKey(static e => e.Id).HasName(Constants.PkStatusRef);
		builder
			.Property(static x => x.Id)
			.HasConversion(static x => (int)x, static x => (Status)x)
			.HasColumnName("statusId");

		builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
		builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");

		var enums = Enum.GetValues<Status>()
			.Select(e => new StatusRef { Id = e, Label = e.ToString(), Description = e.ToString() })
			.ToArray();

		builder.HasData(enums);
	}

	private static class Constants
	{
		internal const string PkStatusRef = "pk_status_ref";
	}
}
