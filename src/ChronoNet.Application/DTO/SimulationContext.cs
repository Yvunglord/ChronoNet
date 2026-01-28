using ChronoNet.Domain.Processes;
using ChronoNet.Domain.Transport;

namespace ChronoNet.Application.DTO;

public sealed class SimulationContext
{
    public IReadOnlyDictionary<int, ProcessType> Processes { get; }
    public IReadOnlyDictionary<int, TransportType> Transports { get; }

    public SimulationContext(Dictionary<int, ProcessType> processes,
        Dictionary<int, TransportType> transports)
    {
        Processes = processes;
        Transports = transports;
    }
}