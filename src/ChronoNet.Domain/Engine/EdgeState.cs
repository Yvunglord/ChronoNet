namespace ChronoNet.Domain.Engine;

public sealed class EdgeState
{
    public string From { get; }
    public string To { get; }
    
    public EdgeState(string from, string to)
    {
        From = from;
        To = to;
    }
}
