using DynamicNetwork.Domain.Analysis;
using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Services;

namespace DynamicNetwork.Infrastructure.DomainServices;

public class GraphAnalysisDomainService : IGraphAnalysisDomainService
{
    public GraphAnalysisResult Analyze(TemporalGraph graph)
    {
        var nodes = graph.AllNetworkNodes.ToList();
        var nodeIndexMap = nodes
            .Select((node, index) => new { Node = node, Index = index })
            .ToDictionary(x => x.Node, x => x.Index);

        return new GraphAnalysisResult
        {
            VertexCount = graph.AllNetworkNodes.Count,
            EdgeCount = graph.Links.Count,
            IsConnected = CheckConnectivity(graph),
            HasCycles = CheckForCycles(graph),
            Density = CalculateDensity(graph),
            Diameter = CalculateDiameter(graph),
            UndirectedLinksCount = graph.Links.Where(l => l.Direction == LinkDirection.Undirected).ToList().Count,
            DirectedLinksCount = graph.Links.Where(l => l.Direction != LinkDirection.Undirected).ToList().Count,
            AdjacencyMatrix = BuildAdjacencyMatrix(graph, nodes, nodeIndexMap),
            IncidenceMatrix = BuildIncidenceMatrix(graph, nodes, nodeIndexMap),
            DegreeCentrality = CalculateDegreeCentrality(graph),
            BetweennessCentrality = CalculateBetweennessCentrality(graph),
            StronglyConnectedComponentsCount = CountStronglyConnectedComponents(graph)
        };
    }

    private Matrix<int> BuildAdjacencyMatrix(
        TemporalGraph graph,
        IReadOnlyList<string> nodes,
        Dictionary<string, int> nodeIndexMap)
    {
        var n = nodes.Count;
        var values = new List<IReadOnlyList<int>>();

        for (int i = 0; i < n; i++)
        {
            var row = new int[n];
            for (int j = 0; j < n; j++)
                row[j] = 0;
            values.Add(row.ToList().AsReadOnly());
        }

        foreach (var edge in graph.Links)
        {
            int i = nodeIndexMap[edge.NodeA];
            int j = nodeIndexMap[edge.NodeB];

            if (edge.Direction == LinkDirection.Undirected)
            {
                var newRowI = values[i].ToList();
                newRowI[j] = 1;
                values[i] = newRowI.AsReadOnly();

                var newRowJ = values[j].ToList();
                newRowJ[i] = 1;
                values[j] = newRowJ.AsReadOnly();
            }
            else if (edge.Direction == LinkDirection.Right)
            {
                var newRowI = values[i].ToList();
                newRowI[j] = 1;
                values[i] = newRowI.AsReadOnly();
            }
            else if (edge.Direction == LinkDirection.Left)
            {
                var newRowJ = values[j].ToList();
                newRowJ[i] = 1;
                values[j] = newRowJ.AsReadOnly();
            }
        }

        return new Matrix<int>(nodes, nodes, values);
    }

    private Matrix<int> BuildIncidenceMatrix(
        TemporalGraph graph,
        IReadOnlyList<string> nodes,
        Dictionary<string, int> nodeIndexMap)
    {
        var n = nodes.Count;
        var m = graph.Links.Count;
        var values = new List<IReadOnlyList<int>>();

        for (int i = 0; i < n; i++)
        {
            var row = new int[m];
            for (int j = 0; j < m; j++)
                row[j] = 0;
            values.Add(row.ToList().AsReadOnly());
        }

        int linkIdx = 0;
        foreach (var link in graph.Links)
        {
            int i = nodeIndexMap[link.NodeA];
            int j = nodeIndexMap[link.NodeB];

            if (link.Direction == LinkDirection.Undirected)
            {
                var newRowI = values[i].ToList();
                newRowI[linkIdx] = 1;
                values[i] = newRowI.AsReadOnly();

                var newRowJ = values[j].ToList();
                newRowJ[linkIdx] = 1;
                values[j] = newRowJ.AsReadOnly();
            }
            else if (link.Direction == LinkDirection.Right)
            {
                var newRowI = values[i].ToList();
                newRowI[linkIdx] = -1;
                values[i] = newRowI.AsReadOnly();

                var newRowJ = values[j].ToList();
                newRowJ[linkIdx] = 1;
                values[j] = newRowJ.AsReadOnly();
            }
            else if (link.Direction == LinkDirection.Left)
            {
                var newRowI = values[i].ToList();
                newRowI[linkIdx] = 1;
                values[i] = newRowI.AsReadOnly();

                var newRowJ = values[j].ToList();
                newRowJ[linkIdx] = -1;
                values[j] = newRowJ.AsReadOnly();
            }

            linkIdx++;
        }

        var linkLabels = graph.Links.Select((l, idx) => $"e{idx}").ToList();

        return new Matrix<int>(nodes, linkLabels, values);
    }

    private bool CheckConnectivity(TemporalGraph graph)
    {
        if (!graph.AllNetworkNodes.Any()) return true;

        var visited = new HashSet<string>();
        var stack = new Stack<string>();

        var nodes = graph.AllNetworkNodes.ToList();

        stack.Push(nodes[0]);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current)) continue;

            var connected = graph.Links
                .Where(e => e.NodeA == current || e.NodeB == current)
                .Select(e => e.NodeA == current ? e.NodeB : e.NodeA);

            foreach (var neighbor in connected)
            {
                if (!visited.Contains(neighbor))
                    stack.Push(neighbor);
            }
        }

        return visited.Count == nodes.Count;
    }

    private bool CheckForCycles(TemporalGraph graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var node in graph.AllNetworkNodes)
        {
            if (!visited.Contains(node))
            {
                if (DFSForCycles(node, graph, visited, recursionStack))
                    return true;
            }
        }

        return false;
    }

    private bool DFSForCycles(string node, TemporalGraph graph,
        HashSet<string> visited, HashSet<string> recursionStack)
    {
        visited.Add(node);
        recursionStack.Add(node);

        var outgoing = graph.Links
            .Where(e => e.NodeA == node && e.Direction != LinkDirection.Left)
            .Select(e => e.NodeB)
            .Union(graph.Links
                .Where(e => e.NodeB == node && e.Direction == LinkDirection.Left)
                .Select(e => e.NodeA));

        foreach (var neighbor in outgoing)
        {
            if (!visited.Contains(neighbor))
            {
                if (DFSForCycles(neighbor, graph, visited, recursionStack))
                    return true;
            }
            else if (recursionStack.Contains(neighbor))
            {
                return true;
            }
        }

        recursionStack.Remove(node);
        return false;
    }

    private double CalculateDensity(TemporalGraph graph)
    {
        int n = graph.AllNetworkNodes.Count();
        if (n <= 1) return 0;

        int maxLinks = n * (n - 1);
        if (graph.Links.All(e => e.Direction == LinkDirection.Undirected))
            maxLinks = n * (n - 1) / 2;

        return graph.Links.Count() / (double)maxLinks;
    }

    private int CalculateDiameter(TemporalGraph graph)
    {
        int n = graph.AllNetworkNodes.Count();
        var indexMap = graph.AllNetworkNodes
                            .Select((node, index) => new { Node = node, Index = index })
                            .ToDictionary(x => x.Node, x => x.Index);

        var dist = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                dist[i, j] = (i == j) ? 0 : int.MaxValue / 2;
            }
        }

        foreach (var edge in graph.Links)
        {
            int i = indexMap[edge.NodeA];
            int j = indexMap[edge.NodeB];

            if (edge.Direction == LinkDirection.Undirected || edge.Direction == LinkDirection.Right)
            {
                dist[i, j] = 1;
            }
            if (edge.Direction == LinkDirection.Undirected || edge.Direction == LinkDirection.Left)
            {
                dist[j, i] = 1;
            }
        }

        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (dist[i, k] + dist[k, j] < dist[i, j])
                    {
                        dist[i, j] = dist[i, k] + dist[k, j];
                    }
                }
            }
        }

        int diameter = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (dist[i, j] < int.MaxValue / 2 && dist[i, j] > diameter)
                {
                    diameter = dist[i, j];
                }
            }
        }

        return diameter > 0 ? diameter : default;
    }

    private Dictionary<string, double> CalculateDegreeCentrality(TemporalGraph graph)
    {
        var centrality = new Dictionary<string, double>();
        int n = graph.AllNetworkNodes.Count();

        foreach (var node in graph.AllNetworkNodes)
        {
            int degree = graph.Links.Count(e =>
                (e.NodeA == node || e.NodeB == node) &&
                e.Direction != LinkDirection.Undirected);

            centrality[node] = degree / (double)(n - 1);
        }

        return centrality;
    }

    private Dictionary<string, double> CalculateBetweennessCentrality(TemporalGraph graph)
    {
        var centrality = new Dictionary<string, double>();
        int n = graph.AllNetworkNodes.Count();
        var indexMap = graph.AllNetworkNodes
                            .Select((node, index) => new { Node = node, Index = index })
                            .ToDictionary(x => x.Node, x => x.Index);

        var reverseMap = indexMap.ToDictionary(x => x.Value, x => x.Key);

        for (int i = 0; i < n; i++)
        {
            centrality[reverseMap[i]] = 0;
        }

        for (int s = 0; s < n; s++)
        {
            var distances = new int[n];
            var paths = new int[n];
            var predecessors = new List<string>[n];

            for (int i = 0; i < n; i++)
            {
                distances[i] = -1;
                paths[i] = 0;
                predecessors[i] = new List<string>();
            }

            distances[s] = 0;
            paths[s] = 1;

            var queue = new Queue<int>();
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();

                var neighbors = new List<int>();
                foreach (var edge in graph.Links)
                {
                    int fromIdx = indexMap[edge.NodeA];
                    int toIdx = indexMap[edge.NodeB];

                    if (fromIdx == v && (edge.Direction == LinkDirection.Undirected ||
                        edge.Direction == LinkDirection.Right))
                    {
                        neighbors.Add(toIdx);
                    }
                    if (toIdx == v && (edge.Direction == LinkDirection.Undirected ||
                        edge.Direction == LinkDirection.Left))
                    {
                        neighbors.Add(fromIdx);
                    }
                }

                foreach (var w in neighbors.Distinct())
                {
                    if (distances[w] == -1)
                    {
                        distances[w] = distances[v] + 1;
                        queue.Enqueue(w);
                    }

                    if (distances[w] == distances[v] + 1)
                    {
                        paths[w] += paths[v];
                        predecessors[w].Add(reverseMap[v]);
                    }
                }
            }

            var delta = new double[n];
            var stack = new Stack<int>();

            var AllNetworkNodesByDistance = Enumerable.Range(0, n)
                .Where(i => distances[i] != -1)
                .OrderByDescending(i => distances[i])
                .ToList();

            foreach (var w in AllNetworkNodesByDistance)
            {
                foreach (var pred in predecessors[w])
                {
                    int predIdx = indexMap[pred];
                    delta[predIdx] += (paths[predIdx] / (double)paths[w]) * (1 + delta[w]);
                }

                if (w != s)
                {
                    centrality[reverseMap[w]] += delta[w];
                }
            }
        }

        double factor = 1.0 / ((n - 1) * (n - 2));
        foreach (var key in centrality.Keys.ToList())
        {
            centrality[key] *= factor;
        }

        return centrality;
    }

    private int CountStronglyConnectedComponents(TemporalGraph graph)
    {
        int n = graph.AllNetworkNodes.Count();
        var indexMap = graph.AllNetworkNodes
                            .Select((node, index) => new { Node = node, Index = index })
                            .ToDictionary(x => x.Node, x => x.Index);

        var adj = new List<int>[n];
        var adjTranspose = new List<int>[n];

        for (int i = 0; i < n; i++)
        {
            adj[i] = new List<int>();
            adjTranspose[i] = new List<int>();
        }

        foreach (var edge in graph.Links)
        {
            int fromIdx = indexMap[edge.NodeA];
            int toIdx = indexMap[edge.NodeB];

            if (edge.Direction == LinkDirection.Undirected || edge.Direction == LinkDirection.Right)
            {
                adj[fromIdx].Add(toIdx);
                adjTranspose[toIdx].Add(fromIdx);
            }

            if (edge.Direction == LinkDirection.Undirected || edge.Direction == LinkDirection.Left)
            {
                adj[toIdx].Add(fromIdx);
                adjTranspose[fromIdx].Add(toIdx);
            }
        }

        var visited = new bool[n];
        var finishOrder = new Stack<int>();

        for (int i = 0; i < n; i++)
        {
            if (!visited[i])
            {
                DFS(i, adj, visited, finishOrder);
            }
        }

        visited = new bool[n];
        int components = 0;

        while (finishOrder.Count > 0)
        {
            int v = finishOrder.Pop();
            if (!visited[v])
            {
                DFSTranspose(v, adjTranspose, visited);
                components++;
            }
        }

        return components;
    }

    private void DFS(int v, List<int>[] adj, bool[] visited, Stack<int> finishOrder)
    {
        visited[v] = true;

        foreach (var neighbor in adj[v])
        {
            if (!visited[neighbor])
            {
                DFS(neighbor, adj, visited, finishOrder);
            }
        }

        finishOrder.Push(v);
    }

    private void DFSTranspose(int v, List<int>[] adjTranspose, bool[] visited)
    {
        visited[v] = true;

        foreach (var neighbor in adjTranspose[v])
        {
            if (!visited[neighbor])
            {
                DFSTranspose(neighbor, adjTranspose, visited);
            }
        }
    }

    public List<string> GetInAllNodes(TemporalGraph graph, string node)
    {
        return graph.Links
            .Where(e => e.NodeB == node && e.Direction != LinkDirection.Right)
            .Select(e => e.NodeA)
            .Distinct()
            .ToList();
    }

    public List<string> GetOutAllNodes(TemporalGraph graph, string node)
    {
        return graph.Links
            .Where(e => e.NodeA == node && e.Direction != LinkDirection.Left)
            .Select(e => e.NodeB)
            .Distinct()
            .ToList();
    }

    public List<string> GetAllConnectedAllNodes(TemporalGraph graph, string node)
    {
        return graph.Links
            .Where(e => e.NodeA == node || e.NodeB == node)
            .Select(e => e.NodeA == node ? e.NodeB : e.NodeA)
            .Distinct()
            .ToList();
    }
}
