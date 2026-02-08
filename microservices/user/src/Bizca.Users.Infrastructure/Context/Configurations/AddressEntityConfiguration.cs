using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.ValueObjects;
using Bizca.Users.Infrastructure.Context.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
	public void Configure(EntityTypeBuilder<Address> builder)
	{
		builder.ToTable("address", "usr");
		builder.HasKey(static entity => entity.Id).HasName(Constants.PkAddress);
		builder
				.Property(static x => x.Id)
				.ValueGeneratedOnAdd()
				.HasValueGenerator<IntValueObjectValueGenerator<AddressId>>()
				.ToIntValueObjectConverter("addressId");

		builder.Property(static e => e.Country).IsRequired().HasMaxLength(100).HasColumnName("country");
		builder.Property(static e => e.City).IsRequired().HasMaxLength(100).HasColumnName("city");
		builder.Property(static e => e.Zipcode).HasMaxLength(10).HasColumnName("zipcode");
		builder.Property(static e => e.Street).HasMaxLength(255).HasColumnName("street");
		builder.AddAuditingProperties().IgnoreAuditingProperties();
		builder.Property(static e => e.CountryCode)
			   .HasMaxLength(2)
			   .HasConversion(static x => (string?)x, static x => CountryCode.TryCreate(x))
			   .HasColumnName("countryCode");
	}

	private static class Constants
	{
		internal const string PkAddress = "pk_address";
	}
}
