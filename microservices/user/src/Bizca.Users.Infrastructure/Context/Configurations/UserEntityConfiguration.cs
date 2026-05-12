using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;
using Bizca.Users.Infrastructure.Context.Extensions;
using Bizca.Users.Infrastructure.Context.ReferentialData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Configurations;

internal sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("user", DatabaseConstants.Schema);
		builder.HasKey(static entity => entity.Id).HasName(Constants.PkUser);
		builder
			.Property(static x => x.Id)
			.ValueGeneratedOnAdd()
			.HasValueGenerator<IntValueObjectValueGenerator<UserId>>()
			.ToIntValueObjectConverter(Constants.UserIdColumnName);

		builder.Property(static e => e.ExternalUserId)
				.HasConversion(static x => x.Value, static x => ExternalUserId.Create(x).Value)
				.HasMaxLength(40).HasColumnName("externalUserId");

		builder.Property(static e => e.Civility)
				.HasConversion(static x => (int)x, static x => (Civility)x)
				.HasColumnName("civilityId");

		builder.Property(static e => e.Status)
				.HasConversion(static x => (int)x, static x => (Status)x)
				.HasColumnName("statusId");

		builder.Property(static e => e.Active).HasColumnName("active");

		builder.Property(static e => e.FirstName).HasMaxLength(100).HasColumnName("firstName");
		builder.Property(static e => e.LastName).HasMaxLength(100).HasColumnName("lastName");
		builder.Property(static e => e.BirthDate).HasColumnName("birthDate");
		builder.Property(static e => e.BirthCity).HasMaxLength(100).HasColumnName("birthCity");
		builder.Property(static e => e.SecurityStamp).HasMaxLength(256).HasColumnName("securityStamp");
		builder.Property(static e => e.PasswordHash).HasMaxLength(256).HasColumnName("passwordHash");
		builder.Property(static e => e.BirthCountry).HasMaxLength(100).HasColumnName("birthCountry");
		builder
			.AddVersionAsShadowProperty()
			.AddAuditingProperties()
			.IgnoreAuditingProperties();

		builder.Property(static e => e.BirthCountryCode)
				.HasMaxLength(2)
				.HasConversion(static x => (string?)x, static x => CountryCode.TryCreate(x))
				.HasColumnName("birthCountryCode");

		builder
			.HasMany(static x => x.UserChannels)
			.WithOne()
			.HasForeignKey(Constants.UserIdColumnName)
			.IsRequired()
			.OnDelete(DeleteBehavior.ClientCascade)
			.HasConstraintName(Constants.FkUserChannel)
			.Metadata
			.PrincipalToDependent
			?.SetPropertyAccessMode(PropertyAccessMode.Field);

		builder
			.HasOne(static x => x.Address)
			.WithOne()
			.HasForeignKey<Address>(Constants.UserIdColumnName)
			.IsRequired()
			.OnDelete(DeleteBehavior.ClientCascade)
			.HasConstraintName(Constants.FkUserAddress)
			.Metadata.PrincipalToDependent
			?.SetPropertyAccessMode(PropertyAccessMode.Field);

		builder
			.HasOne<CivilityRef>()
			.WithMany()
			.HasForeignKey(static e => e.Civility)
			.IsRequired()
			.HasConstraintName(Constants.FkUserCivility);

		builder
			.HasOne<StatusRef>()
			.WithMany()
			.HasForeignKey(static e => e.Status)
			.IsRequired()
			.HasConstraintName(Constants.FkUserStatus);

		builder.HasIndex(static e => e.Civility).HasDatabaseName(Constants.IxUserCivility);
		builder.HasIndex(static e => e.Status).HasDatabaseName(Constants.IxUserStatus);
	}

	private static class Constants
	{
		internal const string PkUser = "pk_user";
		internal const string UserIdColumnName = "userId";
		internal const string FkUserCivility = "fk_user_civilityId";
		internal const string IxUserCivility = "ix_user_civilityId";
		internal const string IxUserStatus = "ix_user_statusId";
		internal const string FkUserChannel = "fk_user_userChannel";
		internal const string FkUserAddress = "fk_user_address";
		internal const string FkUserStatus = "fk_user_statusId";
	}
}