namespace ChronoNet.Application.DTO;

public sealed class ElemDto
{
    public int Id { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}