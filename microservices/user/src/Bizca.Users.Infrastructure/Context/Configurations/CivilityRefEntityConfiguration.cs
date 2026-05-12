using System;
using System.Linq;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class CivilityRefEntityConfiguration : IEntityTypeConfiguration<CivilityRef>
{
	public void Configure(EntityTypeBuilder<CivilityRef> builder)
	{
		builder.ToTable("civility", DatabaseConstants.Schema);
		builder.HasKey(static e => e.Id).HasName(Constants.PkCivilityRef);
		builder
			.Property(static x => x.Id)
			.HasConversion(static x => (int)x, static x => (Civility)x)
			.HasColumnName("civilityId");

		builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
		builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");

		var enums = Enum.GetValues<Civility>()
			.Select(e => new CivilityRef { Id = e, Label = e.ToString(), Description = e.ToString() })
			.ToArray();

		builder.HasData(enums);
	}

	private static class Constants
	{
		internal const string PkCivilityRef = "pk_civility_ref";
	}
}
