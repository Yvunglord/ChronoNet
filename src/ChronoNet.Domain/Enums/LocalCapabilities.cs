namespace ChronoNet.Domain.Enums;
public enum LocalCapabilities
{
    None = 0,
    Compute = 1,
    Storage = 2,
    Transfer = 4,
    CanSend = 8,
    CanReceive = 16
}

