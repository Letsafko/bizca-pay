using System;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users;

public class UserChannelConfirmation : Entity<UserChannelConfirmationId>
{
	private UserChannelConfirmation(
		string confirmationCode,
		DateTimeOffset expirationDatetime,
		DateTimeOffset createdDatetime) : base(createdDatetime, createdDatetime)
	{
		ExpirationDatetime = expirationDatetime;
		ConfirmationCode = confirmationCode;
	}

	public static UserChannelConfirmation Create(
		string confirmationCode,
		DateTimeOffset expirationDatetime,
		DateTimeOffset createdDatetime)
	{
		return new UserChannelConfirmation(
			confirmationCode,
			expirationDatetime,
			createdDatetime);
	}

	public string ConfirmationCode { get; }
	public DateTimeOffset ExpirationDatetime { get; }

	public bool HasExpired(DateTimeOffset utcNow)
	{
		return utcNow > ExpirationDatetime;
	}
}