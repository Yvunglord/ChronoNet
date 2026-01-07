namespace ChronoNet.Domain.Enums;
[Flags]
public enum GlobalCapabilities
{
    None = 0,
    Compute = 1,
    Storage = 2,
    Transfer = 4
}