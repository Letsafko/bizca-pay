using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[PrimaryKey("UserId", "ChannelMask")]
[Table("userChannel", Schema = "usr")]
[Index("UserId", Name = "ix_userChannel_userId")]
[Index("Value", "PartnerId", Name = "ix_userChannel_value_partnerId", IsUnique = true)]
public sealed class UserChannel
{
	[Key][Column("userId")] public int UserId { get; init; }

	[Key][Column("channelMask")] public short ChannelMask { get; init; }

	[Column("partnerId")] public short PartnerId { get; init; }

	[Column("value")]
	[StringLength(50)]
	[Unicode(false)]
	public string Value { get; init; } = null!;

	[Column("active")] public bool Active { get; init; }

	[Column("confirmed")] public bool Confirmed { get; init; }

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[ForeignKey("PartnerId")]
	[InverseProperty("UserChannels")]
	public Partner Partner { get; init; } = null!;

	[ForeignKey("UserId")]
	[InverseProperty("UserChannels")]
	public User User { get; init; } = null!;
}