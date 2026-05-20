using System;
using Bogus;
using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.Domain.Users.ValueObjects;

namespace Bizca.Users.UnitTests.Users.Fakers;

internal sealed class UserProfileFaker : Faker<UserProfile>
{
    public UserProfileFaker(
        string? firstName = null,
        string? lastName = null,
        Civility? civility = null,
        DateOnly? birthDate = null,
        string? birthCity = null,
        string? birthCountry = null,
        CountryCode? birthCountryCode = null,
        Address? address = null)
    {
        CustomInstantiator(f => new UserProfile(
            FirstName: firstName ?? f.Name.FirstName(),
            LastName: lastName ?? f.Name.LastName(),
            Civility: civility ?? f.PickRandom(Civility.Mr, Civility.Ms, Civility.Other),
            BirthDate: birthDate ?? DateOnly.FromDateTime(f.Date.Past(yearsToGoBack: 50, refDate: DateTime.UtcNow.AddYears(-18))),
            BirthCity: birthCity ?? f.Address.City(),
            BirthCountry: birthCountry ?? f.Address.Country(),
            BirthCountryCode: birthCountryCode,
            Address: address));
    }
}
