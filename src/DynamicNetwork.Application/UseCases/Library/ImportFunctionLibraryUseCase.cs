using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.Providers;
using DynamicNetwork.Application.Interfaces.UseCases.Library;

namespace DynamicNetwork.Application.UseCases.Library;

public class ImportFunctionLibraryUseCase : IImportFunctionLibraryUseCase
{
    private readonly IFunctionLibraryFilePort _filePort;
    private readonly IFunctionLibraryProvider _provider;

    public ImportFunctionLibraryUseCase(
    IFunctionLibraryFilePort filePort,
    IFunctionLibraryProvider provider)
    {
        _filePort = filePort;
        _provider = provider;
    }

    public void Execute(string path)
    {
        var library = _filePort.Load(path);

        _provider.Update(library);
    }
}
