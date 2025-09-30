using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("civility", Schema = "usr")]
[Index("CivilityCode", Name = "ix_civility_civilityCode", IsUnique = true)]
public sealed class Civility
{
	[Key][Column("civilityId")] public short CivilityId { get; init; }

	[Column("civilityCode")]
	[StringLength(5)]
	[Unicode(false)]
	public required string CivilityCode { get; init; }

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[InverseProperty("Civility")] public ICollection<User> Users { get; init; } = [];
}