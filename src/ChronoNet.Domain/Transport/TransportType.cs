namespace ChronoNet.Domain.Transport;

public sealed class TransportType
{
    public int Id { get; }
    public double TimePerChunk { get; }

    public IReadOnlyDictionary<int, double> SupportedFlows { get; }

    public TransportType(int id,
        double timePerChunk,
        IReadOnlyDictionary<int, double> supportedFlows)
    {
        Id = id;
        TimePerChunk = timePerChunk;
        SupportedFlows = supportedFlows;
    }
}

