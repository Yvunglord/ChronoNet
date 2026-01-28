namespace ChronoNet.Domain.Engine;

public sealed class ProcessAction
{
    public string DeviceId { get; }
    public int InputFlow { get; }
    public int OutputFlow { get; }
    public double Amount { get; }

    public ProcessAction(string deviceId,
        int inputFlow, int outputFlow, double amount)
    {
        DeviceId = deviceId;
        InputFlow = inputFlow;
        OutputFlow = outputFlow;
        Amount = amount;
    }

    public bool CanApply(SystemState state)
    {
        var d = state.Devices[DeviceId];

        return d.CanCompute
            && d.Storage.GetValueOrDefault(InputFlow) >= Amount
            && d.ComputePerStep >= Amount;
    }

    public void Apply(SystemState state)
    {
        var d = state.Devices[DeviceId];

        d.Storage[InputFlow] -= Amount;
        d.Storage[OutputFlow] = d.Storage.GetValueOrDefault(OutputFlow) + Amount;
    }
}
