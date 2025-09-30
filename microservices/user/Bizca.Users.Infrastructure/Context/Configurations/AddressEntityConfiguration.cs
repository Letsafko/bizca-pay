using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

public class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
	public void Configure(EntityTypeBuilder<Address> builder)
	{
		builder.HasKey(entity => entity.AddressId).HasName(Constants.PkAddress);

		builder.HasIndex(entity => new
		{
			entity.UserId,
			entity.Active
		}).HasDatabaseName(Constants.IxUserActive).IsUnique().HasFilter(Constants.FilterActiveTrue);

		builder.Property(entity => entity.CreationDate).HasDefaultValueSql(Constants.DefaultDateSql);
		builder.Property(entity => entity.LastUpdate).HasDefaultValueSql(Constants.DefaultDateSql);

		builder.HasOne(entity => entity.Country).WithMany(nav => nav.Addresses).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName(Constants.FkCountry);

		builder.HasOne(entity => entity.User).WithMany(nav => nav.Addresses).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName(Constants.FkUser);
	}

	private static class Constants
	{
		public const string FkUser = "fk_address_userId";
		public const string FkCountry = "fk_address_countryId";
		public const string PkAddress = "pk_address";
		public const string IxUserActive = "ix_address_userId_active";
		public const string DefaultDateSql = "(getdate())";
		public const string FilterActiveTrue = "[active]=(1)";
	}
}