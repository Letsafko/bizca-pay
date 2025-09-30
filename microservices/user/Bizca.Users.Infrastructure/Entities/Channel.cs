using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("channel", Schema = "usr")]
[Index("ChannelCode", Name = "ix_channel_channelCode", IsUnique = true)]
public sealed class Channel
{
	[Key][Column("channelId")] public short ChannelId { get; init; }

	[Column("channelCode")]
	[StringLength(30)]
	[Unicode(false)]
	public string ChannelCode { get; init; } = null!;

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("lastUpdate")] public DateTime LastUpdate { get; init; }

	[InverseProperty("Channel")] public ICollection<UserChannelConfirmation> UserChannelConfirmations { get; init; } = [];
}