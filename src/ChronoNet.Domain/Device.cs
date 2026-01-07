using ChronoNet.Domain.Abstractions;
using ChronoNet.Domain.Enums;

namespace ChronoNet.Domain;
public sealed class Device : VertexBase
{
    private GlobalCapabilities _capabilities;

    public GlobalCapabilities Capabilities
    {
        get => _capabilities;
        private set => _capabilities = value;
    }

    public Device(string name, GlobalCapabilities capabilities = GlobalCapabilities.None) : base(name)
    {
        _capabilities = capabilities;
    }

    public void AddCapability(GlobalCapabilities capabilitiy) => _capabilities |= capabilitiy;
    public void RemoveCapability(GlobalCapabilities capability) => _capabilities &= ~capability;
    public bool HasCapability(GlobalCapabilities capabilitiy) => (_capabilities & capabilitiy) == capabilitiy;

    public override string ToString() => Name;
}