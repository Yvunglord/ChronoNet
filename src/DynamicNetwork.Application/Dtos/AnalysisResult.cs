namespace DynamicNetwork.Application.Dtos;

public sealed class AnalysisResult
{
    public int VertexCount { get; set; }
    public int EdgeCount { get; set; }
    public bool IsConnected { get; set; }
    public bool HasCycles { get; set; }
    public double Density { get; set; }
    public int Diameter { get; set; }
    public int UndirectedLinksCount { get; set; }
    public int DirectedLinksCount { get; set; }
    public int StronglyConnectedComponentsCount { get; set; }

    public List<MatrixRowDto> AdjacencyMatrixRows { get; set; } = new();
    public List<MatrixRowDto> IncidenceMatrixRows { get; set; } = new();

    public List<string> AdjacencyColumnHeaders { get; set; } = new();
    public List<string> IncidenceColumnHeaders { get; set; } = new();

    public Dictionary<string, double> DegreeCentrality { get; set; } = new();
    public Dictionary<string, double> BetweennessCentrality { get; set; } = new();

    public string? Message { get; set; }
}