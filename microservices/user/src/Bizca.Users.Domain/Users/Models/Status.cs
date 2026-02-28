using System;

namespace Bizca.Users.Domain.Users.Models;

[Flags]
public enum Status
{
	None = 0,
	Draft = 1,
	KycPending = 2,
	KycVerified = 4,
	Active = 8
}