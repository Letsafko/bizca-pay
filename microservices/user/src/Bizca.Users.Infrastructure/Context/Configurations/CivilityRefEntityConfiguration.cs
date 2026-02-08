using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class CivilityRefEntityConfiguration : IEntityTypeConfiguration<CivilityRef>
{
	public void Configure(EntityTypeBuilder<CivilityRef> builder)
	{
		builder.ToTable("civility", "usr");
		builder.HasKey(static e => e.Id).HasName(Constants.PkCivilityRef);
		builder
			.Property(static x => x.Id)
			.HasConversion(static x => (int)x, static x => (Civility)x)
			.HasColumnName("civilityId");

		builder.Property(static e => e.Label).HasMaxLength(50).HasColumnName("label");
		builder.Property(static e => e.Description).HasMaxLength(50).HasColumnName("description");
		builder.HasData
			(
				new CivilityRef { Id = Civility.Mr, Label = "Mr", Description = "Mr" },
				new CivilityRef { Id = Civility.Ms, Label = "Ms", Description = "Ms" },
				new CivilityRef { Id = Civility.Other, Label = "Other", Description = "Other" }
			);
	}

	private static class Constants
	{
		internal const string PkCivilityRef = "pk_civility_ref";
	}
}
