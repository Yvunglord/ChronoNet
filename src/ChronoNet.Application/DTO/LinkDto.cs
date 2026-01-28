namespace ChronoNet.Application.DTO;

public sealed class LinkDto
{
    public int Id1 { get; set; }
    public int Id2 { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}