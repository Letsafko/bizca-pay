using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("address", Schema = "usr")]
[Index("CountryId", Name = "ix_address_countryId")]
[Index("UserId", Name = "ix_address_userId")]
public sealed class Address
{
	[Key][Column("addressId")] public int AddressId { get; init; }

	[Column("userId")] public int UserId { get; init; }

	[Column("active")] public bool Active { get; init; }

	[Column("addressName")]
	[StringLength(100)]
	[Unicode(false)]
	public string? AddressName { get; init; }

	[Column("city")]
	[StringLength(100)]
	[Unicode(false)]
	public string? City { get; init; }

	[Column("zipcode")]
	[StringLength(10)]
	[Unicode(false)]
	public string? Zipcode { get; init; }

	[Column("street")]
	[StringLength(255)]
	[Unicode(false)]
	public string? Street { get; init; }

	[Column("countryId")] public short CountryId { get; init; }

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[ForeignKey("CountryId")]
	[InverseProperty("Addresses")]
	public required Country Country { get; init; }

	[ForeignKey("UserId")]
	[InverseProperty("Addresses")]
	public User User { get; init; } = null!;
}