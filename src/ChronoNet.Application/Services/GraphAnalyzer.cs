using ChronoNet.Domain;
using ChronoNet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronoNet.Application.Services
{
    public static class GraphAnalyzer
    {
        public static GraphAnalysisResult Analyze(TemporalGraph graph)
        {
            var result = new GraphAnalysisResult
            {
                VertexCount = graph.Vertices.Count,
                EdgeCount = graph.Edges.Count,
                DirectedEdgesCount = graph.Edges.Count(e => e.Direction != EdgeDirection.Undirected),
                UndirectedEdgesCount = graph.Edges.Count(e => e.Direction == EdgeDirection.Undirected),
                VertexNames = graph.Vertices.Select(v => v.Name).ToList(),
                EdgeNames = graph.Edges.Select(e => e.ToString()).ToList()
            };

            result.AdjacencyMatrix = BuildAdjacencyMatrix(graph);
            result.IncidenceMatrix = BuildIncidenceMatrix(graph);
            result.IsConnected = CheckConnectivity(graph);
            result.HasCycles = CheckForCycles(graph);
            result.Density = CalculateDensity(graph);
            result.Diameter = CalculateDiameter(graph);
            result.DegreeCentrality = CalculateDegreeCentrality(graph);
            result.BetweennessCentrality = CalculateBetweennessCentrality(graph);
            result.StronglyConnectedComponentsCount = CountStronglyConnectedComponents(graph);

            return result;
        }

        private static int[,] BuildAdjacencyMatrix(TemporalGraph graph)
        {
            int n = graph.Vertices.Count;
            var matrix = new int[n, n];
            var indexMap = graph.Vertices.Select((v, i) => new { v.Id, i })
                                        .ToDictionary(x => x.Id, x => x.i);

            foreach (var edge in graph.Edges)
            {
                int i = indexMap[edge.From];
                int j = indexMap[edge.To];

                if (edge.Direction == EdgeDirection.Undirected)
                {
                    matrix[i, j] = 1;
                    matrix[j, i] = 1;
                }
                else if (edge.Direction == EdgeDirection.Right)
                {
                    matrix[i, j] = 1;
                }
                else if (edge.Direction == EdgeDirection.Left)
                {
                    matrix[j, i] = 1;
                }
            }

            return matrix;
        }

        private static int[,] BuildIncidenceMatrix(TemporalGraph graph)
        {
            int n = graph.Vertices.Count;
            int m = graph.Edges.Count;
            var matrix = new int[n, m];
            var vertexIndex = graph.Vertices.Select((v, i) => new { v.Id, i })
                                           .ToDictionary(x => x.Id, x => x.i);

            for (int edgeIdx = 0; edgeIdx < m; edgeIdx++)
            {
                var edge = graph.Edges[edgeIdx];
                int i = vertexIndex[edge.From];
                int j = vertexIndex[edge.To];

                if (edge.Direction == EdgeDirection.Undirected)
                {
                    matrix[i, edgeIdx] = 1;
                    matrix[j, edgeIdx] = 1;
                }
                else if (edge.Direction == EdgeDirection.Right)
                {
                    matrix[i, edgeIdx] = -1;
                    matrix[j, edgeIdx] = 1;
                }
                else if (edge.Direction == EdgeDirection.Left)
                {
                    matrix[i, edgeIdx] = 1;
                    matrix[j, edgeIdx] = -1;
                }
            }

            return matrix;
        }

        private static bool CheckConnectivity(TemporalGraph graph)
        {
            if (!graph.Vertices.Any()) return true;

            var visited = new HashSet<Guid>();
            var stack = new Stack<Guid>();

            stack.Push(graph.Vertices[0].Id);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current)) continue;

                var connected = graph.Edges
                    .Where(e => e.From == current || e.To == current)
                    .Select(e => e.From == current ? e.To : e.From);

                foreach (var neighbor in connected)
                {
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }

            return visited.Count == graph.Vertices.Count;
        }

        private static bool CheckForCycles(TemporalGraph graph)
        {
            var visited = new HashSet<Guid>();
            var recursionStack = new HashSet<Guid>();

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex.Id))
                {
                    if (DFSForCycles(vertex.Id, graph, visited, recursionStack))
                        return true;
                }
            }

            return false;
        }

        private static bool DFSForCycles(Guid vertexId, TemporalGraph graph,
            HashSet<Guid> visited, HashSet<Guid> recursionStack)
        {
            visited.Add(vertexId);
            recursionStack.Add(vertexId);

            var outgoing = graph.Edges
                .Where(e => e.From == vertexId && e.Direction != EdgeDirection.Left)
                .Select(e => e.To)
                .Union(graph.Edges
                    .Where(e => e.To == vertexId && e.Direction == EdgeDirection.Left)
                    .Select(e => e.From));

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

            recursionStack.Remove(vertexId);
            return false;
        }

        private static double CalculateDensity(TemporalGraph graph)
        {
            int n = graph.Vertices.Count;
            if (n <= 1) return 0;

            int maxEdges = n * (n - 1);
            if (graph.Edges.All(e => e.Direction == EdgeDirection.Undirected))
                maxEdges = n * (n - 1) / 2;

            return graph.Edges.Count / (double)maxEdges;
        }

        private static int CalculateDiameter(TemporalGraph graph)
        {
            int n = graph.Vertices.Count;
            var indexMap = graph.Vertices.Select((v, i) => new { v.Id, i })
                                        .ToDictionary(x => x.Id, x => x.i);

            var dist = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = (i == j) ? 0 : int.MaxValue / 2;
                }
            }

            foreach (var edge in graph.Edges)
            {
                int i = indexMap[edge.From];
                int j = indexMap[edge.To];

                if (edge.Direction == EdgeDirection.Undirected || edge.Direction == EdgeDirection.Right)
                {
                    dist[i, j] = 1;
                }
                if (edge.Direction == EdgeDirection.Undirected || edge.Direction == EdgeDirection.Left)
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

        private static Dictionary<Guid, double> CalculateDegreeCentrality(TemporalGraph graph)
        {
            var centrality = new Dictionary<Guid, double>();
            int n = graph.Vertices.Count;

            foreach (var vertex in graph.Vertices)
            {
                int degree = graph.Edges.Count(e =>
                    (e.From == vertex.Id || e.To == vertex.Id) &&
                    e.Direction != EdgeDirection.Undirected);

                centrality[vertex.Id] = degree / (double)(n - 1);
            }

            return centrality;
        }

        private static Dictionary<Guid, double> CalculateBetweennessCentrality(TemporalGraph graph)
        {
            var centrality = new Dictionary<Guid, double>();
            int n = graph.Vertices.Count;
            var indexMap = graph.Vertices.Select((v, i) => new { v.Id, i })
                                        .ToDictionary(x => x.Id, x => x.i);
            var reverseMap = indexMap.ToDictionary(x => x.Value, x => x.Key);

            for (int i = 0; i < n; i++)
            {
                centrality[reverseMap[i]] = 0;
            }

            for (int s = 0; s < n; s++)
            {
                var distances = new int[n];
                var paths = new int[n];
                var predecessors = new List<Guid>[n];

                for (int i = 0; i < n; i++)
                {
                    distances[i] = -1;
                    paths[i] = 0;
                    predecessors[i] = new List<Guid>();
                }

                distances[s] = 0;
                paths[s] = 1;

                var queue = new Queue<int>();
                queue.Enqueue(s);

                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();

                    var neighbors = new List<int>();
                    foreach (var edge in graph.Edges)
                    {
                        int fromIdx = indexMap[edge.From];
                        int toIdx = indexMap[edge.To];

                        if (fromIdx == v && (edge.Direction == EdgeDirection.Undirected ||
                            edge.Direction == EdgeDirection.Right))
                        {
                            neighbors.Add(toIdx);
                        }
                        if (toIdx == v && (edge.Direction == EdgeDirection.Undirected ||
                            edge.Direction == EdgeDirection.Left))
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

                var verticesByDistance = Enumerable.Range(0, n)
                    .Where(i => distances[i] != -1)
                    .OrderByDescending(i => distances[i])
                    .ToList();

                foreach (var w in verticesByDistance)
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

        private static int CountStronglyConnectedComponents(TemporalGraph graph)
        {
            int n = graph.Vertices.Count;
            var indexMap = graph.Vertices.Select((v, i) => new { v.Id, i })
                                         .ToDictionary(x => x.Id, x => x.i);

            var adj = new List<int>[n];
            var adjTranspose = new List<int>[n];

            for (int i = 0; i < n; i++)
            {
                adj[i] = new List<int>();
                adjTranspose[i] = new List<int>();
            }

            foreach (var edge in graph.Edges)
            {
                int fromIdx = indexMap[edge.From];
                int toIdx = indexMap[edge.To];

                if (edge.Direction == EdgeDirection.Undirected || edge.Direction == EdgeDirection.Right)
                {
                    adj[fromIdx].Add(toIdx);
                    adjTranspose[toIdx].Add(fromIdx);
                }

                if (edge.Direction == EdgeDirection.Undirected || edge.Direction == EdgeDirection.Left)
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

        private static void DFS(int v, List<int>[] adj, bool[] visited, Stack<int> finishOrder)
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

        private static void DFSTranspose(int v, List<int>[] adjTranspose, bool[] visited)
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

        public static List<Guid> GetInVertices(TemporalGraph graph, Guid vertexId)
        {
            return graph.Edges
                .Where(e => e.To == vertexId && e.Direction != EdgeDirection.Right)
                .Select(e => e.From)
                .Distinct()
                .ToList();
        }

        public static List<Guid> GetOutVertices(TemporalGraph graph, Guid vertexId)
        {
            return graph.Edges
                .Where(e => e.From == vertexId && e.Direction != EdgeDirection.Left)
                .Select(e => e.To)
                .Distinct()
                .ToList();
        }

        public static List<Guid> GetAllConnectedVertices(TemporalGraph graph, Guid vertexId)
        {
            return graph.Edges
                .Where(e => e.From == vertexId || e.To == vertexId)
                .Select(e => e.From == vertexId ? e.To : e.From)
                .Distinct()
                .ToList();
        }
    }
}
