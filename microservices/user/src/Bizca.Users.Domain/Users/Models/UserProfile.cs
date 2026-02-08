using System;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.Domain.Users.Models;

public sealed record UserProfile(
	string FirstName,
	string LastName,
	Civility Civility,
	DateOnly? BirthDate,
	string? BirthCity,
	string? BirthCountry,
	CountryCode? BirthCountryCode,
	Address? Address);