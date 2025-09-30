using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("partner", Schema = "usr")]
[Index("PartnerCode", Name = "ix_partner_partnerCode", IsUnique = true)]
public sealed class Partner
{
	[Key][Column("partnerId")] public short PartnerId { get; init; }

	[Column("partnerCode")]
	[StringLength(10)]
	[Unicode(false)]
	public string PartnerCode { get; init; } = null!;

	[Column("description")]
	[StringLength(50)]
	[Unicode(false)]
	public string Description { get; init; } = null!;

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[InverseProperty("Partner")] public ICollection<UserChannel> UserChannels { get; init; } = [];

	[InverseProperty("Partner")] public ICollection<User> Users { get; init; } = [];
}