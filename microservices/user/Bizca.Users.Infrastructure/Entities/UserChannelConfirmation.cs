using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[PrimaryKey("UserId", "ChannelId", "CreationDate")]
[Table("userChannelConfirmation", Schema = "usr")]
[Index("ChannelId", Name = "ix_userChannelConfirmation_channelId")]
[Index("UserId", Name = "ix_userChannelConfirmation_userId")]
[Index("UserId", "ChannelId", Name = "ix_userChannelConfirmation_userId_channelId")]
public sealed class UserChannelConfirmation
{
	[Key][Column("userId")] public int UserId { get; init; }

	[Key][Column("channelId")] public short ChannelId { get; init; }

	[Column("confirmationCode")]
	[StringLength(50)]
	[Unicode(false)]
	public string ConfirmationCode { get; init; } = null!;

	[Key][Column("creationDate")] public DateTime CreationDate { get; init; }

	[Column("expirationDate")] public DateTime? ExpirationDate { get; init; }

	[ForeignKey("ChannelId")]
	[InverseProperty("UserChannelConfirmations")]
	public Channel Channel { get; init; } = null!;

	[ForeignKey("UserId")]
	[InverseProperty("UserChannelConfirmations")]
	public User User { get; init; } = null!;
}