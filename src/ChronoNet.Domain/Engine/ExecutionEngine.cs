using ChronoNet.Domain.Processes;

namespace ChronoNet.Domain.Engine;

public sealed class ExecutionEngine
{
    public void Step(SystemState state)
    {
        var actions = CollectActions(state);

        foreach (var action in actions)
        {
            action.Apply(state);
        }

        state.Advance();
    }

    private List<dynamic> CollectActions(SystemState state)
    {
        var actions = new List<dynamic>();

        foreach (var device in state.Devices.Values)
        {
            if (device.CanCompute && device.Storage.GetValueOrDefault(1) > 0)
            {
                actions.Add(
                    new ProcessAction(
                        device.Id,
                        inputFlow: 1,
                        outputFlow: 2,
                        amount: 10));
            }
        }

        foreach (var edge in state.Edges)
        {
            var from = state.Devices[edge.From];

            if (from.Storage.GetValueOrDefault(2) > 0)
            {
                actions.Add(
                    new TransportAction(
                        edge.From,
                        edge.To,
                        flow: 2,
                        amount: 10));
            }
        }

        return actions;
    }
}
