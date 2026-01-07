using ChronoNet.Domain;
using ChronoNet.Domain.Enums;
using Microsoft.Msagl.Drawing;

namespace ChronoNet.Infrastructure.Visualization;
public class MsaglGraphAdapter
{
    public Graph Build(TemporalGraph temporalGraph)
    {
        var graph = new Graph
        {
            Attr = { BackgroundColor = Color.White }
        };

        foreach (var v in temporalGraph.Vertices)
        {
            var node = graph.AddNode(v.Id.ToString());
            node.LabelText = v.Name;
            node.Attr.Shape = Shape.Circle;
        }

        foreach (var e in temporalGraph.Edges)
        {
            var edge = graph.AddEdge(
                e.From.ToString(),
                e.To.ToString());
            
            if (e.Direction == Domain.Enums.EdgeDirection.Right)
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
            else if (e.Direction == Domain.Enums.EdgeDirection.Left)
            {
                edge.Attr.ArrowheadAtSource = ArrowStyle.Normal;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;

            }
            else
            {
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
            }
        }

        return graph;
    }
}