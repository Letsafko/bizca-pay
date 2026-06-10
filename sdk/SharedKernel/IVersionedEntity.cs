namespace Bizca.Sdk.SharedKernel;

public interface IVersionedEntity
{
	public byte[] Version { get; }
}