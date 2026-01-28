namespace ChronoNet.Domain.Engine;

public sealed class DeviceState
{
    public string Id { get; }

    public Dictionary<int, double> Storage { get; } = new();
    public Dictionary<int, double> StorageCapacity { get; } = new();

    public bool CanCompute { get; set; }
    public bool CanSend { get; set; }
    public bool CanReceive { get; set; }

    public double ComputePerStep { get; set; }    

    public DeviceState(string id)
    {
        Id = id;
    }
}
