using System;
using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users;

public class UserChannel : Entity<UserChannelId>
{
	public ChannelValue ChannelValue { get; private set; }
	public ChannelType ChannelTypeId { get; private set; }
	public bool Confirmed { get; private set; }
	public IReadOnlyList<UserChannelConfirmation> UserChannelConfirmations => _userChannelConfirmations;
	private readonly List<UserChannelConfirmation> _userChannelConfirmations;

	private UserChannel(
		ChannelValue channelValue,
		ChannelType channelTypeId,
		DateTimeOffset createdDatetime,
		DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
	{
		_userChannelConfirmations = [];
		ChannelValue = channelValue;
		ChannelTypeId = channelTypeId;
		Confirmed = false;
	}

	public static UserChannel Create(
		ChannelValue channelValue,
		ChannelType channelType,
		DateTimeOffset creationDate)
	{
		return new UserChannel(
			channelValue,
			channelType,
			creationDate,
			lastModifiedDatetime: creationDate);
	}
}