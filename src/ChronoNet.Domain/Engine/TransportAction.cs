using ChronoNet.Domain.Processes;

namespace ChronoNet.Domain.Engine;

public sealed class TransportAction
{
    public string From { get; }
    public string To { get; }
    public int Flow { get; }
    public double Amount { get; }

    public TransportAction(string from, string to, int flow, double amount)
    {
        From = from;
        To = to;
        Flow = flow;
        Amount = amount;
    }

    public bool CanApply(SystemState state)
    {
        var src = state.Devices[From];
        var dst = state.Devices[To];

        return src.CanSend
            && dst.CanReceive
            && src.Storage.GetValueOrDefault(Flow) >= Amount
            && dst.Storage.GetValueOrDefault(Flow) + Amount 
            <= dst.StorageCapacity.GetValueOrDefault(Flow);
    }

    public void Apply(SystemState state)
    {
        state.Devices[From].Storage[Flow] -= Amount;
        state.Devices[To].Storage[Flow] += Amount;
    }
}
