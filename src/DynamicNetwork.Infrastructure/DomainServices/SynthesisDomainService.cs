using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Functions;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using DynamicNetwork.Domain.Services;
using DynamicNetwork.Domain.Synthesis;

namespace DynamicNetwork.Infrastructure.DomainServices;

public class SynthesisDomainService : ISynthesisDomainService
{
    public IReadOnlyList<StructConfiguration> SynthesizeAll(
        StructConfigurationSynthesisRequest request,
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<DataFlow> flows,
        FunctionLibrary functionLibrary,
        IReadOnlyList<StructConfiguration> baseConfigurations,
        IReadOnlyList<ReachabilityPath> reachablePaths)
    {
        // 1. Построение маппинга интервалов -> базовые конфигурации
        var intervalToBaseConfig = BuildIntervalToBaseConfigMap(
            graphs,
            reachablePaths,
            baseConfigurations);

        // 2. Планирование маршрутов для каждого потока
        var flowRoutePlans = new List<FlowRoutePlan>();
        foreach (var flow in flows)
        {
            flowRoutePlans.AddRange(PlanFlowRoutes(
                flow,
                reachablePaths,
                request,
                intervalToBaseConfig));
        }

        // 3. Агрегация требований к ресурсам
        var resourceRequirements = AggregateResourceRequirements(
            flowRoutePlans,
            intervalToBaseConfig);

        // 4. Синтез финальных конфигураций
        var synthesizedStructs = SynthesizeConfigurations(
            resourceRequirements,
            intervalToBaseConfig,
            functionLibrary);

        return synthesizedStructs
            .OrderBy(c => c.Interval.Start)
            .ToList()
            .AsReadOnly();
    }

    private Dictionary<TimeInterval, StructConfiguration> BuildIntervalToBaseConfigMap(
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<ReachabilityPath> paths,
        IReadOnlyList<StructConfiguration> baseConfigurations)
    {
        var intervalToBaseConfig = new Dictionary<TimeInterval, StructConfiguration>();

        var intervals = paths
            .Select(p => p.Interval)
            .Where(i => i != TimeInterval.Empty)
            .Distinct()
            .OrderBy(i => i.Start)
            .ToList();

        foreach (var interval in intervals)
        {
            var baseConfig = baseConfigurations.FirstOrDefault(c => c.Interval == interval);
            if (baseConfig != null)
                intervalToBaseConfig[interval] = baseConfig;
        }

        if (paths.Any())
        {
            var firstPathInterval = paths.First().Interval;
            foreach (var graph in graphs.Where(g => g.Interval.Overlaps(firstPathInterval)))
            {
                if (!intervalToBaseConfig.ContainsKey(graph.Interval))
                {
                    var baseConfig = baseConfigurations.FirstOrDefault(c => c.Interval == graph.Interval);
                    if (baseConfig != null)
                        intervalToBaseConfig[graph.Interval] = baseConfig;
                }
            }
        }

        return intervalToBaseConfig;
    }

    private List<FlowRoutePlan> PlanFlowRoutes(
        DataFlow flow,
        IReadOnlyList<ReachabilityPath> allPaths,
        StructConfigurationSynthesisRequest request,
        Dictionary<TimeInterval, StructConfiguration> intervalToBaseConfig)
    {
        var plans = new List<FlowRoutePlan>();
        var sourceNodes = request.NodeInputs.Keys.Select(nc => nc.NodeId).ToHashSet();
        var targetNodes = request.OutputNodes.Select(nc => nc.NodeId).ToHashSet();

        var validPaths = allPaths
            .Where(p => sourceNodes.Contains(p.Nodes.First()) && targetNodes.Contains(p.Nodes.Last()))
            .ToList();

        if (!validPaths.Any())
            return plans;

        foreach (var path in validPaths)
        {
            var currentFlowType = flow.Id;
            var transformations = new List<FlowTransformationStep>();
            var nodeFlowTypes = new Dictionary<string, string>();
            nodeFlowTypes[path.Nodes[0]] = currentFlowType;

            for (int i = 0; i < path.Edges.Count; i++)
            {
                var edge = path.Edges[i];
                var currentNode = path.Nodes[i];
                var nextNode = path.Nodes[i + 1];
                var edgeInterval = path.Interval;

                var requiredTransformation = flow.Transformations
                    .FirstOrDefault(t => t.InputType == currentFlowType);

                string transportFlowType;

                if (requiredTransformation != null)
                {
                    transformations.Add(new FlowTransformationStep
                    {
                        NodeId = currentNode,
                        InputType = currentFlowType,
                        OutputType = requiredTransformation.OutputType,
                        Interval = edgeInterval,
                        FlowVolume = flow.Volume
                    });

                    currentFlowType = requiredTransformation.OutputType;
                    nodeFlowTypes[currentNode] = currentFlowType;
                    transportFlowType = currentFlowType;
                }
                else
                {
                    transportFlowType = currentFlowType;
                }

                transformations.Add(new FlowTransformationStep
                {
                    SourceNode = currentNode,
                    TargetNode = nextNode,
                    TransportType = transportFlowType,
                    Interval = edgeInterval,
                    FlowVolume = flow.Volume
                });

                if (!nodeFlowTypes.ContainsKey(nextNode))
                    nodeFlowTypes[nextNode] = currentFlowType;
            }

            var finalNode = path.Nodes.Last();
            var finalFlowType = currentFlowType;

            if (flow.Transformations.Any() && finalFlowType != flow.Transformations.Last().OutputType)
            {
                var lastTransformation = flow.Transformations.Last();
                if (finalFlowType == lastTransformation.InputType)
                {
                    transformations.Add(new FlowTransformationStep
                    {
                        NodeId = finalNode,
                        InputType = finalFlowType,
                        OutputType = lastTransformation.OutputType,
                        Interval = path.Interval,
                        FlowVolume = flow.Volume
                    });

                    nodeFlowTypes[finalNode] = lastTransformation.OutputType;
                }
            }

            plans.Add(new FlowRoutePlan
            {
                Flow = flow,
                Path = path,
                Transformations = transformations,
                NodeFlowTypes = nodeFlowTypes
            });
        }

        return plans;
    }

    private ResourceRequirements AggregateResourceRequirements(
        List<FlowRoutePlan> flowRoutePlans,
        Dictionary<TimeInterval, StructConfiguration> intervalToBaseConfig)
    {
        var requirements = new ResourceRequirements
        {
            NodeProcessRequirements = new Dictionary<string, Dictionary<TimeInterval, HashSet<string>>>(),
            LinkTransportRequirements = new Dictionary<(string, string), Dictionary<TimeInterval, HashSet<string>>>(),
            NodeStorageRequirements = new Dictionary<string, Dictionary<TimeInterval, Dictionary<string, double>>>()
        };

        foreach (var plan in flowRoutePlans)
        {
            foreach (var transformation in plan.Transformations)
            {
                if (!string.IsNullOrEmpty(transformation.NodeId))
                {
                    if (!requirements.NodeProcessRequirements.ContainsKey(transformation.NodeId))
                        requirements.NodeProcessRequirements[transformation.NodeId] = new Dictionary<TimeInterval, HashSet<string>>();

                    if (!requirements.NodeProcessRequirements[transformation.NodeId].ContainsKey(transformation.Interval))
                        requirements.NodeProcessRequirements[transformation.NodeId][transformation.Interval] = new HashSet<string>();

                    requirements.NodeProcessRequirements[transformation.NodeId][transformation.Interval]
                        .Add($"{transformation.InputType}->{transformation.OutputType}");
                }
                else if (!string.IsNullOrEmpty(transformation.SourceNode))
                {
                    var linkKey = (transformation.SourceNode, transformation.TargetNode);

                    if (!requirements.LinkTransportRequirements.ContainsKey(linkKey))
                        requirements.LinkTransportRequirements[linkKey] = new Dictionary<TimeInterval, HashSet<string>>();

                    if (!requirements.LinkTransportRequirements[linkKey].ContainsKey(transformation.Interval))
                        requirements.LinkTransportRequirements[linkKey][transformation.Interval] = new HashSet<string>();

                    requirements.LinkTransportRequirements[linkKey][transformation.Interval]
                        .Add(transformation.TransportType);
                }
            }

            foreach (var nodeFlow in plan.NodeFlowTypes)
            {
                var nodeId = nodeFlow.Key;
                var flowType = nodeFlow.Value;

                if (!requirements.NodeStorageRequirements.ContainsKey(nodeId))
                    requirements.NodeStorageRequirements[nodeId] = new Dictionary<TimeInterval, Dictionary<string, double>>();

                foreach (var interval in SplitInterval(plan.Path.Interval))
                {
                    if (!requirements.NodeStorageRequirements[nodeId].ContainsKey(interval))
                        requirements.NodeStorageRequirements[nodeId][interval] = new Dictionary<string, double>();

                    if (!requirements.NodeStorageRequirements[nodeId][interval].ContainsKey(flowType))
                        requirements.NodeStorageRequirements[nodeId][interval][flowType] = 0;

                    requirements.NodeStorageRequirements[nodeId][interval][flowType] += plan.Flow.Volume;
                }
            }
        }

        return requirements;
    }

    private List<StructConfiguration> SynthesizeConfigurations(
        ResourceRequirements requirements,
        Dictionary<TimeInterval, StructConfiguration> intervalToBaseConfig,
        FunctionLibrary functionLibrary)
    {
        var synthesizedStructs = new List<StructConfiguration>();
        var processedIntervals = new HashSet<TimeInterval>();

        foreach (var kvp in intervalToBaseConfig)
        {
            var interval = kvp.Key;
            var baseConfig = kvp.Value;

            if (processedIntervals.Contains(interval))
                continue;

            var synthesizedNodes = new List<NodeConfiguration>();
            var synthesizedLinks = new List<LinkConfiguration>();

            foreach (var baseNode in baseConfig.Nodes)
            {
                var synthesizedNode = SynthesizeNodeConfiguration(
                    baseNode,
                    requirements,
                    functionLibrary,
                    interval
                );
                synthesizedNodes.Add(synthesizedNode);
            }

            foreach (var baseLink in baseConfig.Links)
            {
                var synthesizedLink = SynthesizeLinkConfiguration(
                    baseLink,
                    requirements,
                    functionLibrary,
                    interval
                );
                synthesizedLinks.Add(synthesizedLink);
            }

            var synthesizedStruct = new StructConfiguration(
                interval,
                synthesizedNodes,
                synthesizedLinks
            );

            synthesizedStructs.Add(synthesizedStruct);
            processedIntervals.Add(interval);
        }

        synthesizedStructs.Sort((a, b) => a.Interval.Start.CompareTo(b.Interval.Start));
        return synthesizedStructs;
    }

    private NodeConfiguration SynthesizeNodeConfiguration(
        NodeConfiguration baseNode,
        ResourceRequirements requirements,
        FunctionLibrary functionLibrary,
        TimeInterval interval)
    {
        var processes = new List<string>();
        var storageCapacities = new Dictionary<string, double>(baseNode.StorageCapacities);

        if (requirements.NodeProcessRequirements.TryGetValue(baseNode.NodeId, out var intervalProcesses) &&
            intervalProcesses.TryGetValue(interval, out var requiredTransformations))
        {
            foreach (var transformation in requiredTransformations)
            {
                var parts = transformation.Split("->");
                if (parts.Length == 2)
                {
                    var inputType = parts[0];
                    var outputType = parts[1];

                    var matchingProcess = functionLibrary.Processes
                        .FirstOrDefault(p =>
                            p.InputFlowType == inputType &&
                            p.OutputFlowType == outputType &&
                            baseNode.EnabledProcesses.Contains(p.Id));

                    if (matchingProcess != null && !processes.Contains(matchingProcess.Id))
                        processes.Add(matchingProcess.Id);
                }
            }
        }

        if (requirements.NodeStorageRequirements.TryGetValue(baseNode.NodeId, out var intervalStorages) &&
            intervalStorages.TryGetValue(interval, out var requiredStorages))
        {
            foreach (var requiredStorage in requiredStorages)
            {
                var flowType = requiredStorage.Key;
                var requiredVolume = requiredStorage.Value;

                var matchingStorage = functionLibrary.Storages
                    .FirstOrDefault(s => s.AllowedFlowTypes.Contains(flowType));

                if (matchingStorage != null)
                {
                    if (storageCapacities.ContainsKey(matchingStorage.Id))
                    {
                        storageCapacities[matchingStorage.Id] = Math.Max(
                            storageCapacities[matchingStorage.Id],
                            requiredVolume
                        );
                    }
                    else
                    {
                        storageCapacities[matchingStorage.Id] = requiredVolume;
                    }
                }
            }
        }

        // Создаём НОВУЮ иммутабельную конфигурацию (без мутации!)
        return new NodeConfiguration(
            baseNode.NodeId,
            baseNode.EnabledProcesses,
            baseNode.Inputs,
            baseNode.Outputs,
            storageCapacities,
            processes); // ← активные процессы передаются в конструктор
    }

    private LinkConfiguration SynthesizeLinkConfiguration(
        LinkConfiguration baseLink,
        ResourceRequirements requirements,
        FunctionLibrary functionLibrary,
        TimeInterval interval)
    {
        var transports = new List<string>();
        var linkKey = (baseLink.NodeA, baseLink.NodeB);

        if (requirements.LinkTransportRequirements.TryGetValue(linkKey, out var intervalTransports) &&
            intervalTransports.TryGetValue(interval, out var requiredFlowTypes))
        {
            foreach (var flowType in requiredFlowTypes)
            {
                var matchingTransport = functionLibrary.Transports
                    .FirstOrDefault(t =>
                        t.FlowType == flowType &&
                        baseLink.EnabledTransports.Contains(t.Id));

                if (matchingTransport != null && !transports.Contains(matchingTransport.Id))
                    transports.Add(matchingTransport.Id);
            }
        }

        // Создаём НОВУЮ иммутабельную конфигурацию
        return new LinkConfiguration(
            baseLink.NodeA,
            baseLink.NodeB,
            baseLink.EnabledTransports,
            transports); // ← активные транспорты передаются в конструктор
    }

    // Вспомогательные классы остаются как доменные объекты
    private sealed class FlowTransformationStep
    {
        public string? NodeId { get; init; }
        public string? InputType { get; init; }
        public string? OutputType { get; init; }
        public string? SourceNode { get; init; }
        public string? TargetNode { get; init; }
        public string? TransportType { get; init; }
        public required TimeInterval Interval { get; init; }
        public required double FlowVolume { get; init; }
    }

    private sealed class FlowRoutePlan
    {
        public required DataFlow Flow { get; init; }
        public required ReachabilityPath Path { get; init; }
        public required IReadOnlyList<FlowTransformationStep> Transformations { get; init; }
        public required IReadOnlyDictionary<string, string> NodeFlowTypes { get; init; }
    }

    private sealed class ResourceRequirements
    {
        public Dictionary<string, Dictionary<TimeInterval, HashSet<string>>> NodeProcessRequirements { get; init; } = new();
        public Dictionary<(string, string), Dictionary<TimeInterval, HashSet<string>>> LinkTransportRequirements { get; init; } = new();
        public Dictionary<string, Dictionary<TimeInterval, Dictionary<string, double>>> NodeStorageRequirements { get; init; } = new();
    }

    private List<TimeInterval> SplitInterval(TimeInterval interval)
    {
        return new List<TimeInterval> { interval };
    }
}