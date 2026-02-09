using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.Repositories;
using DynamicNetwork.Application.Interfaces.UseCases.Reachability;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using DynamicNetwork.Domain.Services;
using System.Collections.Generic;

namespace DynamicNetwork.Application.UseCases.Reachability;

public class CheckReachabilityUseCase : ICheckReachabilityUseCase
{
    private readonly IStructConfigurationRepository _configRepo;
    private readonly IPathFindingDomainService _pathFinder;

    public CheckReachabilityUseCase(
        IStructConfigurationRepository configRepo,
        IPathFindingDomainService pathFinder)
    {
        _configRepo = configRepo;
        _pathFinder = pathFinder;
    }

    public ReachabilityResult Execute(
        IReadOnlyList<TemporalGraph> graphs,
        ReachabilityRequest request)
    {
        var result = new ReachabilityResult();

        var validGraphs = graphs
            .Where(g => g.Interval.Overlaps(request.CustomInterval))
            .OrderBy(g => g.Interval.Start)
            .ToList();

        if (!validGraphs.Any())
        {
            result.Message = "Нет графов в заданном интервале времени";
            return result;
        }

        var allNodes = validGraphs
            .SelectMany(g => g.AllNetworkNodes)
            .Distinct();

        if (!allNodes.Contains(request.SourceNode))
        {
            result.Message = $"Исходный узел '{request.SourceNode}' не найден";
            return result;
        }

        var missingTargets = request.TargetNodes
            .Where(t => !allNodes.Contains(t))
            .ToList();

        if (missingTargets.Any())
        {
            result.Message = $"Следующие целевые узлы не найдены: {string.Join(", ", missingTargets)}";
            return result;
        }

        var configs = validGraphs
            .Select(g => _configRepo.GetByInterval(g.Interval))
            .Where(c => c != null)
            .ToList();

        var allDomainPaths = new List<ReachabilityPath>();
        foreach (var target in request.TargetNodes)
        {
            var paths = _pathFinder.FindAllPaths(
                validGraphs, configs!, request.SourceNode, target, request.CustomInterval);
            allDomainPaths.AddRange(paths);
        }

        result.AllPaths = allDomainPaths.Select(MapToDto).ToList();
        result.IsReachable = result.AllPaths.Any();
        result.ShortestPathLength = result.AllPaths.Any()
            ? result.AllPaths.Min(p => p.NodeCount - 1)
            : null;

        result.Message = result.IsReachable
            ? $"Найдено {result.AllPaths.Count} путей. Кратчайший путь: {result.ShortestPathLength} шагов"
            : "Пути не найдены";

        return result;
    }

    private ReachabilityPathDto MapToDto(ReachabilityPath path)
    {
        return new ReachabilityPathDto
        {
            Path = path.Nodes.ToList(),
            GraphIndices = path.GraphIndices.ToList(),
            Edges = path.Edges.Select(e => new EdgeInfoDto
            {
                FromNode = e.FromNode,
                ToNode = e.ToNode,
                GraphIndex = e.GraphIndex
            }).ToList(),
            NodeCount = path.Nodes.Count,
            Interval = path.Interval
        };
    }
}
