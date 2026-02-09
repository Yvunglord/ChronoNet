using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.UseCases.Configuration;
using DynamicNetwork.Application.UseCases.Reachability;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Infrastructure.DomainServices;
using DynamicNetwork.Infrastructure.Persistence.Providers;
using DynamicNetwork.Infrastructure.Persistence.Repositories;
using System.Collections.Generic;

namespace DynamicNetwork.SynthesisTester;

public class SynthesisRunner
{
    public SynthesisResult RunSynthesis(TestScenario scenario)
    {
        var libraryProvider = new InMemoryFunctionLibraryProvider(scenario.Library);
        var configRepo = new InMemoryStructConfigurationRepository();
        var flowRepo = new InMemoryDataFlowRepository();

        foreach (var flow in scenario.Flows)
        {
            flowRepo.Add(flow);
        }

        foreach (var graph in scenario.Graphs)
        {
            var nodeConfigs = new List<NodeConfiguration>();
            var linkConfigs = new List<LinkConfiguration>();

            foreach (var nodeId in graph.AllNetworkNodes)
            {
                nodeConfigs.Add(new NodeConfiguration(
                    nodeId,
                    new[] { "proc1", "proc2" },
                    new string[] { },
                    new string[] { },
                    new Dictionary<string, double>(),
                    new string[] { }));
            }

            foreach (var link in graph.Links)
            {
                linkConfigs.Add(new LinkConfiguration(
                    link.NodeA,
                    link.NodeB,
                    new[] { "t1", "t2", "t3" },
                    new string[] { }));
            }

            var baseConfig = new StructConfiguration(
                graph.Interval,
                nodeConfigs,
                linkConfigs);

            configRepo.Add(baseConfig);
        }

        var pathFinder = new PathFindingDomainService();
        var synthesisService = new SynthesisDomainService();

        var reachabilityUseCase = new CheckReachabilityUseCase(
            configRepo,
            pathFinder);

        var synthesizeUseCase = new SynthesizeConfigurationUseCase(
            reachabilityUseCase,
            synthesisService,
            libraryProvider,
            configRepo);

        var request = new StructConfigurationRequestDto
        {
            NodeInputs = scenario.NodeInputs,
            OutputNodes = scenario.OutputNodes,
            CustomInterval = scenario.CustomInterval
        };

        var configs = synthesizeUseCase.Execute(request, scenario.Graphs, scenario.Flows);

        return new SynthesisResult
        {
            Configurations = configs,
            InputNodes = scenario.NodeInputs.Keys,
            OutputNodes = scenario.OutputNodes,
            TimeInterval = scenario.CustomInterval,
            FlowCount = scenario.Flows.Count
        };
    }
}

public class SynthesisResult
{
    public IReadOnlyList<StructConfiguration> Configurations { get; set; } = new List<StructConfiguration>();
    public IEnumerable<NodeConfiguration> InputNodes { get; set; } = new List<NodeConfiguration>();
    public IEnumerable<NodeConfiguration> OutputNodes { get; set; } = new List<NodeConfiguration>();
    public TimeInterval TimeInterval { get; set; }
    public int FlowCount { get; set; }
}