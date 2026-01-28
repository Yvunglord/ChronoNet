using ChronoNet.Application.DTO;
using ChronoNet.Domain;
using ChronoNet.Domain.Enums;

namespace ChronoNet.Application.Services;

public class FlowSimulationService
{
    public void Simulate(TemporalGraph graph,
        SimulationContext context)
    {
        ReceiveInitialFlows(graph, context);
        ExecuteProcesses(graph, context);
        ExecuteTransport(graph, context);
        UpdateStorage(graph);
    }

    private void ReceiveInitialFlows(TemporalGraph graph, SimulationContext ctx)
    {
        foreach (var info in graph.DeviceInfos.Values)
        {
            info.IncomingFlows.Clear();
            info.OutgoingFlows.Clear();
            info.StoredFlows.Clear();
        }

        foreach (var device in graph.Vertices)
        {
            var info = graph.DeviceInfos[device.Id];

            foreach (var input in device.InputFlows)
            {
                info.StoredFlows[input.Key] = input.Value;
                info.AddIncoming(input.Key, input.Value);
            }
        }
    }

    private void ExecuteProcesses(TemporalGraph graph, SimulationContext ctx)
    {
        double intervalTime = graph.Interval.Duration;

        foreach (var device in graph.Vertices)
        {
            var info = graph.DeviceInfos[device.Id];

            foreach (var procId in device.SupportedProcessTypes)
            {
                if (!ctx.Processes.TryGetValue(procId, out var proc))
                    continue;

                double maxByTime = intervalTime / proc.TimePerChunk;

                double maxByInput = proc.InputFlows
                    .Select(input =>
                        info.StoredFlows.TryGetValue(input.Key, out var available)
                        ? available / input.Value
                        : 0)
                    .Min();

                double executions = Math.Floor(Math.Min(maxByTime, maxByInput));
                if (executions <= 0)
                    continue;

                foreach (var input in device.InputFlows)
                    info.StoredFlows[input.Key] -= input.Value * executions;

                foreach (var output in device.OutputFlows)
                {
                    if (!info.StoredFlows.ContainsKey(output.Key))
                        info.StoredFlows[output.Key] = 0;

                    info.StoredFlows[output.Key] += output.Value * executions;
                }
            }
        }
    }

    private void ExecuteTransport(TemporalGraph graph, SimulationContext ctx)
    {
        foreach (var edge in graph.Edges)
        {
            Guid? from = edge.Direction switch
            {
                EdgeDirection.Right => edge.From,
                EdgeDirection.Left => edge.To,
                _ => null
            };

            Guid? to = edge.Direction switch
            {
                EdgeDirection.Right => edge.To,
                EdgeDirection.Left => edge.From,
                _ => null
            };

            if (from == null || to == null)
                continue;

            var sourceInfos = graph.DeviceInfos[from.Value];
            var targetInfos = graph.DeviceInfos[to.Value];

            foreach (var transportId in edge.SupportedTransprtTypes)
            {
                if (!ctx.Transports.TryGetValue(transportId, out var transport))
                    continue;

                double maxByTime = graph.Interval.Duration / transport.TimePerChunk;

                foreach (var flow in transport.SupportedFlows)
                {
                    if (!sourceInfos.StoredFlows.TryGetValue(flow.Key, out var available))
                        continue;

                    double amount = Math.Min(available, maxByTime * flow.Value);

                    sourceInfos.StoredFlows[flow.Key] -= amount;
                    sourceInfos.AddOutgoing(flow.Key, amount);

                    if (!targetInfos.StoredFlows.ContainsKey(flow.Key))
                        targetInfos.StoredFlows[flow.Key] = 0;

                    targetInfos.StoredFlows[flow.Key] += amount;
                    targetInfos.AddIncoming(flow.Key, amount);
                }
            }
        }
    }

    private void UpdateStorage(TemporalGraph graph)
    {
        foreach (var device in graph.Vertices)
        {
            var info = graph.DeviceInfos[device.Id];

            foreach (var cap in device.StorageCapacities)
            {
                if (info.StoredFlows.TryGetValue(cap.Key, out var amount)
                    && amount > cap.Value)
                {
                    info.StoredFlows[cap.Key] = cap.Value;
                }
            }
        }
    }
}
