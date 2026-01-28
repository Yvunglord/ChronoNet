namespace ChronoNet.Application.DTO;

public sealed class StructDto
{
    public int Id { get; set; }
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public long Time { get; set; }

    public List<ElemDto> Elements { get; set; } = new();
    public List<LinkDto> Links { get; set; } = new();
}