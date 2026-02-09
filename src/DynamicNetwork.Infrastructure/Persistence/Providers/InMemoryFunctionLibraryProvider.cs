using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Domain.Functions;

namespace DynamicNetwork.Infrastructure.Persistence.Providers;

public class InMemoryFunctionLibraryProvider : IFunctionLibraryProvider
{
    private FunctionLibrary _library;

    public InMemoryFunctionLibraryProvider()
    {
        _library = new FunctionLibrary(
            Enumerable.Empty<ProcessType>(),
            Enumerable.Empty<TransportType>(),
            Enumerable.Empty<StorageType>());
    }

    public InMemoryFunctionLibraryProvider(FunctionLibrary initialLibrary)
    {
        _library = initialLibrary 
            ?? throw new ArgumentNullException(nameof(initialLibrary));
    }

    public FunctionLibrary GetCurrent() => _library;

    public void Update(FunctionLibrary library)
    {
        _library = library ?? throw new ArgumentNullException(nameof(library));
    }
}
