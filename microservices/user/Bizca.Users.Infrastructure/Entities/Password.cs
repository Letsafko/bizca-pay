using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bizca.Users.Infrastructure.Entities;

[Table("password", Schema = "usr")]
[Index("UserId", Name = "ix_password_userId")]
public sealed class Password
{
	[Key][Column("passwordId")] public int PasswordId { get; init; }

	[Column("userId")] public int UserId { get; init; }

	[Column("active")] public bool Active { get; init; }

	[Column("securityStamp")]
	[StringLength(250)]
	[Unicode(false)]
	public string SecurityStamp { get; init; } = null!;

	[Column("passwordHash")]
	[StringLength(250)]
	[Unicode(false)]
	public string PasswordHash { get; init; } = null!;

	[Column("creationDate")] public DateTime CreationDate { get; init; }

	[ForeignKey("UserId")]
	[InverseProperty("Passwords")]
	public User User { get; init; } = null!;
}