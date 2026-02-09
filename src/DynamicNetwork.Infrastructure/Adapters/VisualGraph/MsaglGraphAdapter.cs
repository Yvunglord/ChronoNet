using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Graph;
using Microsoft.Msagl.Drawing;

namespace DynamicNetwork.Infrastructure.Adapters.VisualGraph;

public class MsaglGraphAdapter
{
    public Graph Build(TemporalGraph temporalGraph)
    {
        var graph = new Graph
        {
            Attr = { BackgroundColor = Color.White }
        };

        foreach (var n in temporalGraph.AllNetworkNodes)
        {
            var node = graph.AddNode(n);
            node.LabelText = n;
            node.Attr.Shape = Shape.Circle;
        }

        foreach (var l in temporalGraph.Links)
        {
            var edge = graph.AddEdge(
                l.NodeA,
                l.NodeB);

            if (l.Direction == LinkDirection.Right)
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;
            else if (l.Direction == LinkDirection.Left)
            {
                edge.Attr.ArrowheadAtSource = ArrowStyle.Normal;
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;

            }
            else if (l.Direction == LinkDirection.Undirected)
            {
                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                edge.Attr.ArrowheadAtSource = ArrowStyle.None;
            }
            else
            {
                graph.RemoveEdge(edge);
            }
        }

        return graph;
    }
}
