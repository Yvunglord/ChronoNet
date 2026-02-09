using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases.Configuration;
using DynamicNetwork.Application.Interfaces.UseCases.Reachability;
using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using DynamicNetwork.Domain.Services;
using DynamicNetwork.Domain.Synthesis;

namespace DynamicNetwork.Application.UseCases.Configuration;

public class SynthesizeConfigurationUseCase : ISynthesizeConfigurationUseCase
{
    private readonly ICheckReachabilityUseCase _reachabilityUseCase;
    private readonly ISynthesisDomainService _synthesisService;
    private readonly IFunctionLibraryProvider _libraryProvider;
    private readonly IStructConfigurationRepository _configRepo;

    public SynthesizeConfigurationUseCase(
        ICheckReachabilityUseCase reachabilityUseCase,
        ISynthesisDomainService synthesisService,
        IFunctionLibraryProvider libraryProvider,
        IStructConfigurationRepository configRepo)
    {
        _reachabilityUseCase = reachabilityUseCase;
        _synthesisService = synthesisService;
        _libraryProvider = libraryProvider;
        _configRepo = configRepo;
    }

    public IReadOnlyList<StructConfiguration> Execute(
        StructConfigurationRequestDto requestDto,
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<DataFlow> flows)
    {
        ValidateRequest(requestDto, flows);

        var library = _libraryProvider.GetCurrent();
        var baseConfigurations = _configRepo.GetAll();

        var reachabilityRequest = new ReachabilityRequest
        {
            SourceNode = requestDto.NodeInputs.Keys.First().NodeId,
            TargetNodes = requestDto.OutputNodes.Select(nc => nc.NodeId).ToList(),
            CustomInterval = requestDto.CustomInterval
        };

        var reachabilityResult = _reachabilityUseCase.Execute(graphs, reachabilityRequest);

        if (!reachabilityResult.IsReachable || !reachabilityResult.AllPaths.Any())
            throw new InvalidOperationException(
                $"No reachable paths found between sources and targets in interval {requestDto.CustomInterval}");

        var domainRequest = new StructConfigurationSynthesisRequest
        {
            NodeInputs = requestDto.NodeInputs,
            OutputNodes = requestDto.OutputNodes,
            CustomInterval = requestDto.CustomInterval
        };

        var domainPaths = reachabilityResult.AllPaths
            .Select(dto =>
            new ReachabilityPath(
                dto.Path.AsReadOnly(),
                dto.GraphIndices.AsReadOnly(),
                dto.Edges.Select(e => 
                    new EdgeTraversal 
                    {
                        FromNode = e.FromNode,
                        ToNode = e.ToNode,
                        GraphIndex = e.GraphIndex,
                        Link = new Link(e.FromNode, e.ToNode) 
                    })
                .ToList()
                .AsReadOnly(),
                dto.Interval)
            )
            .ToList();

        var synthesizedConfigs = _synthesisService.SynthesizeAll(
            domainRequest,
            graphs,
            flows,
            library,
            baseConfigurations.ToList(),
            domainPaths);

        foreach (var config in synthesizedConfigs)
        {
            if (_configRepo.Exists(config.Interval))
                _configRepo.Update(config);
            else
                _configRepo.Add(config);
        }

        return synthesizedConfigs;
    }

    private void ValidateRequest(StructConfigurationRequestDto request, IReadOnlyList<DataFlow> flows)
    {
        if (request.NodeInputs == null || !request.NodeInputs.Any())
            throw new ArgumentException("At least one input node must be specified");
        if (request.OutputNodes == null || !request.OutputNodes.Any())
            throw new ArgumentException("At least one output node must be specified");
        if (flows == null || !flows.Any())
            throw new ArgumentException("At least one data flow must be specified");
    }
}