using ChronoNet.Domain.Flows;
using ChronoNet.Domain.Processes;
using ChronoNet.Domain.Storage;
using ChronoNet.Domain.Transport;

namespace ChronoNet.Infrastructure.Xml;

public class StaticModel
{
    public IReadOnlyCollection<FlowType> Flows { get; init; } = Array.Empty<FlowType>();
    public IReadOnlyCollection<ProcessType> Processes { get; init; } = Array.Empty<ProcessType>();
    public IReadOnlyCollection<TransportType> Transports { get; init; } = Array.Empty<TransportType>();
    public IReadOnlyCollection<StorageType> Srorages { get; init; } = Array.Empty<StorageType>();
}