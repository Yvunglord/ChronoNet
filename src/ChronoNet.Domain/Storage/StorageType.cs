namespace ChronoNet.Domain.Storage;

public sealed class StorageType
{
    public int Id { get; }
    public IReadOnlyCollection<int> SupportedFlows { get; }

    public StorageType(int id,
        IReadOnlyCollection<int> supportedFlows)
    {
        Id = id;
        SupportedFlows = supportedFlows;
    }
}
