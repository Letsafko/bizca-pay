# Testing Patterns Reference

Quick reference for testing patterns in the Bizca .NET solution. Use alongside the `test-driven-development` skill.

Stack: **xUnit**, **FluentAssertions**, **AutoFixture**, **Bogus**, **Moq / Moq.Contrib.HttpClient**, **Reqnroll**, **Testcontainers.PostgreSql**.

## Table of Contents

- [Test Structure (Arrange-Act-Assert)](#test-structure-arrange-act-assert)
- [Test Naming Conventions](#test-naming-conventions)
- [FluentAssertions — Common Assertions](#fluentassertions--common-assertions)
- [Mocking Patterns (Moq)](#mocking-patterns-moq)
- [Test Data — AutoFixture & Bogus](#test-data--autofixture--bogus)
- [Domain Unit Tests](#domain-unit-tests)
- [Integration Tests (EF Core + Testcontainers)](#integration-tests-ef-core--testcontainers)
- [Functional Tests (Reqnroll)](#functional-tests-reqnroll)
- [Test Anti-Patterns](#test-anti-patterns)

---

## Test Structure (Arrange-Act-Assert)

```csharp
[Fact]
public void Create_WithValidData_ReturnsSuccess()
{
    // Arrange
    const string email = "alice@example.com";

    // Act
    var result = Email.Create(email);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be(email);
}
```

---

## Test Naming Conventions

```
[MethodOrScenario]_[Condition]_[ExpectedBehavior]
```

```csharp
public class UserTests
{
    [Fact] public void Create_WithValidData_ReturnsSuccess() { }
    [Fact] public void Create_WithEmptyEmail_ReturnsValidationFailure() { }
    [Fact] public void Create_WithDuplicateEmail_ReturnsConflict() { }
    [Fact] public void Deactivate_WhenAlreadyInactive_ReturnsFailure() { }
}
```

---

## FluentAssertions — Common Assertions

```csharp
// Result<T>
result.IsSuccess.Should().BeTrue();
result.IsFailure.Should().BeTrue();
result.Error.Type.Should().Be(ErrorType.Validation);
result.Error.Should().Be(UserErrors.NotFound);

// Object / DTO equality
user.Email.Value.Should().Be("alice@example.com");
dto.Should().BeEquivalentTo(expected, opts => opts.ExcludingMissingMembers());

// Collections
users.Should().HaveCount(3);
users.Should().ContainSingle(u => u.Email.Value == "alice@example.com");
users.Should().BeEmpty();
users.Should().AllSatisfy(u => u.IsActive.Should().BeTrue());

// HTTP
response.StatusCode.Should().Be(HttpStatusCode.Created);
statusCode.Should().Be(201);

// Null
value.Should().NotBeNull();
value.Should().BeNull();

// Dates
user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

// Exceptions
act.Should().Throw<InvalidOperationException>().WithMessage("*expected*");
await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
```

---

## Mocking Patterns (Moq)

### Service / repository mock

```csharp
var mockRepo = new Mock<IUserRepository>();

mockRepo
    .Setup(r => r.GetByIdAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);

var sut = new UserService(mockRepo.Object);
```

### HttpClient mock (Moq.Contrib.HttpClient)

```csharp
var handler = new Mock<HttpMessageHandler>();
handler
    .SetupRequest(HttpMethod.Get, "https://api.example.com/users/1")
    .ReturnsResponse(HttpStatusCode.OK, JsonContent.Create(userDto));

var client = handler.CreateClient();
```

### Verify calls

```csharp
mockRepo.Verify(
    r => r.AddAsync(It.Is<User>(u => u.Email.Value == "alice@example.com"),
                    It.IsAny<CancellationToken>()),
    Times.Once);

mockRepo.VerifyNoOtherCalls();
```

### Mock at boundaries only

```
Mock these (infrastructure):        Don't mock these (domain):
├── IUserRepository                 ├── Entity factories (User.Create)
├── IDateTimeProvider               ├── Value Objects (Email, Address)
├── HttpClient / external APIs      ├── Result<T> helpers
└── IEmailService / messaging       └── Pure domain methods
```

---

## Test Data — AutoFixture & Bogus

### AutoFixture (structural data)

```csharp
var fixture = new Fixture();
var userId  = fixture.Create<Guid>();
var dto     = fixture.Build<UserDto>()
                     .With(x => x.Email, "alice@example.com")
                     .Create();
```

### Bogus (realistic fake data)

```csharp
var faker = new Faker<CreateUserRequest>()
    .RuleFor(x => x.FirstName, f => f.Name.FirstName())
    .RuleFor(x => x.LastName,  f => f.Name.LastName())
    .RuleFor(x => x.Email,     f => f.Internet.Email());

var request = faker.Generate();
var batch   = faker.Generate(10);
```

---

## Domain Unit Tests

Pure .NET tests — no DB, no HTTP, no DI container. Fast and deterministic.

```csharp
public class EmailTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("bob+tag@domain.co")]
    public void Create_WithValidEmail_ReturnsSuccess(string raw)
    {
        var result = Email.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ReturnsValidationFailure(string? raw)
    {
        var result = Email.Create(raw!);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
```

---

## Integration Tests (EF Core + Testcontainers)

### Shared database fixture

```csharp
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public ApplicationDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
```

### Test class

```csharp
[Collection("Database")]
public class UserRepositoryTests(DatabaseFixture db) : IAsyncLifetime
{
    // Truncate per test — container is shared, data is not
    public Task InitializeAsync() =>
        db.Context.Database.ExecuteSqlRawAsync("TRUNCATE users CASCADE");

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_PersistsUser_CanBeRetrievedById()
    {
        var user = User.Create("Alice", "alice@example.com").Value;

        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();

        var stored = await db.Context.Users.FindAsync(user.Id);
        stored.Should().NotBeNull();
        stored!.Email.Value.Should().Be("alice@example.com");
    }
}
```

---

## Functional Tests (Reqnroll)

### Feature file

```gherkin
Feature: Create user

  Scenario: Successfully create a user with valid data
    Given the following user details
      | FirstName | LastName | Email             |
      | Alice     | Smith    | alice@example.com |
    When I send a POST request to "/api/users"
    Then the response status code should be 201
    And the response should contain the created user
```

### Step definitions

```csharp
[Binding]
public sealed class CreateUserSteps(ScenarioContext context, HttpClient client)
{
    private const string StatusCodeKey = "StatusCode";
    private const string ResponseKey   = "UserResponse";

    [Given("the following user details")]
    public void GivenUserDetails(DataTable table)
    {
        // Use CreateInstance<T>() — never row["ColumnName"]
        context.Set(table.CreateInstance<CreateUserRequest>(), "Request");
    }

    [When(@"I send a POST request to ""(.*)""")]
    public async Task WhenISendPost(string path)
    {
        var request  = context.Get<CreateUserRequest>("Request");
        var response = await client.PostAsJsonAsync(path, request);

        // Deserialize immediately — never store HttpResponseMessage (IDisposable)
        var dto = await response.Content.ReadFromJsonAsync<UserResponse>();
        context.Set((int)response.StatusCode, StatusCodeKey);
        context.Set(dto, ResponseKey);
    }

    [Then("the response status code should be (.*)")]
    public void ThenStatusCode(int expected) =>
        context.Get<int>(StatusCodeKey).Should().Be(expected);

    [Then("the response should contain the created user")]
    public void ThenResponseContainsUser()
    {
        var request  = context.Get<CreateUserRequest>("Request");
        var response = context.Get<UserResponse>(ResponseKey);
        response!.Email.Should().Be(request.Email);
    }
}
```

### Hooks

```csharp
[Binding]
public sealed class DatabaseHooks(ApplicationDbContext db)
{
    // Testcontainers manages container lifecycle.
    // This hook manages DATA lifecycle — required to isolate scenarios.
    [BeforeScenario]
    public async Task TruncateTables() =>
        await db.Database.ExecuteSqlRawAsync("TRUNCATE users CASCADE");
}
```

---

## Test Anti-Patterns

| Anti-Pattern | Problem | Fix |
|---|---|---|
| `Assert.Equal` only | Poor failure messages | Use FluentAssertions (`.Should().Be()`) |
| Storing `HttpResponseMessage` in `ScenarioContext` | `IDisposable` leak | Deserialize immediately; store `int` + typed DTO |
| `row["ColumnName"]` on DataTable | Fragile, verbose | `table.CreateInstance<T>()` or `table.CreateSet<T>()` |
| Mocking `DbContext` / `DbSet` directly | Misses real query behavior | Use Testcontainers with real PostgreSQL |
| No `[BeforeScenario]` truncation | Data leaks between scenarios | Always truncate in `[BeforeScenario]` |
| `Thread.Sleep` in tests | Flaky under load | `await Task.Delay` or polling with timeout |
| Magic strings for `ScenarioContext` keys | Typo causes runtime `KeyNotFoundException` | `private const string` keys per step class |
| Testing implementation details | Brittle on refactor | Test observable behavior: `Result<T>`, HTTP status, persisted data |
| Shared mutable static state | Order-dependent failures | Stateless unit tests; truncate DB per scenario |
