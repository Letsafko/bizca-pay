using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("user", Schema = "usr")]
[Index("BirthCountryId", Name = "ix_user_birthCountryId")]
[Index("CivilityId", Name = "ix_user_civilityId")]
[Index("EconomicActivityId", Name = "ix_user_economicActivityId")]
[Index("PartnerId", Name = "ix_user_partnerId")]
[Index("PartnerId", "ExternalUserId", Name = "ix_user_partnerId_externalUserId", IsUnique = true)]
[Index("UserCode", Name = "ix_user_userCode", IsUnique = true)]
public sealed class User
{
	[Key][Column("userId")] public int UserId { get; init; }

	[Column("externalUserId")]
	[StringLength(20)]
	[Unicode(false)]
	public string ExternalUserId { get; init; } = null!;

	[Column("userCode")] public Guid UserCode { get; init; }

	[Column("partnerId")] public short PartnerId { get; init; }

	[Column("civilityId")] public short CivilityId { get; init; }

	[Column("active")] public bool Active { get; init; }

	[Column("economicActivityId")] public short? EconomicActivityId { get; init; }

	[Column("firstName")]
	[StringLength(100)]
	public string FirstName { get; init; } = null!;

	[Column("lastName")]
	[StringLength(100)]
	public string LastName { get; init; } = null!;

	[Column("birthDate")] public DateOnly? BirthDate { get; init; }

	[Column("birthCountryId")] public short? BirthCountryId { get; init; }

	[Column("birthCity")]
	[StringLength(50)]
	[Unicode(false)]
	public string? BirthCity { get; init; }

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[Column("rowversion")]
	public required byte[] RowVersion { get; init; }

	[InverseProperty("User")] public ICollection<Address> Addresses { get; init; } = [];

	[ForeignKey("BirthCountryId")]
	[InverseProperty("Users")]
	public Country? BirthCountry { get; init; }

	[ForeignKey("CivilityId")]
	[InverseProperty("Users")]
	public required Civility Civility { get; init; }

	[ForeignKey("EconomicActivityId")]
	[InverseProperty("Users")]
	public EconomicActivity? EconomicActivity { get; init; }

	[ForeignKey("PartnerId")]
	[InverseProperty("Users")]
	public Partner? Partner { get; init; }

	[InverseProperty("User")] public ICollection<Password> Passwords { get; init; } = [];

	[InverseProperty("User")] public ICollection<UserChannelConfirmation> UserChannelConfirmations { get; init; } = [];

	[InverseProperty("User")] public ICollection<UserChannel> UserChannels { get; init; } = [];
}