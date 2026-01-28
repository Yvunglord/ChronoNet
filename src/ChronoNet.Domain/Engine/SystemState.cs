namespace ChronoNet.Domain.Engine;

public sealed class SystemState
{
    public Dictionary<string, DeviceState> Devices { get; } = new();
    public List<EdgeState> Edges { get; } = new();

    public int TimeStep { get; private set; }

    public void Advance()
    {
        TimeStep++;
    }
}
