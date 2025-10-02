using System;
using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users;

public class User : Entity<UserId>, IVersionedEntity
{
	private User(
		ExternalUserId externalUserId,
		string firstName,
		string lastName,
		Status status,
		Civility civility,
		bool active,
		DateOnly? birthDate,
		string? birthCity,
		string? birthCountry,
		CountryCode? birthCountryCode,
		string? passwordHash,
		string? securityStamp,
		DateTimeOffset createdDatetime,
		DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
	{
		Active = active;
		ExternalUserId = externalUserId;
		Status = status;
		Civility = civility;
		FirstName = firstName;
		LastName = lastName;
		BirthDate = birthDate;
		BirthCity = birthCity;
		BirthCountry = birthCountry;
		BirthCountryCode = birthCountryCode;
		PasswordHash = passwordHash;
		SecurityStamp = securityStamp;
	}

	public static User Create(
		UserProfile userProfile,
		string? passwordHash,
		string? securityStamp,
		DateTimeOffset createdDatetime)
	{
		return new User(
			ExternalUserId.Create(Guid.CreateVersion7()).Value,
			userProfile.FirstName,
			userProfile.LastName,
			status: Status.Draft,
			userProfile.Civility,
			active: false,
			userProfile.BirthDate,
			userProfile.BirthCity,
			userProfile.BirthCountry,
			userProfile.BirthCountryCode,
			passwordHash,
			securityStamp,
			createdDatetime,
			lastModifiedDatetime: createdDatetime);
	}

	public ExternalUserId ExternalUserId { get; private set; }
	public Civility Civility { get; private set; }
	public bool Active { get; private set; }
	public Status Status { get; private set; }
	public string FirstName { get; private set; }
	public string LastName { get; private set; }
	public DateOnly? BirthDate { get; private set; }
	public string? BirthCity { get; private set; }
	public string? BirthCountry { get; private set; }
	public CountryCode? BirthCountryCode { get; private set; }
	public string? SecurityStamp { get; private set; }
	public string? PasswordHash { get; private set; }
	public byte[] Version { get; init; } = [];
	public Address? Address => _address;
	private readonly Address? _address;
	public IReadOnlyList<UserChannel> UserChannels => _userChannels ?? [];
	private readonly List<UserChannel>? _userChannels;
}