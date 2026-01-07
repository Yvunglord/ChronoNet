using ChronoNet.Domain;
using ChronoNet.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChronoNet.Application.Services
{
    public class GraphAnalysisResult
    {
        public int VertexCount { get; set; }
        public int EdgeCount { get; set; }
        public int DirectedEdgesCount { get; set; }
        public int UndirectedEdgesCount { get; set; }
        public bool IsConnected { get; set; }
        public bool HasCycles { get; set; }
        public double Density { get; set; }
        public int? Diameter { get; set; }
        public Dictionary<Guid, double> DegreeCentrality { get; set; } = new();
        public Dictionary<Guid, double> BetweennessCentrality { get; set; } = new();
        public int[,] AdjacencyMatrix { get; set; } = new int[0, 0];
        public int[,] IncidenceMatrix { get; set; } = new int[0, 0];
        public List<string> VertexNames { get; set; } = new List<string>();
        public List<string> EdgeNames { get; set; } = new List<string>();
        public int StronglyConnectedComponentsCount { get; set; }

        public List<MatrixRow> AdjacencyMatrixWithHeaders
        {
            get
            {
                var rows = new List<MatrixRow>();
                for (int i = 0; i < AdjacencyMatrix.GetLength(0); i++)
                {
                    var cells = new List<MatrixCell>();
                    for (int j = 0; j < AdjacencyMatrix.GetLength(1); j++)
                    {
                        cells.Add(new MatrixCell { Value = AdjacencyMatrix[i, j].ToString() });
                    }
                    rows.Add(new MatrixRow
                    {
                        RowHeader = VertexNames.Count > i ? VertexNames[i] : $"V{i + 1}",
                        Cells = cells
                    });
                }
                return rows;
            }
        }

        public List<MatrixRow> IncidenceMatrixWithHeaders
        {
            get
            {
                var rows = new List<MatrixRow>();
                for (int i = 0; i < IncidenceMatrix.GetLength(0); i++)
                {
                    var cells = new List<MatrixCell>();
                    for (int j = 0; j < IncidenceMatrix.GetLength(1); j++)
                    {
                        cells.Add(new MatrixCell { Value = IncidenceMatrix[i, j].ToString() });
                    }
                    rows.Add(new MatrixRow
                    {
                        RowHeader = VertexNames.Count > i ? VertexNames[i] : $"V{i + 1}",
                        Cells = cells
                    });
                }
                return rows;
            }
        }

        public List<string> AdjacencyColumnHeaders
        {
            get
            {
                var headers = new List<string>();
                for (int i = 0; i < AdjacencyMatrix.GetLength(1); i++)
                {
                    headers.Add(VertexNames.Count > i ? VertexNames[i] : $"V{i + 1}");
                }
                return headers;
            }
        }

        public List<string> IncidenceColumnHeaders
        {
            get
            {
                var headers = new List<string>();
                for (int i = 0; i < IncidenceMatrix.GetLength(1); i++)
                {
                    headers.Add(EdgeNames.Count > i ? EdgeNames[i] : $"E{i + 1}");
                }
                return headers;
            }
        }
    }

    public class MatrixRow
    {
        public string RowHeader { get; set; } = "";
        public List<MatrixCell> Cells { get; set; } = new List<MatrixCell>();
    }

    public class MatrixCell
    {
        public string Value { get; set; } = "";
    }
}