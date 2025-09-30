using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("economicActivity", Schema = "usr")]
[Index("EconomicActivityCode", Name = "ix_economicActivity_economicActivityCode", IsUnique = true)]
public sealed class EconomicActivity
{
	[Key][Column("economicActivityId")] public short EconomicActivityId { get; init; }

	[Column("economicActivityCode")]
	[StringLength(30)]
	[Unicode(false)]
	public string EconomicActivityCode { get; init; } = null!;

	[Column("description")]
	[StringLength(50)]
	[Unicode(false)]
	public string Description { get; init; } = null!;

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[InverseProperty("EconomicActivity")] public ICollection<User> Users { get; init; } = [];
}