using ChronoNet.Domain.Enums;

namespace ChronoNet.Domain;
public sealed class Edge
{
    private EdgeDirection _direction;

    public Guid From { get; }
    public Guid To { get; }
    public EdgeDirection Direction
    {
        get => _direction;
        private set => _direction = value;
    }

    public HashSet<int> SupportedTransprtTypes { get; } = new();

    public Edge(Guid from, Guid to, EdgeDirection direction = EdgeDirection.Undirected)
    {
        From = from;
        To = to;
        Direction = direction;
    }

    public void SetDirection(EdgeDirection direction) => _direction = direction;
    public Guid GetSource()
    {
        return _direction switch
        {
            EdgeDirection.Right => From,
            EdgeDirection.Left => To,
            EdgeDirection.Undirected => default,
            _ => throw new InvalidOperationException("Invalid direction state")
        };
    }

    public Guid GetTarget()
    {
        return _direction switch
        {
            EdgeDirection.Right => To,
            EdgeDirection.Left => From,
            EdgeDirection.Undirected => default,
            _ => throw new InvalidOperationException("Invalid direction state")
        };
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Edge);
    }

    public bool Equals(Edge other)
    {
        if (other is null) return false;

        return (From == other.From && To == other.To) ||
               (From == other.To && To == other.From);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }

    public override string ToString()
    {
        string arrow = Direction switch
        {
            EdgeDirection.Undirected => "↔",
            EdgeDirection.Right => "→",
            EdgeDirection.Left => "←",
            _ => "—"
        };

        return $"{From} {arrow} {To}";
    }
}