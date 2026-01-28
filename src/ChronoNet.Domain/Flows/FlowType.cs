namespace ChronoNet.Domain.Flows;

public sealed class FlowType
{
    public int Id { get; }
    public string? Name { get; }

    public FlowType(int id, string? name)
    {
        Id = id;
        Name = name;
    }
}