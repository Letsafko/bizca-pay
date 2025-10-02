using System;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users;

public sealed class Address : Entity<AddressId>
{
	private Address(
		CountryCode? countryCode,
		string? city,
		string? zipcode,
		string? street,
		string? country,
		DateTimeOffset createdDatetime,
		DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
	{
		CountryCode = countryCode;
		Country = country;
		Zipcode = zipcode;
		Street = street;
		City = city;
	}

	public static Address Create(
		CountryCode? countryCode,
		string? city,
		string? zipcode,
		string? street,
		string? country,
		DateTimeOffset creationDate)
	{
		return new Address(countryCode, city, zipcode, street, country, creationDate, creationDate);
	}

	public string? City { get; private set; }
	public string? Zipcode { get; private set; }
	public string? Street { get; private set; }
	public string? Country { get; private set; }
	public CountryCode? CountryCode { get; }
}
