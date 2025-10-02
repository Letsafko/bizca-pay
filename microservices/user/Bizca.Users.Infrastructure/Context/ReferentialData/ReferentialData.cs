namespace Bizca.Users.Infrastructure.Context.ReferentialData;

internal abstract class ReferentialData<TId>
{
	public required TId Id { get; init; }
	public required string Label { get; init; }
	public required string Description { get; init; }
}