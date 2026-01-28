namespace ChronoNet.Domain.Processes;

public sealed class ProcessType
{
    public int Id { get; }
    public double TimePerChunk { get; }


    public IReadOnlyDictionary<int, double> InputFlows { get; }
    public IReadOnlyDictionary<int, double> OutputFlows { get; }

    public ProcessType(int id,
        double timePerChunk,
        IReadOnlyDictionary<int, double> inputFlows,
        IReadOnlyDictionary<int, double> outputFlows)
    {
        Id = id;
        TimePerChunk = timePerChunk;
        InputFlows = inputFlows;
        OutputFlows = outputFlows;
    }
}
