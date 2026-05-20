using System;
using Bizca.Users.Domain.Users;
using Bizca.Users.Domain.Users.Models;
using Bizca.Users.UnitTests.Users.Fakers;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users;

[Trait("Category", "Unit")]
public sealed class UserTests
{
    private static readonly UserProfileFaker ProfileFaker = new();

    [Fact]
    public void ANewUser_IsInactiveWithDraftStatus_UntilConfirmed()
    {
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(ProfileFaker.Generate(), passwordHash: null, securityStamp: null, now);

        user.Active.Should().BeFalse();
        user.Status.Should().Be(Status.Draft);
    }

    [Fact]
    public void ANewUser_ReceivesAUniqueExternalIdentity_OnCreation()
    {
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(ProfileFaker.Generate(), passwordHash: null, securityStamp: null, now);

        user.ExternalUserId.Should().NotBeNull();
        user.ExternalUserId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void ANewUser_HasItsCreationTimestamp_SetToTheProvidedDate()
    {
        var now = DateTimeOffset.UtcNow;

        var user = User.Create(ProfileFaker.Generate(), passwordHash: null, securityStamp: null, now);

        user.CreatedDatetime.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TwoNewUsers_HaveDifferentExternalIdentities()
    {
        var now = DateTimeOffset.UtcNow;

        var userA = User.Create(ProfileFaker.Generate(), passwordHash: null, securityStamp: null, now);
        var userB = User.Create(ProfileFaker.Generate(), passwordHash: null, securityStamp: null, now);

        userA.ExternalUserId.Value.Should().NotBe(userB.ExternalUserId.Value);
    }
}
