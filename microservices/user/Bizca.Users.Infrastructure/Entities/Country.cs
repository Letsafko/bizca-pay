using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("country", Schema = "usr")]
[Index("CountryCode", Name = "ix_country_countryCode", IsUnique = true)]
public sealed class Country
{
	[Key][Column("countryId")] public short CountryId { get; init; }

	[Column("countryCode")]
	[StringLength(2)]
	[Unicode(false)]
	public string CountryCode { get; init; } = null!;

	[Column("description")]
	[StringLength(50)]
	[Unicode(false)]
	public string Description { get; init; } = null!;

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[InverseProperty("Country")] public ICollection<Address> Addresses { get; init; } = [];

	[InverseProperty("BirthCountry")] public ICollection<User> Users { get; init; } = [];
}