namespace ChronoNet.Domain.Abstractions;
public class VertexBase
{
    public Guid Id { get; }
    public string Name { get; protected set; } = default!;
    public VertexBase(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public virtual void Rename(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }
}