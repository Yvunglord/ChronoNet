using ChronoNet.Application.DTO;
using ChronoNet.Domain;
using ChronoNet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Application.Services
{
    public static class ReachabilityService
    {
        public static ReachabilityResult CalculateReachability(
            IList<TemporalGraph> allGraphs,
            ReachabilityRequest request,
            Dictionary<string, Device> deviceMap)
        {
            var result = new ReachabilityResult();

            var intersectingGraphs = allGraphs
                .Where(g => g.Interval.Overlaps(request.CustomInterval))
                .OrderBy(g => g.Interval.Start)
                .ToList();

            if (!intersectingGraphs.Any())
            {
                result.Message = "Нет графов в заданном интервале времени";
                return result;
            }

            if (!deviceMap.TryGetValue(request.SourceDeviceName, out var sourceDevice))
            {
                result.Message = $"Исходное устройство '{request.SourceDeviceName}' не найдено";
                return result;
            }

            var targetDevices = request.TargetDeviceNames
                .Select(name => deviceMap.TryGetValue(name, out var device) ? device : null)
                .Where(d => d != null)
                .ToList();

            if (!targetDevices.Any())
            {
                result.Message = "Целевые устройства не найдены";
                return result;
            }

            if (!request.ConsiderCapabilities)
            {
                return CalculateSimpleReachability(intersectingGraphs, sourceDevice, targetDevices);
            }
            else
            {
                return CalculateCapabilityBasedReachability(
                    intersectingGraphs, sourceDevice, targetDevices, request.DataSize);
            }
        }

        private static ReachabilityResult CalculateSimpleReachability(
            List<TemporalGraph> graphs,
            Device source,
            List<Device> targets)
        {
            var result = new ReachabilityResult();
            var allPaths = new List<PathWithInterval>();

            foreach (var target in targets)
            {
                var paths = FindPathsThroughGraphs(graphs, source.Id, target.Id);
                allPaths.AddRange(paths);
            }

            result.AllPaths = allPaths;
            result.IsReachable = result.AllPaths.Any();
            result.ShortestPathLength = result.AllPaths.Any()
                ? result.AllPaths.Min(p => p.Path.Count - 1)
                : null;
            result.Message = result.IsReachable
                ? $"Найдено {result.AllPaths.Count} путей. Кратчайший путь: {result.ShortestPathLength} шагов"
                : "Пути не найдены";

            return result;
        }

        private static ReachabilityResult CalculateCapabilityBasedReachability(
            List<TemporalGraph> graphs,
            Device source,
            List<Device> targets,
            long? dataSize)
        {
            var result = new ReachabilityResult();
            var capabilityIssues = new Dictionary<Guid, string>();
            var validPaths = new List<PathWithInterval>();

            foreach (var target in targets)
            {
                var paths = FindPathsThroughGraphs(graphs, source.Id, target.Id);

                foreach (var path in paths)
                {
                    if (CheckPathCapabilitiesThroughGraphs(graphs, path.Path, dataSize, out var issues))
                    {
                        validPaths.Add(path);
                    }
                    else
                    {
                        foreach (var issue in issues)
                        {
                            capabilityIssues[issue.Key] = issue.Value;
                        }
                    }
                }
            }

            result.AllPaths = validPaths;
            result.IsReachable = result.AllPaths.Any();
            result.ShortestPathLength = result.AllPaths.Any()
                ? result.AllPaths.Min(p => p.Path.Count - 1)
                : null;
            result.CapabilityIssues = capabilityIssues;

            result.Message = result.IsReachable
                ? $"Найдено {result.AllPaths.Count} валидных путей с учетом возможностей. Кратчайший: {result.ShortestPathLength} шагов"
                : "Нет валидных путей с учетом возможностей устройств";

            return result;
        }

        private static bool CheckPathCapabilitiesThroughGraphs(
            List<TemporalGraph> graphs,
            List<Guid> path,
            long? dataSize,
            out Dictionary<Guid, string> issues)
        {
            issues = new Dictionary<Guid, string>();

            if (path == null || path.Count == 0)
                return false;

            var deviceGraphs = new Dictionary<Guid, List<TemporalGraph>>();

            foreach (var deviceId in path)
            {
                var graphsWithDevice = graphs
                    .Where(g => g.Vertices.Any(v => v.Id == deviceId))
                    .ToList();

                if (!graphsWithDevice.Any())
                {
                    issues[deviceId] = $"Устройство не найдено ни в одном графе";
                    return false;
                }

                deviceGraphs[deviceId] = graphsWithDevice;
            }

            for (int i = 0; i < path.Count; i++)
            {
                var deviceId = path[i];
                var deviceGraphsList = deviceGraphs[deviceId];

                foreach (var graph in deviceGraphsList)
                {
                    var device = graph.Vertices.FirstOrDefault(v => v.Id == deviceId);
                    if (device == null) continue;

                    if (i == 0)
                    {
                        if (!device.HasCapability(GlobalCapabilities.Transfer))
                        {
                            issues[deviceId] = "Источник не имеет возможности передачи";
                            return false;
                        }
                    }
                    else if (i == path.Count - 1)
                    {
                        if (!device.HasCapability(GlobalCapabilities.Storage))
                        {
                            issues[deviceId] = "Цель не имеет возможности хранения";
                            return false;
                        }
                    }
                    else
                    {
                        if (!device.HasCapability(GlobalCapabilities.Transfer))
                        {
                            issues[deviceId] = "Промежуточный узел не имеет возможности передачи";
                            return false;
                        }
                    }

                    var localCapabilities = graph.GetLocalCapabilities(deviceId);

                    if (i == 0)
                    {
                        if ((localCapabilities & LocalCapabilities.CanSend) == 0)
                        {
                            issues[deviceId] = "Источник не может отправлять данные в данный момент времени";
                            return false;
                        }
                    }
                    else if (i == path.Count - 1)
                    {
                        if ((localCapabilities & LocalCapabilities.CanReceive) == 0)
                        {
                            issues[deviceId] = "Цель не может принимать данные в данный момент времени";
                            return false;
                        }
                    }
                    else
                    {
                        if ((localCapabilities & (LocalCapabilities.CanReceive | LocalCapabilities.CanSend))
                            != (LocalCapabilities.CanReceive | LocalCapabilities.CanSend))
                        {
                            issues[deviceId] = "Промежуточный узел не может передавать данные в данный момент времени";
                            return false;
                        }
                    }

                    if (dataSize.HasValue && i == path.Count - 1 && device.HasCapability(GlobalCapabilities.Storage))
                    {
                        if ((localCapabilities & LocalCapabilities.Storage) == 0)
                        {
                            issues[deviceId] = "Хранилище переполнено";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static List<List<Guid>> FindAllPaths(
            TemporalGraph graph,
            Guid source,
            Guid target,
            HashSet<Guid> visited,
            List<Guid>? currentPath = null)
        {
            var paths = new List<List<Guid>>();
            currentPath ??= new List<Guid>();

            if (visited.Contains(source))
                return paths;

            visited.Add(source);
            currentPath.Add(source);

            if (source == target)
            {
                paths.Add(new List<Guid>(currentPath));
            }
            else
            {
                var neighbors = graph.Edges
                    .Where(e => e.From == source &&
                           (e.Direction == EdgeDirection.Right || e.Direction == EdgeDirection.Undirected))
                    .Select(e => e.To)
                    .Union(graph.Edges
                        .Where(e => e.To == source &&
                               (e.Direction == EdgeDirection.Left || e.Direction == EdgeDirection.Undirected))
                        .Select(e => e.From))
                    .Distinct();

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        var newPaths = FindAllPaths(graph, neighbor, target,
                            new HashSet<Guid>(visited), new List<Guid>(currentPath));
                        paths.AddRange(newPaths);
                    }
                }
            }

            return paths;
        }

        private static List<PathWithInterval> FindPathsThroughGraphs(
            List<TemporalGraph> graphs,
            Guid source,
            Guid target)
        {
            var allPaths = new List<PathWithInterval>();

            for (int startGraphIndex = 0; startGraphIndex < graphs.Count; startGraphIndex++)
            {
                var startGraph = graphs[startGraphIndex];

                if (!startGraph.Vertices.Any(v => v.Id == source))
                    continue;

                var pathsFromStart = FindPathsFromGraph(
                    graphs, startGraphIndex, source, target,
                    new HashSet<Guid>(), new List<Guid>());

                allPaths.AddRange(pathsFromStart);
            }

            return allPaths;
        }

        private static List<PathWithInterval> FindPathsFromGraph(
            List<TemporalGraph> graphs,
            int currentGraphIndex,
            Guid currentNode,
            Guid target,
            HashSet<Guid> visitedNodes,
            List<Guid> currentPath)
        {
            var paths = new List<PathWithInterval>();
            var currentGraph = graphs[currentGraphIndex];

            var newPath = new List<Guid>(currentPath) { currentNode };
            var newVisited = new HashSet<Guid>(visitedNodes) { currentNode };

            if (currentNode == target)
            {
                int minGraphIndex = currentGraphIndex;
                int maxGraphIndex = currentGraphIndex;

                var interval = currentGraph.Interval;

                paths.Add(new PathWithInterval
                {
                    Path = newPath,
                    Interval = interval
                });
                return paths;
            }

            var neighbors = GetNeighborsInGraph(currentGraph, currentNode);

            foreach (var neighbor in neighbors)
            {
                if (!newVisited.Contains(neighbor))
                {
                    var neighborPaths = FindPathsFromGraph(
                        graphs, currentGraphIndex, neighbor, target,
                        newVisited, newPath);
                    paths.AddRange(neighborPaths);
                }
            }

            if (currentGraphIndex + 1 < graphs.Count)
            {
                var nextGraph = graphs[currentGraphIndex + 1];

                if (nextGraph.Vertices.Any(v => v.Id == currentNode))
                {
                    var pathsInNextGraph = FindPathsFromGraph(
                        graphs, currentGraphIndex + 1, currentNode, target,
                        newVisited, newPath);
                    paths.AddRange(pathsInNextGraph);
                }
            }

            return paths;
        }

        private static List<Guid> GetNeighborsInGraph(TemporalGraph graph, Guid nodeId)
        {
            var neighbors = new List<Guid>();

            var outgoing = graph.Edges
                .Where(e => e.From == nodeId &&
                       (e.Direction == EdgeDirection.Right || e.Direction == EdgeDirection.Undirected))
                .Select(e => e.To);

            var incoming = graph.Edges
                .Where(e => e.To == nodeId &&
                       (e.Direction == EdgeDirection.Left || e.Direction == EdgeDirection.Undirected))
                .Select(e => e.From);

            return outgoing.Union(incoming).Distinct().ToList();
        }

        private class PathComparer : IEqualityComparer<List<Guid>>
        {
            public bool Equals(List<Guid>? x, List<Guid>? y)
            {
                if (x == null || y == null) return false;
                if (x.Count != y.Count) return false;

                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i] != y[i]) return false;
                }

                return true;
            }

            public int GetHashCode(List<Guid> obj)
            {
                int hash = 17;
                foreach (var id in obj)
                {
                    hash = hash * 23 + id.GetHashCode();
                }
                return hash;
            }
        }
    }
}
